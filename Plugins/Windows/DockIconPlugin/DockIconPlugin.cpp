#include <array>
#include <string>
#include <windows.h>
#include <shellapi.h>
#include <propkey.h>
#include <propsys.h>
#include <propvarutil.h>

#define DLL_EXPORT __declspec(dllexport)
#define STDCALL __stdcall

extern "C"
{
    DLL_EXPORT void STDCALL SetIcon(HWND hWnd, HICON hSmallIcon, HICON hBigIcon)
    {
        SendMessage(hWnd, WM_SETICON, ICON_SMALL, reinterpret_cast<LPARAM>(hSmallIcon));
        SendMessage(hWnd, WM_SETICON, ICON_BIG, reinterpret_cast<LPARAM>(hBigIcon));
    }

    DLL_EXPORT void STDCALL DeleteIcon(HICON hIcon)
    {
        if (hIcon != nullptr)
        {
            DestroyIcon(hIcon);
        }
    }

    DLL_EXPORT void STDCALL SetAppId(HWND hWnd, const wchar_t* appId)
    {
        IPropertyStore* pPropStore = nullptr;
        auto hr = SHGetPropertyStoreForWindow(hWnd, IID_PPV_ARGS(&pPropStore));
        if (SUCCEEDED(hr))
        {
            PROPVARIANT propVar;

            hr = InitPropVariantFromString(appId, &propVar);
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

    DLL_EXPORT void STDCALL ClearAppId(HWND hWnd)
    {
        IPropertyStore* pPropStore = nullptr;
        auto hr = SHGetPropertyStoreForWindow(hWnd, IID_PPV_ARGS(&pPropStore));
        if (SUCCEEDED(hr))
        {
            PROPVARIANT propVar;
            PropVariantInit(&propVar);

            hr = pPropStore->SetValue(PKEY_AppUserModel_ID, propVar);
            if (SUCCEEDED(hr))
            {
                pPropStore->Commit();
            }
            pPropStore->Release();
        }
    }

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
}