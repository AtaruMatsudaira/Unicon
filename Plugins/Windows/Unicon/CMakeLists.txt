cmake_minimum_required(VERSION 4.0)
project(UniconPlugin)

set(CMAKE_CXX_STANDARD 20)

add_library(UniconPlugin SHARED UniconPlugin.cpp
)

set_target_properties(UniconPlugin PROPERTIES
        OUTPUT_NAME "UniconPluginForWindows"
        PREFIX ""
)

if(MINGW)
    target_link_options(UniconPlugin PRIVATE
            -static-libgcc
            -static-libstdc++
            -Wl,-Bstatic,--whole-archive -lwinpthread -Wl,--no-whole-archive
    )
endif()

if(WIN32)
    target_link_libraries(UniconPlugin
            shell32
            user32
            shlwapi
            propsys
    )

    target_compile_definitions(UniconPlugin PRIVATE UNICODE _UNICODE)
endif()


