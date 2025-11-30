cmake_minimum_required(VERSION 4.0)
project(DockIconPlugin)

set(CMAKE_CXX_STANDARD 20)

add_library(DockIconPlugin SHARED DockIconPlugin.cpp)

set_target_properties(DockIconPlugin PROPERTIES
        OUTPUT_NAME "DockIconPluginForWindows"
        PREFIX ""
)

if(MINGW)
    target_link_options(DockIconPlugin PRIVATE
            -static-libgcc
            -static-libstdc++
            -Wl,-Bstatic,--whole-archive -lwinpthread -Wl,--no-whole-archive
    )
endif()

if(WIN32)
    target_link_libraries(DockIconPlugin
            shell32
            user32
            shlwapi
            propsys
    )

    target_compile_definitions(DockIconPlugin PRIVATE UNICODE _UNICODE)
endif()


