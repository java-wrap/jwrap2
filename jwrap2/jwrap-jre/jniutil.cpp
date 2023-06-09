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

extern "C" const wchar_t* run_class_main(const wchar_t * x)
{
    //unicode_ostream uout(std::cout);
    std::string xml = wide_to_utf8(x);
    //uout << xml << std::endl;
    pugi::xml_document xmlDoc;
    pugi::xml_parse_result result = xmlDoc.load_string(xml.c_str());
    if (!result)
        return L"Invalid xml";
    pugi::xpath_node x_jre = xmlDoc.select_node("//jre");
    //uout << "jre: " << x_jre.node().text().get() << std::endl;
    std::wstring jre = utf8_to_wide(x_jre.node().text().get());
    std::string jwrap_conf_xml = read_binary_file(jre + L"/jwrap.conf.xml");
    //uout << jwrap_conf_xml << std::endl;
    pugi::xml_document xmlDoc2;
    if(!xmlDoc2.load_string(jwrap_conf_xml.c_str()))
        return L"Invalid jwrap.conf.xml";
    pugi::xpath_node x_jvm = xmlDoc2.select_node("//jvm");
    //uout << "jvm(1): " << x_jvm.node().text().get() << std::endl;
    std::wstring jvm = jre + utf8_to_wide(x_jvm.node().text().get());
    //uout << "jvm(2): " << jvm << std::endl;
    pugi::xpath_node x_jar = xmlDoc.select_node("//jar");
    //uout << "jar: " << x_jar.node().text().get() << std::endl;
    std::wstring jar = utf8_to_wide(x_jar.node().text().get());
    pugi::xpath_node x_boot = xmlDoc.select_node("//boot.jar");
    //uout << "boot: " << x_boot.node().text().get() << std::endl;
    std::wstring boot = utf8_to_wide(x_boot.node().text().get());
    pugi::xpath_node x_main = xmlDoc.select_node("//main");
    //uout << "main: " << x_main.node().text().get() << std::endl;
    std::wstring mainClass = utf8_to_wide(x_main.node().text().get());
    std::vector<std::wstring> args;
    args.push_back(jar);
    args.push_back(mainClass);
    pugi::xpath_node_set x_args = xmlDoc.select_nodes("//args/arg");
    for (pugi::xpath_node x_arg: x_args)
    {
        pugi::xml_node n_args = x_arg.node();
        //uout << "arg: " << n_args.text().get() << std::endl;
        args.push_back(utf8_to_wide(n_args.text().get()));
    }
    //uout << "args.size()=" << args.size() << std::endl;
    //std::string bootJar = read_binary_file(boot.c_str());
    //uout << "before" << std::endl;
    static thread_local std::wstring b;
    //b = JniUtil::RunClassMain(jvm, L"jwrap.boot.App", args, boot, bootJar);
    b = JniUtil::RunClassMain(utf8_to_wide(xml));
    //uout << "after: " << b << std::endl;
    return b.c_str();
}

JniUtil::JniUtil()
{
}

std::string jstringToString(JNIEnv* env, jstring jStr)
{
  const char* cStr = env->GetStringUTFChars(jStr, nullptr);
  std::string str(cStr);
  env->ReleaseStringUTFChars(jStr, cStr);
  return str;
}

