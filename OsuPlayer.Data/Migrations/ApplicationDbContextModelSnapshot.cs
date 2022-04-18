﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OsuPlayer.Data;

#nullable disable

namespace OsuPlayer.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.4");

            modelBuilder.Entity("OsuPlayer.Data.Models.ExportItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("Creator")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<string>("ExportPath")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<long>("ExportTime")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("PlayItemId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayItemPath")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ExportList");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.LoosePlayItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("Creator")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<int>("Index")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LastPlay")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("PlayItemId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayItemPath")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Index");

                    b.HasIndex("LastPlay");

                    b.ToTable("CurrentPlaying");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Folder")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAutoManaged")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("LastPlay")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<int?>("PlayItemAssetId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("PlayItemConfigId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayItemDetailId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Folder");

                    b.HasIndex("Path")
                        .IsUnique();

                    b.HasIndex("PlayItemAssetId");

                    b.HasIndex("PlayItemConfigId");

                    b.HasIndex("PlayItemDetailId");

                    b.ToTable("PlayItems");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayItemAsset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("StoryboardVideoPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoPath")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PlayItemAssets");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayItemConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("LyricOffset")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Offset")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PlayItemConfigs");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayItemDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("ArtistUnicode")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("AudioFileName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("BeatmapFileName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<int>("BeatmapId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BeatmapSetId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Creator")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<long>("DefaultStarRatingCtB")
                        .HasColumnType("INTEGER");

                    b.Property<long>("DefaultStarRatingMania")
                        .HasColumnType("INTEGER");

                    b.Property<long>("DefaultStarRatingStd")
                        .HasColumnType("INTEGER");

                    b.Property<long>("DefaultStarRatingTaiko")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FolderName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("TitleUnicode")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<long>("TotalTime")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UpdateTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Artist");

                    b.HasIndex("ArtistUnicode");

                    b.HasIndex("Creator");

                    b.HasIndex("Source");

                    b.HasIndex("Tags");

                    b.HasIndex("Title");

                    b.HasIndex("TitleUnicode");

                    b.HasIndex("Artist", "ArtistUnicode", "Title", "TitleUnicode", "Creator", "Source", "Tags");

                    b.ToTable("PlayItemDetails");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("CreateTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("ImagePath")
                        .HasColumnType("TEXT");

                    b.Property<int>("Index")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("UpdateTime")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Index");

                    b.ToTable("PlayLists");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CreateTime = 0L,
                            Index = 0,
                            IsDefault = true,
                            Name = "Favorite",
                            UpdateTime = 0L
                        });
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayListPlayItemRelation", b =>
                {
                    b.Property<int>("PlayItemId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayListId")
                        .HasColumnType("INTEGER");

                    b.HasKey("PlayItemId", "PlayListId");

                    b.HasIndex("PlayListId");

                    b.ToTable("PlayListRelations");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.SoftwareState", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("IgnoredVersion")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<long?>("LastSync")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("LastUpdateCheck")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MinimalWindowPosition")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("MinimalWindowWorkingArea")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<bool>("ShowFullNavigation")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ShowMinimalWindow")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ShowWelcome")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("SoftwareStates");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayItem", b =>
                {
                    b.HasOne("OsuPlayer.Data.Models.PlayItemAsset", "PlayItemAsset")
                        .WithMany()
                        .HasForeignKey("PlayItemAssetId");

                    b.HasOne("OsuPlayer.Data.Models.PlayItemConfig", "PlayItemConfig")
                        .WithMany()
                        .HasForeignKey("PlayItemConfigId");

                    b.HasOne("OsuPlayer.Data.Models.PlayItemDetail", "PlayItemDetail")
                        .WithMany()
                        .HasForeignKey("PlayItemDetailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlayItemAsset");

                    b.Navigation("PlayItemConfig");

                    b.Navigation("PlayItemDetail");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayListPlayItemRelation", b =>
                {
                    b.HasOne("OsuPlayer.Data.Models.PlayItem", "PlayItem")
                        .WithMany("PlayListRelations")
                        .HasForeignKey("PlayItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OsuPlayer.Data.Models.PlayList", "PlayList")
                        .WithMany("PlayListRelations")
                        .HasForeignKey("PlayListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlayItem");

                    b.Navigation("PlayList");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayItem", b =>
                {
                    b.Navigation("PlayListRelations");
                });

            modelBuilder.Entity("OsuPlayer.Data.Models.PlayList", b =>
                {
                    b.Navigation("PlayListRelations");
                });
#pragma warning restore 612, 618
        }
    }
}
