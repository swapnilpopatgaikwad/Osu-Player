﻿using System.Diagnostics;
using Anotar.NLog;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using OsuPlayer.Data.Models;

namespace OsuPlayer.Data;

public sealed partial class ApplicationDbContext
{
    public async ValueTask<PaginationQueryResult<PlayGroupQuery>> SearchPlayItemsAsync(string searchText,
        BeatmapOrderOptions beatmapOrderOptions,
        int page,
        int countPerPage)
    {
        if (page <= 0) page = 1;

        var query = PlayItems
            .AsNoTracking()
            .Include(k => k.PlayItemAsset)
            .Join(PlayItemDetails.Where(GetWhereExpression(searchText)),
                playItem => playItem.PlayItemDetailId,
                playItemDetail => playItemDetail.Id,
                (playItem, playItemDetail) => new
                {
                    PlayItem = playItem,
                    PlayItemDetail = playItemDetail,
                    PlayItemAssets = playItem.PlayItemAsset
                })
            .Select(k => new PlayGroupQuery
            {
                Folder = k.PlayItem.Folder,
                IsAutoManaged = k.PlayItem.IsAutoManaged,
                Artist = k.PlayItemDetail.Artist,
                ArtistUnicode = k.PlayItemDetail.ArtistUnicode,
                Title = k.PlayItemDetail.Title,
                TitleUnicode = k.PlayItemDetail.TitleUnicode,
                Tags = k.PlayItemDetail.Tags,
                Source = k.PlayItemDetail.Source,
                Creator = k.PlayItemDetail.Creator,
                BeatmapSetId = k.PlayItemDetail.BeatmapSetId,
                ThumbPath = k.PlayItemAssets == null ? null : k.PlayItemAssets.ThumbPath,
                StoryboardVideoPath = k.PlayItemAssets == null ? null : k.PlayItemAssets.StoryboardVideoPath,
                VideoPath = k.PlayItemAssets == null ? null : k.PlayItemAssets.VideoPath,
            });

        var sqlStr = query.ToQueryString();
        var fullResult = await query.ToArrayAsync();

        var enumerable = fullResult
            .GroupBy(k => k.Folder, StringComparer.Ordinal)
            .SelectMany(k => k
                .GroupBy(o => o, MetaComparer.Instance)
                .Select(o => o.First())
            );

        enumerable = beatmapOrderOptions switch
        {
            BeatmapOrderOptions.Artist => enumerable.OrderBy(k =>
                    string.IsNullOrEmpty(k.ArtistUnicode) ? k.Artist : k.ArtistUnicode,
                StringComparer.InvariantCultureIgnoreCase),
            BeatmapOrderOptions.Title => enumerable.OrderBy(k =>
                    string.IsNullOrEmpty(k.TitleUnicode) ? k.Title : k.TitleUnicode,
                StringComparer.InvariantCultureIgnoreCase),
            BeatmapOrderOptions.Creator => enumerable.OrderBy(k => k.Creator,
                StringComparer.OrdinalIgnoreCase),
            _ => throw new ArgumentOutOfRangeException(nameof(beatmapOrderOptions), beatmapOrderOptions, null)
        };

        var bufferResult = enumerable.ToArray();
        var totalCount = bufferResult.Length;
        var beatmaps = bufferResult.Skip((page - 1) * countPerPage).Take(countPerPage).ToArray();

        return new PaginationQueryResult<PlayGroupQuery>(beatmaps, totalCount);
    }

    public async ValueTask<PlayList[]> GetCollectionsAsync()
    {
        return await PlayLists
            .AsNoTracking()
            .OrderByDescending(k => k.Index)
            .ToArrayAsync();
    }

    public async ValueTask<PaginationQueryResult<PlayItem>> GetBeatmapsFromCollectionAsync(PlayList playList,
        int page = 0,
        int countPerPage = 50,
        BeatmapOrderOptions options = BeatmapOrderOptions.CreateTime)
    {
        if (playList == null) throw new ArgumentNullException(nameof(playList));
        var relations = PlayListRelations
            .AsNoTracking()
            .Where(k => k.PlayListId == playList.Id);

        var ordered = options switch
        {
            BeatmapOrderOptions.Index => relations.OrderByDescending(k => k.Index),
            BeatmapOrderOptions.CreateTime => relations.OrderByDescending(k => k.CreateTime),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options, null)
        };

        var beatmaps = ordered.Join(PlayItems,
            k => k.PlayItemId,
            k => k.Id,
            (k, x) => x);

        var buffer = await beatmaps.ToArrayAsync();
        var count = buffer.Length;
        var result = buffer.Skip(page * countPerPage).Take(countPerPage).ToArray();