//std::wstring JniUtil::RunClassMain(const std::wstring &jvmDll, const std::wstring &mainClass, const std::vector<std::wstring> &args, const std::wstring &bootJarPath, const std::string bootJar)
std::wstring JniUtil::RunClassMain(const std::wstring &xml_wide)
{
    //unicode_ostream uout(std::cout);
    std::string xml = wide_to_utf8(xml_wide);
    //uout << xml << std::endl;
    pugi::xml_document xmlDoc;
    pugi::xml_parse_result result = xmlDoc.load_string(xml.c_str());
    if (!result)
        return L"Invalid xml";
    pugi::xpath_node x_jre = xmlDoc.select_node("//jre");
    //uout << "jre: " << x_jre.node().text().get() << std::endl;
    std::wstring jre = utf8_to_wide(x_jre.node().text().get());
    std::string jwrap_conf_xml = read_binary_file(jre + L"/jwrap.conf.xml");
    //uout << jwrap_conf_xml << std::endl;
    pugi::xml_document xmlDoc2;
    if(!xmlDoc2.load_string(jwrap_conf_xml.c_str()))
        return L"Invalid jwrap.conf.xml";
    pugi::xpath_node x_jvm = xmlDoc2.select_node("//jvm");
    //uout << "jvm(1): " << x_jvm.node().text().get() << std::endl;
    std::wstring jvmDll = jre + utf8_to_wide(x_jvm.node().text().get());
    //uout << "jvm(2): " << jvmDll << std::endl;
    pugi::xpath_node x_jar = xmlDoc.select_node("//jar");
    //uout << "jar: " << x_jar.node().text().get() << std::endl;
    std::wstring jar = utf8_to_wide(x_jar.node().text().get());
#if 0x0
    pugi::xpath_node x_boot = xmlDoc.select_node("//boot.jar");
    //uout << "boot: " << x_boot.node().text().get() << std::endl;
    std::wstring boot = utf8_to_wide(x_boot.node().text().get());
#else
    pugi::xpath_node x_boot = xmlDoc.select_node("//boot.class");
    //uout << "boot: " << x_boot.node().text().get() << std::endl;
    std::wstring boot = utf8_to_wide(x_boot.node().text().get());
#endif
    pugi::xpath_node x_main = xmlDoc.select_node("//main");
    //uout << "main: " << x_main.node().text().get() << std::endl;
    //std::wstring mainClass = utf8_to_wide(x_main.node().text().get());
    //uout << "args.size()=" << args.size() << std::endl;
#if 0x0
    std::vector<std::wstring> classPaths {boot};
#else
    std::string bootClassBytes = read_binary_file(boot);
    std::vector<std::wstring> classPaths {};
#endif
    std::filesystem::path path(boot);
    std::wstring parent = path.parent_path().wstring();
    std::vector<std::wstring> libraryPaths { parent + L"\\dll" };
    JVM jvm = create_java_vm(jvmDll, classPaths, libraryPaths);
    if (!jvm.vm)
    {
        std::cerr<<"Failed to Create JavaVM\n";
        return L"Failed to Create JavaVM\n";
    }
    //uout << "mainCass: " << mainClass << std::endl;
    std::wstring errorMessage;
    // クラスをロード
    //std::wstring mainClass_copy = mainClass;
    //strutil::replace_all(mainClass_copy, L".", L"/");
    //jclass cls = jvm.env->FindClass(wide_to_ansi(mainClass_copy).c_str());
#if 0x0
    jclass cls = jvm.env->FindClass("jwrap/boot/App");
#else
    jclass cls = jvm.env->DefineClass("jwrap/boot/App", NULL, (const jbyte *)&bootClassBytes[0], bootClassBytes.size());
#endif
    if (cls != nullptr) {
        // メソッドIDを取得
        //jmethodID mid = jvm.env->GetStaticMethodID(cls, "main", "([Ljava/lang/String;)V");
        //jmethodID mid = jvm.env->GetStaticMethodID(cls, "tryToRun", "([Ljava/lang/String;)Ljava/lang/String;");
        jmethodID mid = jvm.env->GetStaticMethodID(cls, "tryToRun", "(Ljava/lang/String;)Ljava/lang/String;");

        if (mid == nullptr) {
            //uout << "(mid == nullptr)" << std::endl;
        } else {
            // 引数を設定
            //uout << "args.size()=" << args.size() << std::endl;
#if 0x0
            jobjectArray mainArgs = jvm.env->NewObjectArray(args.size(), jvm.env->FindClass("java/lang/String"), nullptr);
            for (std::size_t i=0; i<args.size(); i++)
            {
                jvm.env->SetObjectArrayElement(mainArgs, i, jvm.env->NewStringUTF(wide_to_utf8(args[i]).c_str()));
            }
#endif
            // メソッドを呼び出し
            //jvm.env->CallStaticVoidMethod(cls, mid, mainArgs);
            jobject ret = jvm.env->CallStaticObjectMethod(cls, mid, jvm.env->NewStringUTF(xml.c_str()));
            errorMessage = utf8_to_wide(jstringToString(jvm.env, (jstring)ret));
        }
    }
    // JVMを破棄
    jvm.vm->DestroyJavaVM();
    return errorMessage;
}
