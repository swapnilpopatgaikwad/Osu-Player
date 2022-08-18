﻿using Milki.OsuPlayer.Shared.Observable;

namespace Milki.OsuPlayer.ViewModels;

public class ListPageViewModel : VmBase
{
    public ListPageViewModel(int index)
    {
        Index = index;
    }

    public int Index { get; set; }
    public bool IsActivated { get; set; }
}