        return new PaginationQueryResult<PlayItem>(result, count);
    }

    public async ValueTask AddCollectionAsync(string name, bool locked = false)
    {
        var maxIndex = await PlayLists
            .OrderByDescending(k => k.Index)
            .Select(k => k.Index)
            .FirstOrDefaultAsync();
        var collection = new PlayList
        {
            Name = name,
            IsDefault = locked,
            Index = maxIndex + 1,
        };

        PlayLists.Add(collection);
        await SaveChangesAsync();
    }

    public async ValueTask DeleteBeatmapsFromCollectionAsync(IEnumerable<PlayItem> beatmaps, PlayList collection)
    {
        var relations = beatmaps
            .Join(PlayListRelations.Where(k => k.PlayListId == collection.Id), k => k.Id,
                k => k.PlayItemId,
                (_, k) => k)
            .ToArray();

        await this.BulkDeleteAsync(relations);
        await this.BulkSaveChangesAsync();
    }

    public async Task<PaginationQueryResult<LoosePlayItem>> GetRecentList(
        int page = 0,
        int countPerPage = 50)
    {
        var buffer = await RecentPlay
            .AsNoTracking()
            .OrderByDescending(k => k.LastPlay)
            .ToArrayAsync();

        var count = buffer.Length;
        var result = buffer
            .Skip(page * countPerPage)
            .Take(countPerPage)
            .ToArray();

        return new PaginationQueryResult<LoosePlayItem>(result, count);
    }

    public async ValueTask AddOrUpdateBeatmapToRecentPlayAsync(PlayItem playItem, DateTime playTime)
    {
        await AddOrUpdateLoosePlayItemCore(playItem, playTime, RecentPlay);
    }

    public async ValueTask AddOrUpdateBeatmapToCurrentPlayAsync(PlayItem playItem, DateTime playTime)
    {
        await AddOrUpdateLoosePlayItemCore(playItem, playTime, CurrentPlay);
    }

    public async ValueTask RecreateCurrentPlayAsync(IEnumerable<PlayItem> playItems)
    {
        var sw = Stopwatch.StartNew();
        var dbItems = await CurrentPlay
            .ToDictionaryAsync(k => k.PlayItemId, k => k);

        LogTo.Debug(() => $"Found {dbItems.Count} LooseItems in {sw.ElapsedMilliseconds}ms.");
        sw.Restart();

        var newAllLooseItems = playItems
            .Select(k => new KeyValuePair<int, LoosePlayItem>(k.Id, k.ToLoosePlayItem(DateTime.MinValue)))
            .ToDictionary(k => k.Key, k => k.Value);

        LogTo.Debug(() => $"Enumerate {dbItems.Count} newAllLooseItems in {sw.ElapsedMilliseconds}ms.");
        sw.Restart();

        // Delete obsolete
        var obsoleteNeedDel = dbItems
            .Where(k => !newAllLooseItems.ContainsKey(k.Key.Value))
            .Select(k => k.Value)
            .ToList();

        LogTo.Debug(() => $"Found {obsoleteNeedDel.Count} LooseItems to delete in {sw.ElapsedMilliseconds}ms.");
        sw.Restart();
        if (obsoleteNeedDel.Count > 0)
        {
            await this.BulkDeleteAsync(obsoleteNeedDel);
            await this.BulkSaveChangesAsync();

            LogTo.Debug(() => $"Delete {dbItems.Count} LooseItems in {sw.ElapsedMilliseconds}ms.");
            sw.Restart();
        }

        // Update exist
        var existNeedUpdate = dbItems
            .Select((k, i) =>
            {
                if (!newAllLooseItems.TryGetValue(k.Key.Value, out var newItem)) return null!;
                var oldItem = k.Value;
                oldItem.Artist = newItem.Artist;
                oldItem.Title = newItem.Title;
                oldItem.Creator = newItem.Creator;
                oldItem.Version = newItem.Version;
                oldItem.Index = newItem.Index;
                return newItem;
            })
            .Where(k => k != null!)
            .ToArray();

        LogTo.Debug(() => $"Found {existNeedUpdate.Length} LooseItems to update in {sw.ElapsedMilliseconds}ms.");
        sw.Restart();

        if (existNeedUpdate.Length > 0)
        {
            var actualUpdated = await this.SaveChangesAsync();
            LogTo.Debug(() => $"Update {actualUpdated} LooseItems in {sw.ElapsedMilliseconds}ms.");
            sw.Restart();
        }

        // Add new
        var listItem = newAllLooseItems
            .Where(k => !dbItems.ContainsKey(k.Key))
            .Select(playItemDetail => playItemDetail.Value)
            .ToList();

        LogTo.Debug(() => $"Found {listItem.Count} LooseItems to Add in {sw.ElapsedMilliseconds}ms.");
        sw.Restart();

        if (listItem.Count > 0)
        {
            await this.BulkInsertAsync(listItem, k => k.CustomDestinationTableName = nameof(CurrentPlay));
            await this.BulkSaveChangesAsync();

            LogTo.Debug(() => $"Add {listItem.Count} LooseItems in {sw.ElapsedMilliseconds}ms.");
            sw.Restart();
        }
    }

    public async Task<PaginationQueryResult<ExportItem>> GetExportList(
        int page = 0,
        int countPerPage = 50)
    {
        var buffer = await Exports
            .AsNoTracking()
            .OrderByDescending(k => k.ExportTime)
            .ToArrayAsync();

        var count = buffer.Length;

        var result = buffer
            .Skip(page * countPerPage)
            .Take(countPerPage)
            .ToArray();

        return new PaginationQueryResult<ExportItem>(result, count);
    }

    private async ValueTask AddOrUpdateLoosePlayItemCore(PlayItem playItem, DateTime playTime,
        DbSet<LoosePlayItem> loosePlayItems)
    {
        var loosePlayItem = await loosePlayItems
            .FirstOrDefaultAsync(k => k.PlayItemId == playItem.Id);
        if (loosePlayItem == null)
        {
            loosePlayItems.Add(playItem.ToLoosePlayItem(playTime));
        }
        else
        {
            loosePlayItem.UpdateFromPlayItem(playItem, playTime);
        }

        await SaveChangesAsync();
    }
}