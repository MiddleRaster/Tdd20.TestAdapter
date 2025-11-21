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
    // Uncommenting the line above is a problem for VS's Test Explorer UI (but not for vstest.console.exe's UI).
	// So, what I do is check each newly added test against all the existing tests and throw if there's a dupe.
	// Throwing causes VS not to find any tests at all in your .dll (it looks like a dll load failure).
	// If it turns out to be a performance problem (O(n^2)), I could add them blindly, 
	// and then check inside DllMain (during DLL_PROCESS_ATTACH & DLL_THREAD_ATTACH), which is O(log(n)).
	// However, note that throwing from inside DllMain will probably crash the process.
};
