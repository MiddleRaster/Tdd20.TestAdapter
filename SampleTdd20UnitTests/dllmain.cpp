import std;
import tdd20;
using namespace TDD20;

#include <windows.h>

BOOL APIENTRY DllMain( HMODULE /*hModule*/, DWORD  ul_reason_for_call, LPVOID )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

static BSTR StringToBstr(const std::string& input)
{
    if (input.empty())
        return SysAllocString(L"");
    int wlen = MultiByteToWideChar(CP_ACP, 0, input.c_str(), static_cast<int>(input.size()), nullptr, 0);
    std::wstring wstr(wlen, L'\0');
               MultiByteToWideChar(CP_ACP, 0, input.c_str(), static_cast<int>(input.size()), &wstr[0], wlen);
    return SysAllocStringLen(wstr.data(), wlen);
}
static std::string BstrToString(BSTR bstr)
{
    int wslen = SysStringLen(bstr);
    int len   = WideCharToMultiByte(CP_ACP, 0, bstr, wslen, nullptr,         0, nullptr, nullptr);
    std::string string(len, '\0');
                WideCharToMultiByte(CP_ACP, 0, bstr, wslen, string.data(), len, nullptr, nullptr);
    return string;
}

extern "C" __declspec(dllexport) int __stdcall RunSingleTest(BSTR bstrTestName, BSTR* bstrFilename, int* pLine, BSTR* bstrMessage)
{
    if (!bstrTestName || !bstrFilename || !pLine || !bstrMessage)
        return E_POINTER;

    class TestMatcher
    {
        const std::string targetName;
    public:
        TestMatcher(const std::string& name) : targetName(name) {}
        bool WantTest(const std::string& name) const { return name == targetName; }
    } matcher(BstrToString(bstrTestName));
    std::ostringstream oss;
    auto [passed, failed] = Test::RunTests(matcher, oss);

    if (passed + failed == 0)
        return ERROR_NOT_FOUND;
    if (failed == 0) {
        *bstrFilename = SysAllocString(L"");
        *pLine        = 0;
        *bstrMessage  = SysAllocString(L"");
        return (!*bstrFilename || !*bstrMessage) ? E_OUTOFMEMORY : S_OK;
    }

    int line = 0;
    std::string filename, message;
    std::istringstream iss(oss.str());
    std::getline(iss, filename, '(');   // Read filename up to '('
    iss >> line;                        // Read line number up to ')'
    iss.ignore(1, ')');                 // skip the ')'
    std::getline(iss, message);         // Read the rest of the line as message

    *bstrFilename = StringToBstr(filename);
    *bstrMessage = StringToBstr(message);
    *pLine      = line;
    return (!*bstrFilename || !*bstrMessage) ? E_OUTOFMEMORY : S_OK;
}
extern "C" __declspec(dllexport) int __stdcall ListAllTests(BSTR* output)
{
    if (!output)
        return E_POINTER;

    struct Dumper
    {
        mutable std::string s;
        bool WantTest(const std::string& name) const { s += name + "\n"; return false; }
    } dumper;
    Test::RunTests(dumper, std::ostringstream{});
    *output = StringToBstr(dumper.s);
    return (!*output) ? E_OUTOFMEMORY : S_OK;
}

#if defined(_M_IX86) // On x86, compiler decorates as _RunSingleTest@16.
#pragma comment(linker, "/EXPORT:RunSingleTest=_RunSingleTest@16")
#pragma comment(linker, "/EXPORT:ListAllTests=_ListAllTests@4")
#endif
