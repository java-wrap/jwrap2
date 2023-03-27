#! /usr/bin/env bash
set -uvx
set -e
cwd=`pwd`

rm -rf $HOME/cmd/jwrap*

$SPIDER/.software/zulu17-jdk/bin/javac.exe -encoding UTF-8 jwrap-boot/src/main/java/jwrap/boot/App.java
cp -rp jwrap-boot/src/main/java/jwrap/boot/App.class $HOME/cmd/jwrap-boot.class

rm -rf bin obj
dotnet build -c Release jwrap-head.csproj
cp -rp bin/Release/net462/*.exe $HOME/cmd/
rm -rf bin obj
dotnet build -c Release jwrapw-head.csproj
cp -rp bin/Release/net462/*.exe $HOME/cmd/
rm -rf bin obj
dotnet build -c Release jwrap-gen.csproj
ilmerge /out:$HOME/cmd/jwrap-gen.exe /wildcards bin/Release/net462/jwrap-gen.exe bin/Release/net462/*.dll
cd $cwd/jwrap-jre
scons -c
scons
cp -rp jwrap-jre.dll $HOME/cmd/

#cd $cwd/jwrap-boot
#gradle jar
#cp -rp ./build/libs/jwrap-boot.jar $HOME/cmd/

ls -ltr $HOME/cmd/jwrap*
