#include "dm.tlh"
#include <iostream>

/// <summary>
/// 初始化大漠对象 (免注册大漠com, 直接加载dll)
/// https://blog.csdn.net/chuhe163/article/details/112745590
/// </summary>
/// <returns></returns>
Idmsoft* GetDmObject()
{
	Idmsoft* dmObj = NULL;

	//直接加载dll创建对象，避免进行注册文件
	typedef HRESULT(_stdcall* pfnGCO)(REFCLSID, REFIID, void**);
	pfnGCO fnGCO = NULL;
	auto dmPath = L"dm/dm.dll";
	// 加载大漠的dll
	HINSTANCE hdllInst = LoadLibrary(dmPath);

	if (hdllInst == 0)
	{
		std::cout << "未找到-> " << dmPath << std::endl;
		return NULL;
	}

	// 获取dll中的函数
	fnGCO = (pfnGCO)GetProcAddress(hdllInst, "DllGetClassObject");

	if (fnGCO != 0)
	{
		IClassFactory* classFactory = NULL;

		HRESULT hResult = (fnGCO)(__uuidof(dmsoft), IID_IClassFactory, (void**)&classFactory);

		if (SUCCEEDED(hResult) && (classFactory != NULL))
		{
			// 创建对象
			hResult = classFactory->CreateInstance(NULL, __uuidof(Idmsoft), (void**)&dmObj);

			if ((SUCCEEDED(hResult) && (dmObj != NULL)) == FALSE)
			{
				return NULL;
			}

		}

		classFactory->Release();
	}

	return dmObj;
}