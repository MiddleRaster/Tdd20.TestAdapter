## Tdd20.TestAdapter

A Visual Studio Test Explorer Test Adapter for [tdd20-style C++ unit tests](https://github.com/MiddleRaster/tdd20).

### How to use

After creating your Tdd20-style C++ unit test `.exe`, you can either:

1. drop this tiny assembly next to it, or  
2. install the NuGet package into your C++ test project (recommended).

Once installed, your tests will automatically appear in Visual Studio’s Test Explorer.

### NuGet Package

When you build the `Tdd20.TestAdapter` project, the NuGet package is placed in `\LocalNugetFeed`.

To use it in Visual Studio 2026:

1. Go to **Tools → NuGet Package Manager → Package Manager Settings**.  
2. Select **Package Sources** and add the `\LocalNugetFeed` folder as a source.  
3. In your C++ test project, open **Manage NuGet Packages** and install `Tdd20.TestAdapter`.

That’s it — Test Explorer will load the adapter and discover your tests automatically.
