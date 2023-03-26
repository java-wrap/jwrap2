#ifndef JNIUTIL_H
#define JNIUTIL_H

//#include <QObject>
#include <string>
#include <vector>

class JniUtil
{
    //Q_OBJECT
public:
    JniUtil();
    //static bool RunClassMain(const std::wstring &jvmDll, const std::wstring &mainClass, const std::vector<std::wstring> &args, const std::vector<std::wstring> &classPaths);
    //static bool RunClassMain(const std::wstring &jvmDll, const std::wstring &mainClass, const std::vector<std::wstring> &args, const std::string bootJar);
    //static std::wstring RunClassMain(const std::wstring &jvmDll, const std::wstring &mainClass, const std::vector<std::wstring> &args, const std::wstring &bootJarPath, const std::string bootJar);
    static std::wstring RunClassMain(const std::wstring &xml);
};

#endif // JNIUTIL_H
