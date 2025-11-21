export module VsTdd20;

import tdd20;

export class VsTest : private TDD20::Test
{
	static std::string EnforceUniqueness(const std::string& name)
	{
		auto stripOffLine = [](const std::string& name) { return name.substr(0, name.rfind('?')); };
		auto nameWithoutLine = stripOffLine(name);
		auto it = std::find_if(Test::tests.begin(), Test::tests.end(), [&nameWithoutLine, &stripOffLine](const auto& p) { return stripOffLine(p.first) == nameWithoutLine; });
		if (it != Test::tests.end()) 
			throw std::invalid_argument("Duplicate test name: " + name);

		return name;
	}
public:
	VsTest(const std::string& testname, std::function<void()> func, std::source_location loc = std::source_location::current())
		: TDD20::Test(EnforceUniqueness(std::format("{}?{}?{}", testname, loc.file_name(), loc.line())), func)
	{}
};
