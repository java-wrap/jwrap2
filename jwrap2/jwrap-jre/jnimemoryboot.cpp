#include "jnimemoryboot.h"

#include "debug_line.h"

#include <jni.h>
#include <iostream>
#include <memory>
#include <utility>
#include <vector>

JVM create_java_vm(const std::wstring &jvmDllPath)
{
    const char* argv[] = {"-Djava.compiler=NONE",
                          "-Djava.class.path=."}; //"-verbose:jni"
    const int argc = static_cast<int>(sizeof(argv) / sizeof(argv[0]));

    JavaVMInitArgs jvm_args;
    JavaVMOption options[argc];

    for (int i = 0; i < argc; ++i)
    {
        options[i].optionString = const_cast<char*>(argv[i]);
    }

    JavaVM* vm = nullptr;
    JNIEnv* env = nullptr;
    //JNI_GetDefaultJavaVMInitArgs(&jvm_args);

    jvm_args.version = JNI_VERSION_1_8;
    jvm_args.nOptions = argc;
    jvm_args.options = options;
    jvm_args.ignoreUnrecognized = false;

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

int Xmain(const std::wstring &jvmDllPath, const std::string &bootJar)
{
    JVM jvm = create_java_vm(jvmDllPath);
    if (!jvm.vm)
    {
        std::cerr<<"Failed to Create JavaVM\n";
        return 0;
    }

    std::vector<std::uint8_t> jar_data = std::vector<std::uint8_t>(bootJar.begin(), bootJar.end());

    if (load_jar(jvm.env, jar_data, true))
    {
        std::cout<<"Jar loaded successfully\n";
        jclass cls;                        // クラス
        jmethodID mid;                     // メソッドID
        cls = jvm.env->FindClass("global/Main");

        if (cls != nullptr) {
            // メソッドIDを取得
            mid = jvm.env->GetStaticMethodID(cls, "main", "([Ljava/lang/String;)V");

            if (mid != nullptr) {
                // 引数を設定
#if 0x1
                jobjectArray args = jvm.env->NewObjectArray(2, jvm.env->FindClass("java/lang/String"), nullptr);
                jvm.env->SetObjectArrayElement(args, 0, jvm.env->NewStringUTF("arg1"));
                jvm.env->SetObjectArrayElement(args, 1, jvm.env->NewStringUTF("arg2"));
#else
                jobjectArray mainArgs = env->NewObjectArray(args.length(), env->FindClass("java/lang/String"), nullptr);
                for (int i=0; i<args.length(); i++)
                {
                    env->SetObjectArrayElement(mainArgs, 0, env->NewStringUTF(args[i].toStdString().c_str()));
                }
#endif

                // メソッドを呼び出し
                jvm.env->CallStaticVoidMethod(cls, mid, args);
            }
        }
    }
    else
    {
        std::cerr<<"Failed to load Jar\n";
    }

    jvm.vm->DestroyJavaVM();
    return 0;
}
