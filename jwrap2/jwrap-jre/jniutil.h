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
    static bool RunClassMain(const std::wstring &jvmDll, const std::wstring &mainClass, const std::vector<std::wstring> &args, const std::vector<std::wstring> &classPaths);
};

#endif // JNIUTIL_H
