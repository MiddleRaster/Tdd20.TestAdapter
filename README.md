## Tdd20.TestAdapter

A Visual Studio Test Explorer Test Adapter for [tdd20-style C++ unit tests](https://github.com/MiddleRaster/tdd20).

### How to use

I say, "style", because Visual Studio's Test Explorer expects a little more information about the tests than Tdd20 provides.
In particular, VS also wants to know what file and line number a test method *starts* on.
That info is provided by a small module, VsTdd20.ixx; once that is imported you can write tests like this:

```cpp
import std;
import tdd20;
import VsTdd20;
using namespace TDD20;

VsTest tests[] =
{
	{ "Sample Test 1", []() { Assert::AreEqual(2 + 2, 4); } },
	{ "Sample Test 2", []() { Assert::IsTrue(false, "This is a failing test"); }},
	{ "Identically named test, but different file", []()
		{
			Assert::IsTrue(true);
			Assert::IsFalse(false);
			Assert::AreEqual(1, 1);
		}
	},
};

```

Then after having created your Tdd20-style C++ unit test `.exe`, you can either:

1. drop this tiny assembly next to it, or  
2. add the NuGet package to your C++ project (recommended).

Once installed, your tests will automatically appear in Visual Studio’s Test Explorer.

### NuGet Package

When you build the `Tdd20.TestAdapter` project, the NuGet package is placed in `\LocalNugetFeed`.

To use it in Visual Studio 2026:

1. Go to **Tools → NuGet Package Manager → Package Manager Settings**.  
2. Select **Package Sources** and add the `\LocalNugetFeed` folder as a source.  
3. In your C++ test project, open **Manage NuGet Packages** and install `Tdd20.TestAdapter`.

That’s it — Test Explorer will load the adapter and discover your tests automatically.
