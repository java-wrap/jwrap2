#QT += core xmlpatterns

CONFIG -= qt

CONFIG += c++17
CONFIG += force_debug_info

TEMPLATE = lib
#CONFIG += staticlib
CONFIG += dll

#DEFINES += QT_DISABLE_DEPRECATED_BEFORE=0x060000
DEFINES += DEBUG_LINE

gcc:QMAKE_CXXFLAGS_WARN_ON += -Wno-unused-parameter -Wno-unused-function
msvc:QMAKE_CXXFLAGS += /bigobj

INCLUDEPATH += $$PWD
INCLUDEPATH += $$(SPIDER)/.software/zulu17-jdk/include
INCLUDEPATH += $$(SPIDER)/.software/zulu17-jdk/include/win32

#gcc:LIBS += -lssl -lcrypto

DESTDIR += $$PWD

TARGET = $${TARGET}$${QT_MAJOR_VERSION}-$${QMAKE_HOST.arch}
#message($$QMAKE_QMAKE)
contains(QMAKE_QMAKE, .*static.*) {
    message( "[STATIC BUILD]" )
    DEFINES += QT_STATIC_BUILD
    TARGET = $${TARGET}-static
} else {
    message( "[SHARED BUILD]" )
}
message($$TARGET)
MOC_DIR = build-$${TARGET}
OBJECTS_DIR = build-$${TARGET}
RCC_DIR = build-$${TARGET}
UI_DIR = build-$${TARGET}

include($$(HOME)/common/include/include.pri)
#include($$(HOME)/common/boost.pri)
#include($$(HOME)/common/common/common.pri)
#include($$(HOME)/common/common2/common2.pri)

HEADERS += \
    jnimemoryboot.h \
    jniutil.h \
    pugiconfig.hpp \
    pugixml.hpp

SOURCES += \
    jnimemoryboot.cpp \
    jniutil.cpp \
    pugixml.cpp
