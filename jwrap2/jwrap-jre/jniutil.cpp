#include "jniutil.h"

#include <jni.h>
#include <windows.h>
#include "strconv.h"
#include "wstrutil.h"
#include "pugixml.hpp"

#include <fstream>
#include <string>
#include <filesystem>

#include "jnimemoryboot.h"

std::string read_binary_file(const std::wstring& filename)
{
    std::ifstream file(std::filesystem::path(filename), std::ios::binary | std::ios::ate);
    if (!file) {
        throw std::runtime_error("Failed to open file");
    }
    std::streamsize size = file.tellg();
    file.seekg(0, std::ios::beg);
    std::string buffer(size, '\0');
    if (!file.read(buffer.data(), size)) {
        throw std::runtime_error("Failed to read file");
    }
    return buffer;
}

extern "C" int run_class_main(const wchar_t * x)
{
    std::string xml = wide_to_utf8(x);
    unicode_ostream uout(std::cout);
    uout << xml << std::endl;
    pugi::xml_document xmlDoc;
    pugi::xml_parse_result result = xmlDoc.load_string(xml.c_str());
    if (!result)
        return 1;
    pugi::xpath_node x_jre = xmlDoc.select_node("//jre");
    uout << "jre: " << x_jre.node().text().get() << std::endl;
    std::wstring jre = utf8_to_wide(x_jre.node().text().get());
    std::string jwrap_conf_xml = read_binary_file(jre + L"/jwrap.conf.xml");
    uout << jwrap_conf_xml << std::endl;
    pugi::xml_document xmlDoc2;
    if(!xmlDoc2.load_string(jwrap_conf_xml.c_str()))
        return 1;
    pugi::xpath_node x_jvm = xmlDoc2.select_node("//jvm");
    uout << "jvm(1): " << x_jvm.node().text().get() << std::endl;
    std::wstring jvm = jre + utf8_to_wide(x_jvm.node().text().get());
    uout << "jvm(2): " << jvm << std::endl;
    pugi::xpath_node x_jar = xmlDoc.select_node("//jar");
    uout << "jar: " << x_jar.node().text().get() << std::endl;
    std::wstring jar = utf8_to_wide(x_jar.node().text().get());
    pugi::xpath_node x_boot = xmlDoc.select_node("//boot.jar");
    uout << "boot: " << x_boot.node().text().get() << std::endl;
    std::wstring boot = utf8_to_wide(x_boot.node().text().get());
    pugi::xpath_node x_main = xmlDoc.select_node("//main");
    uout << "main: " << x_main.node().text().get() << std::endl;
    std::wstring mainClass = utf8_to_wide(x_main.node().text().get());
    std::vector<std::wstring> args;
    args.push_back(jar);
    args.push_back(mainClass);
    pugi::xpath_node_set x_args = xmlDoc.select_nodes("//args/arg");
    for (pugi::xpath_node x_arg: x_args)
    {
        pugi::xml_node n_args = x_arg.node();
        uout << "arg: " << n_args.text().get() << std::endl;
        args.push_back(utf8_to_wide(n_args.text().get()));
    }
    uout << "args.size()=" << args.size() << std::endl;
    std::string bootJar = read_binary_file(boot.c_str());
    //std::vector<std::wstring> classPaths { boot };
    uout << "before" << std::endl;
    bool b = JniUtil::RunClassMain(jvm, L"jwrap.boot.App", args, boot, bootJar);
    uout << "after: " << b << std::endl;
    return 0;
}



JniUtil::JniUtil()
{
}

