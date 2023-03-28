#include <iostream>
#include <stdint.h>
#include "strconv.h"

extern "C" __declspec(dllexport)
int32_t add2(int32_t a, int32_t b)
{
	std::cout << a << "+" << b << std::endl;
	return a + b;
}

extern "C" __declspec(dllexport)
void helloW(const wchar_t *msg)
{
	dbgout(std::cout, L"msg=%s\n", msg);
}

extern "C" __declspec(dllexport)
const wchar_t *greetingW(const wchar_t *name)
{
	static thread_local std::wstring result;
	result = format(L"Hello %s!", name);
	return result.c_str();
}
