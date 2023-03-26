#ifndef JNIMEMORYBOOT_H
#define JNIMEMORYBOOT_H

#include <jni.h>
#include <string>
#include <vector>

struct JVM
{
    JavaVM* vm;
    JNIEnv* env;
};

//JVM create_java_vm(const std::wstring &jvmDllPath, const std::vector<std::wstring> &classPaths);
JVM create_java_vm(const std::wstring &jvmDllPath, const std::vector<std::wstring> &classPaths, const std::vector<std::wstring> &libraryPaths);
bool load_jar(JNIEnv* env, const std::vector<std::uint8_t> &jar_data, bool ignore_exceptions = false);

#endif // JNIMEMORYBOOT_H
