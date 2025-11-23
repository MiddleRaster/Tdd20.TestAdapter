import std;
import tdd20;
using namespace TDD20;

import VsTdd20;

VsTest additionalTests[] = {
	{ std::string{"Additional Test 1"}, []() { Assert::AreEqual(2 + 2, 4); } },
	{ std::string{"Additional Test 2"}, []() { Assert::IsTrue(true, "This is not a failing test"); }},
	{ "Identically named test, but different file", []()
		{
			Assert::IsTrue(true);
			Assert::IsFalse(false);
			Assert::AreEqual(1, 1);
			Assert::Fail("Yet another failure");
		}
	},
//  { "Identically named test, but different file", []() { Assert::IsFalse(false); }},
    // Uncommenting the line above is a problem for VS's Test Explorer UI (but not for vstest.console.exe's UI):
	// The two tests get merged onto a single node in the Test Explorer treeview.
	// I could throw an exception, but that prevents the .dll from loading at all, which is confusing to the user.
	// So I decided to write an error message write into the Test Explorer treeview. Try it out by uncommenting the last test, above.
};
