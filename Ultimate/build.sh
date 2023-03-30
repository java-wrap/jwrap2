#! busybox bash
set -uvx
set -e
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
#version="${ts}-beta"
#version="${ts}"
version="0.0.`date "+%Y%m%d.%H%M%S"`-beta"
echo $version
sed -i -e "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/g" Ultimate.csproj
rm -rf obj bin
rm -rf Ultimate.Json
svn export https://github.com/JamesNK/Newtonsoft.Json/trunk/Src/Newtonsoft.Json Ultimate.Json
rm -rf Ultimate.Json/Properties/AssemblyInfo.cs
find Ultimate.Json -name "*.cs" -exec sed -i -e "s/Newtonsoft[.]Json/Ultimate.Json/g" {} +
rm -rf *.nupkg
dotnet pack -c Rlease -o .
