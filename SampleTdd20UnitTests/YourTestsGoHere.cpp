import std;
import tdd20;
using namespace TDD20;

import VsTdd20;

VsTest tests[] =
{
	{ std::string{"Sample Test 1"}, []() { Assert::AreEqual(2 + 2, 4); } },
	{ std::string{"Sample Test 2"}, []() { Assert::IsTrue(false, "This is a failing test"); }},
	{ "Identically named test, but different file", []()
		{
			Assert::IsTrue(true);
			Assert::IsFalse(false);
			Assert::AreEqual(1, 1);
		}
	},
};
