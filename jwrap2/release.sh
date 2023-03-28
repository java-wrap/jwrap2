#! /usr/bin/env bash
set -uvx
set -e
cwd=`pwd`

#rm -rf $HOME/cmd/jwrap*
mkdir -p $HOME/cmd/tmp
rm -rf $HOME/cmd/tmp/jwrap*

$SPIDER/.software/zulu17-jdk/bin/javac.exe -encoding UTF-8 jwrap-boot/src/main/java/jwrap/boot/App.java
cp -rp jwrap-boot/src/main/java/jwrap/boot/App.class $HOME/cmd/tmp/jwrap-boot.class

rm -rf bin obj
dotnet build -c Release jwrap-head.csproj
ilmerge /out:$HOME/cmd/tmp/jwrap-head.exe /wildcards bin/Release/net462/*.exe bin/Release/net462/*.dll
rm -rf bin obj
dotnet build -c Release jwrapw-head.csproj
ilmerge /out:$HOME/cmd/tmp/jwrap-headw.exe /wildcards bin/Release/net462/*.exe bin/Release/net462/*.dll
rm -rf bin obj
dotnet build -c Release jwrap-gen.csproj
ilmerge /out:$HOME/cmd/tmp/jwrap-gen.exe /wildcards bin/Release/net462/*.exe bin/Release/net462/*.dll
cd $cwd/jwrap-jre
scons -c
scons
cp -rp jwrap-jre.dll $HOME/cmd/tmp/

#cd $cwd/jwrap-boot
#gradle jar
#cp -rp ./build/libs/jwrap-boot.jar $HOME/cmd/

ls -ltr $HOME/cmd/tmp/jwrap*
