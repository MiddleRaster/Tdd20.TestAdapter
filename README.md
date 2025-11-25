## Tdd20.TestAdapter

A Visual Studio Test Explorer Test Adapter for [tdd20-style C++ unit tests](https://github.com/MiddleRaster/tdd20).

### How to use

After creating your Tdd20-style C++ unit test .exe, you can either:

1. drop this tiny assembly next to it, or
2. use the nuget package, bu installing it into your .vcxproj file (recommended).

Your tests will show up in Visual Studio's Test Explorer TreeView.

### Nuget Package

When building the Tdd20.TestAdapter project, the NuGet package is dropped in a directory in your root: \\LocalNugetFeed.
To add this feed to Visual Studio 2026, go to Tools->NuGet Package Manager->Package Manager Settings.
Select 'Package Sources' and add the \LocalNugetFeed folder as a source. It will then be available to install into your .vcxproj.
