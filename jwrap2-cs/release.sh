#! /usr/bin/env bash
set -uvx
set -e
script_dir=$(dirname "$(realpath "$0")")
last_dir=$(basename "$script_dir")
dll=${last_dir}.dll
csproj=${last_dir}.csproj
gh auth login --hostname github.com --git-protocol https --web
cwd=`pwd`
rm -rf bin obj
dotnet build -c Release ${csproj}
cd $cwd/bin/Release/net462
echo dll=$dll
repopath=javacommons/java.library
gh release create --repo $repopath --generate-notes jar || true
gh release upload jar "${dll}" --repo $repopath --clobber
url="https://github.com/javacommons/java.library/releases/download/jar/${dll}"
echo Uploaded to $url
