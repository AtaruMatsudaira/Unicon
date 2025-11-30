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
}