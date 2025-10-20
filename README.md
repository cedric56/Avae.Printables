# Avae.Linux.Essentials

A port of Microsoft.Maui.Essentials tailored for Avalonia.Linux, bringing essential cross-platform APIs to your Avalonia-based linux applications. This package simplifies access to common device and application features.

# Features

Cross-Platform Essentials: Leverage APIs from Microsoft.Maui.Essentials adapted for linux environments.

MIT Licensed: Freely use, modify, and distribute under the permissive MIT License.

# Getting Started

Follow these steps to integrate Avae.Linux.Essentials into your Avalonia.Linux project.

# Prerequisites

An Avalonia.Linux project set up with .NET.

# Installation

1. Add Microsoft.Maui.Essentials to Your Shared Project

In your shared project’s .csproj file, include the Microsoft.Maui.Essentials package. Use one of the following methods
````
<UseMauiEssentials>true</UseMauiEssentials>
````
OR
````
<PackageReference Include="Microsoft.Maui.Essentials">
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
````

# Configuration

1. Enable TextToSpeech

````
 sudo apt install espeak -y
````

2. Enable Geolocation

````
 sudo apt install geoclue-2.0 -y
````

3. Enable MediaPicker, you must have libcvextern.so. Consider is in an Native folder

````
 using Microsoft.Maui.ApplicationModel;
 public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseMauiEssentials((architecture) => "YourProject.Native.libcvextern.so")
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
````

# Usage

Once installed, you can use Microsoft.Maui.Essentials APIs within your Avalonia.Linux application. For example, access application information via AppInfo.

# Example: Accessing AppInfo
````
using Microsoft.Maui.Essentials;

string appName = AppInfo.Name;
string appVersion = AppInfo.VersionString;
````
# Built With

This package builds upon the excellent work of:

Microsoft.Maui.Essentials

AvaloniaUI

Emgu.CV

SecureLocalStorage

VCardParser

TmdsDBus

# License

Avae.Linux.Essentials is licensed under the MIT License.

# Contributing

Contributions are welcome! Please submit issues or pull requests to the GitHub repository. Ensure your code follows the project’s coding standards.

# Acknowledgments

Thanks to the Avalonia team for their robust UI framework.

Gratitude to the MAUI team for providing cross-platform essentials.