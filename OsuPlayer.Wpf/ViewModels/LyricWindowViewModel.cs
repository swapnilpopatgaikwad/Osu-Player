﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Milki.OsuPlayer.Common;
using Milki.OsuPlayer.Common.Configuration;
using Milki.OsuPlayer.Media.Audio;
using Milki.OsuPlayer.Presentation.Interaction;
using Milki.OsuPlayer.Shared.Dependency;
using Milki.OsuPlayer.Utils;

namespace Milki.OsuPlayer.ViewModels
{
    public class LyricWindowViewModel : VmBase
    {
        private bool _showFrame;
        private bool _isLyricEnabled;
        private object _fontFamily;
        private double _hue;
        private double _saturation;
        private double _lightness;

        public ObservablePlayController Controller { get; } = Service.Get<ObservablePlayController>();
        public SharedVm Shared { get; } = SharedVm.Default;

        public bool ShowFrame
        {
            get => _showFrame;
            set
            {
                _showFrame = value;
                OnPropertyChanged();
            }
        }


        public bool IsLyricWindowShown
        {
            get => _isLyricEnabled;
            set
            {
                _isLyricEnabled = value;
                OnPropertyChanged();
            }
        }

        public object FontFamily
        {
            get => _fontFamily;
            set
            {
                if (Equals(value, _fontFamily)) return;
                _fontFamily = value;
                AppSettings.Default.Lyric.FontFamily = _fontFamily?.ToString();
                AppSettings.SaveDefault();
                OnPropertyChanged();
            }
        }

        public ICollection<FontFamily> AllFontFamilies { get; } =
            new SortedSet<FontFamily>(Fonts.SystemFontFamilies.Concat(new[]
            {
                (FontFamily) Application.Current.FindResource("SspRegular")
            }), new FontFamilyComparer());

        public double Hue
        {
            get => _hue;
            set
            {
                if (value.Equals(_hue)) return;
                _hue = value;
                OnPropertyChanged();
            }
        }

        public double Saturation
        {
            get => _saturation;
            set
            {
                if (value.Equals(_saturation)) return;
                _saturation = value;
                OnPropertyChanged();
            }
        }

        public double Lightness
        {
            get => _lightness;
            set
            {
                if (value.Equals(_lightness)) return;
                _lightness = value;
                OnPropertyChanged();
            }
        }
    }
}