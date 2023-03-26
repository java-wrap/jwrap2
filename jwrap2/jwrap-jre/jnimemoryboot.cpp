#include "jnimemoryboot.h"

#include "debug_line.h"

#include <jni.h>
#include <iostream>
#include <memory>
#include <utility>
#include <vector>
#include "wstrutil.h"

JVM create_java_vm(const std::wstring &jvmDllPath, const std::vector<std::wstring> &classPaths, const std::vector<std::wstring> &libraryPaths)
{
    std::wstring join_classpath = (std::wstring(L"-Djava.class.path=")+strutil::join(classPaths, L";"));
    std::wstring join_libpath = (std::wstring(L"-Djava.library.path=")+strutil::join(libraryPaths, L";"));

    JavaVMInitArgs jvm_args;

    JavaVM* vm = nullptr;
    JNIEnv* env = nullptr;

    jvm_args.version = JNI_VERSION_1_6;
    jvm_args.ignoreUnrecognized = JNI_TRUE;
    jvm_args.nOptions = 0;
    JavaVMOption options[2];
    if (classPaths.size() > 0)
    {
        options[jvm_args.nOptions].optionString = strdup(wide_to_ansi(join_classpath).c_str());
        jvm_args.nOptions += 1;
        jvm_args.options = options;
    }
    if (libraryPaths.size() > 0)
    {
        options[jvm_args.nOptions].optionString = strdup(wide_to_ansi(join_libpath).c_str());
        jvm_args.nOptions += 1;
        jvm_args.options = options;
    }

    HINSTANCE hinstLib = LoadLibraryW(jvmDllPath.c_str());
    typedef jint (JNICALL *PtrCreateJavaVM)(JavaVM **, void **, void *);
    PtrCreateJavaVM ptrCreateJavaVM = (PtrCreateJavaVM)GetProcAddress(hinstLib,"JNI_CreateJavaVM");
    if (ptrCreateJavaVM(&vm, (void**)&env, &jvm_args) != JNI_OK)
    {
        return {vm, env};
    }
    return {vm, env};
}

