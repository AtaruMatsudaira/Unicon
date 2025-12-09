#include <array>
#include <string>
#include <windows.h>
#include <shellapi.h>
#include <propkey.h>
#include <propsys.h>
#include <propvarutil.h>

#define DLL_EXPORT __declspec(dllexport)
#define STDCALL __stdcall

std::wstring g_appId;
HICON g_smallIcon = nullptr;
HICON g_bigIcon = nullptr;
HHOOK g_hHook = nullptr;

void SetIcon(HWND hWnd, HICON hSmallIcon, HICON hBigIcon)
{
    SendMessage(hWnd, WM_SETICON, ICON_SMALL, reinterpret_cast<LPARAM>(hSmallIcon));
    SendMessage(hWnd, WM_SETICON, ICON_BIG, reinterpret_cast<LPARAM>(hBigIcon));
}

void SetAppId(HWND hWnd, const std::wstring& appId)
{
    IPropertyStore* pPropStore = nullptr;
    auto hr = SHGetPropertyStoreForWindow(hWnd, IID_PPV_ARGS(&pPropStore));
    if (SUCCEEDED(hr))
    {
        PROPVARIANT propVar;
        if (appId.empty())
        {
            PropVariantInit(&propVar);
        }
        else
        {
            hr = InitPropVariantFromString(appId.c_str(), &propVar);
        }

        if (SUCCEEDED(hr))
        {
            hr = pPropStore->SetValue(PKEY_AppUserModel_ID, propVar);
            if (SUCCEEDED(hr))
            {
                pPropStore->Commit();
            }
            PropVariantClear(&propVar);
        }
        pPropStore->Release();
    }
}

LRESULT CALLBACK EditorWindowProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode == HCBT_CREATEWND)
    {
        auto hWnd = reinterpret_cast<HWND>(wParam);

        auto hasIcon = (g_smallIcon != nullptr) || (g_bigIcon != nullptr);
        if (hasIcon)
        {
            SetIcon(hWnd, g_smallIcon, g_bigIcon);
        }

        auto hasAppId = !g_appId.empty();
        if (hasAppId)
        {
            SetAppId(hWnd, g_appId);
        }
    }

    return CallNextHookEx(g_hHook, nCode, wParam, lParam);
}

struct EnumWindowParam
{
    DWORD targetProcessId;
    std::wstring appId;
    HICON hSmallIcon;
    HICON hBigIcon;
};

BOOL CALLBACK ApplyToWindowProc(HWND hWnd, LPARAM lParam)
{
    auto params = reinterpret_cast<EnumWindowParam*>(lParam);
    DWORD processId = 0;
    GetWindowThreadProcessId(hWnd, &processId);

    if (processId == params->targetProcessId && IsWindowVisible(hWnd))
    {
        SetIcon(hWnd, params->hSmallIcon, params->hBigIcon);
        SetAppId(hWnd, params->appId);
    }

    return TRUE;
}

void StartHook()
{
    if (g_hHook != nullptr)
    {
        return;
    }

    g_hHook = SetWindowsHookEx(WH_CBT, EditorWindowProc, nullptr, GetCurrentThreadId());
}

void StopHook()
{
    if (g_hHook != nullptr)
    {
        UnhookWindowsHookEx(g_hHook);
        g_hHook = nullptr;
    }
}

extern "C"
{
    DLL_EXPORT HICON STDCALL ExtractIconFromPath(const wchar_t* path, int size)
    {
        std::array<HICON, 1> icons = { nullptr };
        std::array<UINT, 1> iconIds = { 0 };

        auto count = PrivateExtractIcons(path,0,size,size,icons.data(),iconIds.data(),1, 0);
        if (count > 0 && icons[0] != nullptr)
        {
            return icons[0];
        }

        return nullptr;
    }

    DLL_EXPORT void STDCALL ApplyToProcessWindows(int processId, const wchar_t* appId, HICON hSmallIcon, HICON hBigIcon)
    {
        EnumWindowParam params{};
        params.targetProcessId = static_cast<DWORD>(processId);
        params.appId = appId != nullptr ? appId : L"";
        params.hSmallIcon = hSmallIcon;
        params.hBigIcon = hBigIcon;

        EnumWindows(ApplyToWindowProc, reinterpret_cast<LPARAM>(&params));

        if (appId == nullptr)
        {
            g_appId.clear();
        }
        else
        {
            g_appId = appId;
        }

        if (g_smallIcon != hSmallIcon)
        {
            if (g_smallIcon != nullptr)
            {
                DestroyIcon(g_smallIcon);
            }
            g_smallIcon = hSmallIcon;
        }

        if (g_bigIcon != hBigIcon)
        {
            if (g_bigIcon != nullptr)
            {
                DestroyIcon(g_bigIcon);
            }
            g_bigIcon = hBigIcon;
        }
    }

    DLL_EXPORT void STDCALL DeleteIcon(HICON hIcon)
    {
        if (hIcon != nullptr)
        {
            DestroyIcon(hIcon);
        }
    }

    DLL_EXPORT void STDCALL InitializeUniconPlugin()
    {
        StartHook();
    }

    DLL_EXPORT void STDCALL DestroyUniconPlugin()
    {
        auto processId = GetCurrentProcessId();
        ApplyToProcessWindows(static_cast<int>(processId), L"", nullptr, nullptr);

        StopHook();
    }
}