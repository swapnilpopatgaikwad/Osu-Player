using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xaml;
using Milki.OsuPlayer.Common;
using Milki.OsuPlayer.Data;
using Milki.OsuPlayer.Data.Models;
using Milki.OsuPlayer.Media.Audio;
using Milki.OsuPlayer.Pages;
using Milki.OsuPlayer.Presentation;
using Milki.OsuPlayer.Presentation.Interaction;
using Milki.OsuPlayer.Presentation.ObjectModel;
using Milki.OsuPlayer.Shared.Dependency;
using Milki.OsuPlayer.Shared.Models;
using Milki.OsuPlayer.UiComponents.FrontDialogComponent;
using Milki.OsuPlayer.UiComponents.NotificationComponent;
using Milki.OsuPlayer.UiComponents.PanelComponent;
using Milki.OsuPlayer.UserControls;
using Milki.OsuPlayer.Windows;

namespace Milki.OsuPlayer.ViewModels
{
    public class SearchPageViewModel : VmBase
    {
        private const int MaxListCount = 100;

        private ObservableCollection<OrderedModel<Beatmap>> _dataList;
        private List<ListPageViewModel> _pages;
        private ListPageViewModel _lastPage;
        private ListPageViewModel _firstPage;
        private ListPageViewModel _currentPage;
        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (Equals(value, _searchText)) return;
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<OrderedModel<Beatmap>> DataList
        {
            get => _dataList;
            private set
            {
                if (Equals(value, _dataList)) return;
                _dataList = value;
                OnPropertyChanged();
            }
        }

        public List<ListPageViewModel> Pages
        {
            get => _pages;
            private set
            {
                if (Equals(value, _pages)) return;
                _pages = value;
                OnPropertyChanged();
            }
        }

        public ListPageViewModel LastPage
        {
            get => _lastPage;
            private set
            {
                if (Equals(value, _lastPage)) return;
                _lastPage = value;
                OnPropertyChanged();
            }
        }

        public ListPageViewModel FirstPage
        {
            get => _firstPage;
            private set
            {
                if (Equals(value, _firstPage)) return;
                _firstPage = value;
                OnPropertyChanged();
            }
        }

        public ListPageViewModel CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (Equals(value, _currentPage)) return;
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public VirtualizingGalleryWrapPanel GalleryWrapPanel { get; set; }

        private readonly Stopwatch _querySw = new Stopwatch();
        private bool _isQuerying;
        private static readonly object QueryLock = new object();

        public async Task PlayListQueryAsync(int page = 0)
        {
            var sortMode = BeatmapOrderOptions.Artist;
            _querySw.Restart();

            lock (QueryLock)
            {
                if (_isQuerying)
                    return;
                _isQuerying = true;
            }

            try
            {
                await Task.Run(() =>
                {
                    while (_querySw.ElapsedMilliseconds < 300)
                        Thread.Sleep(10);
                    _querySw.Stop();
                });

                await using var dbContext = new ApplicationDbContext();
                var paginationQueryResult = await dbContext
                    .SearchBeatmapByOptions(SearchText,
                        sortMode,
                        page: page,
                        countPerPage: MaxListCount
                    );

                var result = await dbContext.FillBeatmapThumbs(paginationQueryResult.Collection);
                DataList = new ObservableCollection<OrderedModel<Beatmap>>(result.AsOrdered());
                SetPage(paginationQueryResult.Count, page);
            }
            finally
            {
                lock (QueryLock)
                {
                    _isQuerying = false;
                }
            }
        }

        private void SetPage(int totalCount, int nowPage)
        {
            var totalPageCount = (int)Math.Ceiling(totalCount / (float)MaxListCount);
            int count, startIndex;
            if (totalPageCount > 10)
            {
                if (nowPage > 5)
                {
                    FirstPage = new ListPageViewModel(1);
                    if (nowPage >= totalPageCount - 5)
                    {
                        startIndex = totalPageCount - 10;
                    }
                    else
                    {
                        startIndex = nowPage - 5;
                    }
                }
                else
                {
                    startIndex = 0;
                }

                count = 10;
            }
            else
            {
                count = totalPageCount;
                startIndex = 0;
            }

            var pages = new List<ListPageViewModel>(totalPageCount);
            for (int i = startIndex; i < startIndex + count; i++)
            {
                pages.Add(new ListPageViewModel(i + 1));
            }

            Pages = pages;
            ListPageViewModel page = GetPage(nowPage + 1);

            if (page != null)
                page.IsActivated = true;

            CurrentPage = page;
            GalleryWrapPanel?.ClearNotificationCount();

            //DisplayedMaps = SearchedMaps.Skip(nowIndex * MaxListCount).Take(MaxListCount).ToList();
        }

