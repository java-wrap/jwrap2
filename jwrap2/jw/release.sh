#! /usr/bin/env bash
set -uvx
set -e
gh auth login --hostname github.com --git-protocol https --web
userName=nuget-tools
appName=dn4
cwd=`pwd`
rm -rf build
gradle jar
cd $cwd/build/libs
jar=`ls *.jar`
echo jar=$jar
repopath=javacommons/java.library
gh release create --repo $repopath --generate-notes jar || true
gh release upload jar "${jar}" --repo $repopath --clobber
url="https://github.com/javacommons/java.library/releases/download/jar/${jar}"
echo Uploaded to $url

