#! /usr/bin/env bash
set -uvx
set -e
cwd=`pwd`
rm -rf bin obj
dotnet build -c Release jwrap.csproj
cp -rp bin/Release/net462/*.exe $HOME/cmd/
rm -rf bin obj
dotnet build -c Release jwrapw.csproj
cp -rp bin/Release/net462/*.exe $HOME/cmd/
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
scons
cp -rp jwrap-jre.dll $HOME/cmd/