//bool JniUtil::RunClassMain(const std::wstring &jvmDll, const std::wstring &mainClass, const std::vector<std::wstring> &args, const std::vector<std::wstring> &classPaths)
bool JniUtil::RunClassMain(const std::wstring &jvmDll, const std::wstring &mainClass, const std::vector<std::wstring> &args, const std::wstring &bootJarPath, const std::string bootJar)
{
    unicode_ostream uout(std::cout);

#if 0x0
    std::vector<std::wstring> classPaths;
    JVM jvm = create_java_vm(jvmDll, classPaths);
    if (!jvm.vm)
    {
        std::cerr<<"Failed to Create JavaVM\n";
        return false;
    }

    std::vector<std::uint8_t> jar_data = std::vector<std::uint8_t>(bootJar.begin(), bootJar.end());

    if (!load_jar(jvm.env, jar_data, true)) return false;

    // クラスをロード
    std::wstring mainClass_copy = mainClass;
    strutil::replace_all(mainClass_copy, L".", L"/");
    jclass cls = jvm.env->FindClass(wide_to_ansi(mainClass_copy).c_str());

    if (cls != nullptr) {
        // メソッドIDを取得
        jmethodID mid = jvm.env->GetStaticMethodID(cls, "main", "([Ljava/lang/String;)V");

        if (mid != nullptr) {
            // 引数を設定
#if 0x0
            jobjectArray args = env->NewObjectArray(2, env->FindClass("java/lang/String"), nullptr);
            env->SetObjectArrayElement(args, 0, env->NewStringUTF("arg1"));
            env->SetObjectArrayElement(args, 1, env->NewStringUTF("arg2"));
#else
            uout << "args.size()=" << args.size() << std::endl;
            jobjectArray mainArgs = jvm.env->NewObjectArray(args.size(), jvm.env->FindClass("java/lang/String"), nullptr);
            for (std::size_t i=0; i<args.size(); i++)
            {
                jvm.env->SetObjectArrayElement(mainArgs, i, jvm.env->NewStringUTF(wide_to_utf8(args[i]).c_str()));
            }
#endif
            // メソッドを呼び出し
            jvm.env->CallStaticVoidMethod(cls, mid, mainArgs);
        }
    }

#else

    std::vector<std::wstring> classPaths {bootJarPath};
    JVM jvm = create_java_vm(jvmDll, classPaths);
    if (!jvm.vm)
    {
        std::cerr<<"Failed to Create JavaVM\n";
        return false;
    }

    /*
    JavaVM *jvm;                      // JVMのポインタ
    JNIEnv *env;                      // JNIの環境ポインタ
    JavaVMInitArgs vm_args;           // JVMの初期化引数
    JavaVMOption options[1];          // JVMのオプション
    long status;                      // ステータス
    jclass cls;                        // クラス
    jmethodID mid;                     // メソッドID

    std::string join_classpath = "";
    for (std::size_t i=0; i<classPaths.size(); i++)
    {
        if (!join_classpath.empty())
        {
            join_classpath += ";";
        }
        join_classpath += wide_to_ansi(classPaths[i]);
    }
    uout << "join_classpath: " << join_classpath << std::endl;
    // JVMのオプションを設定
    options[0].optionString = strdup(::format("-Djava.class.path=%s", join_classpath.c_str()).c_str());

    // JVMの初期化引数を設定
    vm_args.version = JNI_VERSION_1_6;
    vm_args.nOptions = 1;
    vm_args.options = options;
    vm_args.ignoreUnrecognized = JNI_TRUE;

    // JVMを初期化
    //status = JNI_CreateJavaVM(&jvm, (void**)&env, &vm_args);
    // JVMを初期化
    HINSTANCE hinstLib = LoadLibraryW(jvmDll.c_str());
    typedef jint (JNICALL *PtrCreateJavaVM)(JavaVM **, void **, void *);
    PtrCreateJavaVM ptrCreateJavaVM = (PtrCreateJavaVM)GetProcAddress(hinstLib,"JNI_CreateJavaVM");
    status = ptrCreateJavaVM(&jvm, (void**)&env, &vm_args);
    */

    /*if (status != JNI_ERR)*/ {
        // クラスをロード
        std::wstring mainClass_copy = mainClass;
        strutil::replace_all(mainClass_copy, L".", L"/");
        jclass cls = jvm.env->FindClass(wide_to_ansi(mainClass_copy).c_str());

        if (cls != nullptr) {
            // メソッドIDを取得
            jmethodID mid = jvm.env->GetStaticMethodID(cls, "main", "([Ljava/lang/String;)V");

            if (mid != nullptr) {
                // 引数を設定
#if 0x0
                jobjectArray args = env->NewObjectArray(2, env->FindClass("java/lang/String"), nullptr);
                env->SetObjectArrayElement(args, 0, env->NewStringUTF("arg1"));
                env->SetObjectArrayElement(args, 1, env->NewStringUTF("arg2"));
#else
                uout << "args.size()=" << args.size() << std::endl;
                jobjectArray mainArgs = jvm.env->NewObjectArray(args.size(), jvm.env->FindClass("java/lang/String"), nullptr);
                for (std::size_t i=0; i<args.size(); i++)
                {
                    jvm.env->SetObjectArrayElement(mainArgs, i, jvm.env->NewStringUTF(wide_to_utf8(args[i]).c_str()));
                }
#endif

                // メソッドを呼び出し
                jvm.env->CallStaticVoidMethod(cls, mid, mainArgs);
            }
        }

        // JVMを破棄
        jvm.vm->DestroyJavaVM();
    }
#endif

    return true;
}
