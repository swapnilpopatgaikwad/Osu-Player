﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Milki.OsuPlayer.Data.Models;
using Milki.OsuPlayer.Presentation.Interaction;

namespace Milki.OsuPlayer.ViewModels
{
    public class SelectCollectionPageViewModel : VmBase
    {
        private ObservableCollection<Collection> _collections;
        private IList<Beatmap> _dataList;

        public ObservableCollection<Collection> Collections
        {
            get => _collections;
            set
            {
                _collections = value;
                OnPropertyChanged();
            }
        }

        public IList<Beatmap> DataList
        {
            get => _dataList;
            set
            {
                _dataList = value;
                OnPropertyChanged();
            }
        }
    }
}