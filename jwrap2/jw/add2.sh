set -uvx
set -e
g++ -shared -o add2.dll -I$HOME/common/include add2.cpp -static
