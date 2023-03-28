set -uvx
set -e
cwd=`pwd`
rm -rf build
gradle shadowJar
g++ -shared -o build/libs/add2.dll -I$HOME/common/include add2.cpp -static
cd $cwd/build/libs
jwrap-gen.exe ./my-app2-all.jar
ls -ltr
