/*
Copyright (C) 2020-2024 SignatureBeef

This file is part of Open Terraria API v3 (OTAPI)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace OTAPI.Client.Launcher;

public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
        //DataContext = new MessageBoxWindowViewModel();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void OK(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}

public static class MessageBox
{
    public static void Show(string text, string title)
    {
        var wnd = new MessageBoxWindow()
        {
            DataContext = new MessageBoxWindowViewModel()
            {
                Text = text,
                Title = title.ToUpper()
            }
        };
        wnd.Show();
        var app = Application.Current ?? throw new System.Exception("Failed to launch MessageBoxWindow");
        app.Run(wnd);
    }
}

public class MessageBoxWindowViewModel : ReactiveObject
{
    private string? title;
    public string? Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, $"OTAPI Client: {value}");
    }

    private string? text;
    public string? Text
    {
        get => text;
        set => this.RaiseAndSetIfChanged(ref text, value);
    }
}
