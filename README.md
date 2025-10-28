# Avae.Printables

A crossplatform library for printing. (Supported formats Png,Pdf,Html,Gif,Jpeg,Bmp,Ico)

# Features

Cross-Platform : Leverage APIs adapted for multiple environments.

MIT Licensed: Freely use, modify, and distribute under the permissive MIT License.

# Getting Started

Follow these steps to integrate Avae.Printables into your Avalonia project.

# Prerequisites

An Avalonia project set up with .NET.

# Installation

Add Nuget Avae.Printables to Your Shared Project

# Configuration

Enable Printables.

````
 using Avae.Printables;
 public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePrintables()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
````
Android

````
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        .UsePrintables(this, this)
        .WithInterFont()
        .UseReactiveUI();
}
````

iOS
````
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        .UsePrintables()
        .WithInterFont()
        .UseReactiveUI();
}
````

Browser (Please add nuget package Avae.Printables to project to make sure printing.js in wwwroot folder)

````
private static Task Main(string[] args) => BuildAvaloniaApp()
            .WithInterFont()
             .StartBrowserAppAsync("out").ContinueWith(async t =>
             {
                 await JSHost.ImportAsync("printing", "/printing.js");
             });

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePrintables();
````

Linux (the following workflow is required)

https://github.com/lytico/GtkSharp.Workload

# Example: Print a file

````
using Avae.Printables;

Printable.PrintAsync("C://Temp/test.pdf");

````

# Example: Print MainView

````
using Avae.Printables;

Printable.PrintAsync();

````

# Example: Print visuals

````
using Avae.Printables;

var visual = Application.Current.ApplicationLifetime
            is ISingleViewApplicationLifetime singleViewApplicationLifetime ? singleViewApplicationLifetime.MainView :
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        
var panel = new Grid()
{
    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
};
panel.RowDefinitions.Add(new RowDefinition(GridLength.Star));
panel.Children.Add(new TextBlock()
{
    Text = "Hello",
    FontSize = 60,
    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
});

await Printable.PrintAsync([visual, panel]);
   
````

# Built With

This package builds upon the excellent work of:

AvaloniaUI

SkiaSharp

# License

Avae.Printables is licensed under the MIT License.

# Contributing

Contributions are welcome! Please submit issues or pull requests to the GitHub repository. Ensure your code follows the project’s coding standards.