        private ListPageViewModel GetPage(int page)
        {
            return Pages.FirstOrDefault(k => k.Index == page);
        }

        public ICommand SwitchCommand
        {
            get
            {
                return new DelegateCommand(obj =>
                {
                    if (obj is bool b)
                    {
                        if (CurrentPage == null) return;
                        var page = b ? GetPage(CurrentPage.Index + 1) : GetPage(CurrentPage.Index - 1);
                        if (page == null) return;
                        if (page.IsActivated)
                        {
                            return;
                        }

                        SetPage(DataList.Count, page.Index - 1);
                    }
                    else
                    {
                        var reqPage = (int)obj;
                        var page = GetPage(reqPage);
                        if (page.IsActivated)
                        {
                            return;
                        }

                        SetPage(DataList.Count, reqPage - 1);
                    }
                });
            }
        }

        public ICommand SearchByConditionCommand
        {
            get
            {
                return new DelegateCommand(param =>
                {
                    WindowEx.GetCurrentFirst<MainWindow>()
                        .SwitchSearch
                        .CheckAndAction(page => ((SearchPage)page).Search((string)param));
                });
            }
        }

        public ICommand OpenSourceFolderCommand
        {
            get
            {
                return new DelegateCommand(async param =>
                {
                    var beatmap = (Beatmap)param;
                    var map = await GetHighestSrBeatmap(beatmap);
                    if (map == null) return;
                    var folderName = beatmap.GetFolder(out _, out _);
                    if (!Directory.Exists(folderName))
                    {
                        Notification.Push(@"所选文件不存在，可能没有及时同步。请尝试手动同步osuDB后重试。");
                        return;
                    }

                    Process.Start(folderName);
                });
            }
        }

        public ICommand OpenScorePageCommand
        {
            get
            {
                return new DelegateCommand(async param =>
                {
                    var beatmap = (Beatmap)param;
                    var map = await GetHighestSrBeatmap(beatmap);
                    if (map == null) return;
                    Process.Start($"https://osu.ppy.sh/s/{map.BeatmapSetId}");
                });
            }
        }

        public ICommand SaveCollectionCommand
        {
            get
            {
                return new DelegateCommand(async param =>
                {
                    var beatmap = (Beatmap)param;

                    await using var dbContext = new ApplicationDbContext();
                    var beatmaps = await dbContext.GetBeatmapsFromFolder(beatmap.FolderNameOrPath, beatmap.InOwnDb);

                    var control = new DiffSelectControl(
                        beatmaps, (selected, arg) =>
                        {
                            arg.Handled = true;
                            FrontDialogOverlay.Default.ShowContent(
                                new SelectCollectionControl(selected),
                                DialogOptionFactory.SelectCollectionOptions
                            );
                        });
                    FrontDialogOverlay.Default.ShowContent(control, DialogOptionFactory.DiffSelectOptions);
                });
            }
        }

        public ICommand ExportCommand
        {
            get
            {
                return new DelegateCommand(async param =>
                {
                    var beatmap = (Beatmap)param;
                    var map = await GetHighestSrBeatmap(beatmap);
                    if (map == null) return;
                    ExportPage.QueueBeatmap(map);
                });
            }
        }

        public ICommand DirectPlayCommand
        {
            get
            {
                return new DelegateCommand(async param =>
                {
                    var beatmap = (Beatmap)param;
                    var map = await GetHighestSrBeatmap(beatmap);
                    if (map == null) return;
                    var controller = Service.Get<ObservablePlayController>();
                    await controller.PlayNewAsync(map);
                });
            }
        }

        public ICommand PlayCommand
        {
            get
            {
                return new DelegateCommand(async param =>
                {
                    var beatmap = (Beatmap)param;
                    await using var dbContext = new ApplicationDbContext();
                    var beatmaps = await dbContext.GetBeatmapsFromFolder(beatmap.FolderNameOrPath, beatmap.InOwnDb);

                    var control = new DiffSelectControl(
                        beatmaps, async (selected, arg) =>
                        {
                            var controller = Service.Get<ObservablePlayController>();
                            await controller.PlayNewAsync(selected, true);
                        });
                    FrontDialogOverlay.Default.ShowContent(control, DialogOptionFactory.DiffSelectOptions);
                });
            }
        }

        private async Task<Beatmap> GetHighestSrBeatmap(Beatmap beatmap)
        {
            if (beatmap == null) return null;

            await using var dbContext = new ApplicationDbContext();
            var map = (await dbContext.GetBeatmapsFromFolder(beatmap.FolderNameOrPath, beatmap.InOwnDb))
                .GetHighestDiff();
            return map;
        }
    }

    [MarkupExtensionReturnType(typeof(ContentControl))]
    public class RootObject : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
            return rootObjectProvider?.RootObject;
        }
    }
}