// Dll1.cpp : Defines the exported functions for the DLL application.
//

#define DLLEXPORT_API extern "C" _declspec(dllexport)

#include "stdafx.h"
#include <iostream>

using namespace std;

extern "C" _declspec(dllexport) int __stdcall loop_test()
{

	int j = 1;

	for (int i = 0; i < 10000000; i++) {
		j = (i + 5) / j;
	}

	return j;
}

extern "C" _declspec(dllexport) int __stdcall main()
{
	cout << "Hello World\n"; // 输出 Hello World
	return 0;
}