bool load_jar(JNIEnv* env, const std::vector<std::uint8_t> &jar_data, bool ignore_exceptions /* = false */)
{
    auto string_replace_all = [&](std::string str, const std::string& from, const std::string& to) -> std::string {
        size_t start_pos = 0;
        while((start_pos = str.find(from, start_pos)) != std::string::npos)
        {
            str.replace(start_pos, from.length(), to);
        }
        return str;
    };

    auto byte_array_input_stream = [](JNIEnv* env, const std::vector<std::uint8_t> &buffer) -> jobject {
        jbyteArray arr = env->NewByteArray(static_cast<jsize>(buffer.size()));
        if (arr)
        {
            env->SetByteArrayRegion(arr, 0, static_cast<jsize>(buffer.size()), reinterpret_cast<const jbyte*>(&buffer[0]));
            jclass cls = env->FindClass("java/io/ByteArrayInputStream");
            if (cls)
            {
                jmethodID method = env->GetMethodID(cls, "<init>", "([B)V");
                if (method)
                {
                    jobject result = env->NewObject(cls, method, arr);
                    if (result)
                    {
                        env->DeleteLocalRef(std::exchange(result, env->NewGlobalRef(result)));
                        env->DeleteLocalRef(cls);
                        env->DeleteLocalRef(arr);
                        return result;
                    }
                }
                env->DeleteLocalRef(cls);
            }
            env->DeleteLocalRef(arr);
        }
        return nullptr;
    };

    auto jar_input_stream = [](JNIEnv* env, jobject input_stream) -> jobject {
        jclass cls = env->FindClass("java/util/jar/JarInputStream");
        if (cls)
        {
            jmethodID method = env->GetMethodID(cls, "<init>", "(Ljava/io/InputStream;)V");
            if (method)
            {
                jobject result = env->NewObject(cls, method, input_stream);
                if (result)
                {
                    env->DeleteLocalRef(std::exchange(result, env->NewGlobalRef(result)));
                    env->DeleteLocalRef(cls);
                    return result;
                }
            }
            env->DeleteLocalRef(cls);
        }
        return nullptr;
    };

    auto input_stream_read = [](JNIEnv* env, jobject input_stream) -> jint {
        jclass cls = env->GetObjectClass(input_stream);
        if (cls)
        {
            jmethodID method = env->GetMethodID(cls, "read", "()I");
            if (method)
            {
                jint result = env->CallIntMethod(input_stream, method);
                env->DeleteLocalRef(cls);
                return result;
            }

            env->DeleteLocalRef(cls);
        }
        return -1;
    };

    auto byte_array_output_stream = [](JNIEnv* env) -> jobject {
        jclass cls = env->FindClass("java/io/ByteArrayOutputStream");
        if (cls)
        {
            jmethodID method = env->GetMethodID(cls, "<init>", "()V");
            if (method)
            {
                jobject result = env->NewObject(cls, method);
                if (result)
                {
                    env->DeleteLocalRef(std::exchange(result, env->NewGlobalRef(result)));
                    env->DeleteLocalRef(cls);
                    return result;
                }
            }
            env->DeleteLocalRef(cls);
        }
        return nullptr;
    };

    auto output_stream_write = [](JNIEnv* env, jobject output_stream, jint value) {
        jclass cls = env->GetObjectClass(output_stream);
        if (cls)
        {
            jmethodID method = env->GetMethodID(cls, "write", "(I)V");
            if (method)
            {
                env->CallVoidMethod(output_stream, method, value);
            }
            env->DeleteLocalRef(cls);
        }
    };

    auto byte_array_output_stream_to_byte_array = [](JNIEnv* env, jobject output_stream) -> std::vector<std::uint8_t> {
        jclass cls = env->GetObjectClass(output_stream);
        if (cls)
        {
            jmethodID method = env->GetMethodID(cls, "toByteArray", "()[B");
            if (method)
            {
                jbyteArray bytes = reinterpret_cast<jbyteArray>(env->CallObjectMethod(output_stream, method));
                if (bytes)
                {
                    void* arr = env->GetPrimitiveArrayCritical(bytes, nullptr);
                    if (arr)
                    {
                        jsize size = env->GetArrayLength(bytes);
                        auto result = std::vector<std::uint8_t>(static_cast<std::uint8_t*>(arr),
                                                                static_cast<std::uint8_t*>(arr) + size);
                        env->ReleasePrimitiveArrayCritical(bytes, arr, 0);
                        env->DeleteLocalRef(bytes);
                        env->DeleteLocalRef(cls);
                        return result;
                    }
                }
            }
            env->DeleteLocalRef(cls);
        }
        return {};
    };

    auto get_next_jar_entry = [](JNIEnv* env, jobject jar_input_stream) -> jobject {
        jclass cls = env->GetObjectClass(jar_input_stream);
        if (cls)
        {
            jmethodID method = env->GetMethodID(cls, "getNextJarEntry", "()Ljava/util/jar/JarEntry;");
            if (method)
            {
                jobject result = env->CallObjectMethod(jar_input_stream, method);
                env->DeleteLocalRef(std::exchange(result, env->NewGlobalRef(result)));
                env->DeleteLocalRef(cls);
                return result;
            }

            env->DeleteLocalRef(cls);
        }
        return nullptr;
    };

    auto jar_entry_get_name = [](JNIEnv* env, jobject jar_entry) -> std::string {
        jclass cls = env->FindClass("java/util/jar/JarEntry");
        if (cls)
        {
            jmethodID method = env->GetMethodID(cls, "getName", "()Ljava/lang/String;");
            if (method)
            {
                jstring jstr = reinterpret_cast<jstring>(env->CallObjectMethod(jar_entry, method));
                const char *name_string = env->GetStringUTFChars(jstr, 0);
                std::string result = std::string(name_string);
                env->ReleaseStringUTFChars(jstr, name_string);
                env->DeleteLocalRef(jstr);
                env->DeleteLocalRef(cls);
                return result;
            }
            env->DeleteLocalRef(cls);
        }
        return std::string();
    };

    // Load the Jar
    jobject bais = byte_array_input_stream(env, jar_data);
    if (!bais)
    {
        std::cerr<<"Failed to open ByteArrayInputStream\n";
        return false;
    }

    jobject jis = jar_input_stream(env, bais);
    if (!jis)
    {
        env->DeleteGlobalRef(bais);
        std::cerr<<"Failed to open JarInputStream\n";
        return false;
    }

    // Read the jar entries
    jobject jar_entry = nullptr;
    const std::string extension = ".class";

    while((jar_entry = get_next_jar_entry(env, jis)))
    {
        std::string name = jar_entry_get_name(env, jar_entry);
        if ((name.length() > extension.length()) && (name.rfind(extension) == name.length() - extension.length()))
        {
            jobject baos = byte_array_output_stream(env);
            if (!baos)
            {
                std::cerr<<"Failed to open ByteArrayOutputStream for: "<<name<<"\n";
                if (!ignore_exceptions)
                {
                    env->DeleteGlobalRef(jar_entry);
                    env->DeleteGlobalRef(jis);
                    env->DeleteGlobalRef(bais);
                    return false;
                }
            }

            // Define the class
            jint value = -1;
            while((value = input_stream_read(env, jis)) != -1)
            {
                output_stream_write(env, baos, value);
            }

            std::vector<std::uint8_t> bytes = byte_array_output_stream_to_byte_array(env, baos);

            std::string canonicalName = string_replace_all(string_replace_all(name, "/", "."),
                                                            ".class",
                                                            std::string());

            jclass cls = env->DefineClass(string_replace_all(name, ".class", "").c_str(),
                                            nullptr, reinterpret_cast<jbyte*>(bytes.data()),
                                            static_cast<jint>(bytes.size()));

            if (cls)
            {
                std::cerr<<"Defined: "<<canonicalName<<" Size: "<<bytes.size()<<"\n";
                env->DeleteLocalRef(cls);
            }
            else
            {
                std::cerr<<"Failed to define: "<<canonicalName<<"  Size: "<<bytes.size()<<"\n";
                if (env->ExceptionCheck())
                {
                    env->ExceptionDescribe();
                    env->ExceptionClear();

                    if (!ignore_exceptions)
                    {
                        env->DeleteGlobalRef(jar_entry);
                        env->DeleteGlobalRef(jis);
                        env->DeleteGlobalRef(bais);
                        return false;
                    }
                }
            }
        }
        else
        {
            std::cerr<<"Skipping Resource: "<<name<<"\n";
        }

        env->DeleteGlobalRef(jar_entry);
    }
    env->DeleteGlobalRef(jis);
    env->DeleteGlobalRef(bais);
    return true;
}
