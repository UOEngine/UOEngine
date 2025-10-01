cmake_minimum_required(VERSION 4.0)

project(UOEngine VERSION 0.0
                LANGUAGES CXX CSharp)

set(CMAKE_CXX_STANDARD 20)              # Set the standard
set(CMAKE_CXX_STANDARD_REQUIRED ON)     # Enforce the standard

option(USE_LIVEPP    "Enable Live++ support"           OFF)
option(ENABLE_ASAN   "Enable address sanitiser (ASAN)" OFF)

cmake_policy(SET CMP0141 NEW)

include(FetchContent)

set(UOENGINE_MAIN_SRC_DIR               "${CMAKE_CURRENT_SOURCE_DIR}")
set(UOENGINE_THIRD_PARTY_DIR            "${UOENGINE_MAIN_SRC_DIR}/ThirdParty")
set(UOENGINE_TOOLS_SRC_DIR              "${UOENGINE_MAIN_SRC_DIR}/Source/Tools")
set(UOENGINE_DOWNLOAD_THIRD_PARTY_DIR   "${UOENGINE_MAIN_SRC_DIR}/Intermediate/Downloads")

include(${UOENGINE_MAIN_SRC_DIR}/CMake/UOEngine.cmake)

set(CMAKE_CONFIGURATION_TYPES           "Debug;Development")
set(CMAKE_BINARY_DIR                    "${UOENGINE_MAIN_SRC_DIR}/Intermediate")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY      "${UOENGINE_MAIN_SRC_DIR}/Binaries/Win64")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY      "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}")

set(CMAKE_CXX_FLAGS_DEVELOPMENT "-O2" CACHE STRING "Flags for Development build" FORCE)
set(CMAKE_C_FLAGS_DEVELOPMENT   "-O2" CACHE STRING "Flags for Development build" FORCE)

if(WIN32 AND MSVC)
    include(CMake/Win64.cmake)
else()
    message(FATAL_ERROR "Unsupported platform now for.")
endif()

file(GLOB_RECURSE cmake_files "${UOENGINE_MAIN_SRC_DIR}/Source/ThirdParty/*.*")

foreach(cmake_file IN LISTS cmake_files)
    message(STATUS "Found third party cmake file: ${cmake_file}")

    include(${cmake_file})

endforeach()

file(GLOB_RECURSE cmake_files "${UOENGINE_MAIN_SRC_DIR}/Source/*/CMakeLists.txt")

foreach(cmake_file IN LISTS cmake_files)
    get_filename_component(dir ${cmake_file} DIRECTORY)

    message(STATUS "Found CMakeLists.txt in directory: ${dir}")

    add_subdirectory(${dir})

endforeach()

# Find C# projects

file(GLOB_RECURSE csproj_files "${UOENGINE_MAIN_SRC_DIR}/Source/Game/*.csproj")

foreach(csproj_file IN LISTS csproj_files)

    get_filename_component(csproj_name ${csproj_file} NAME)

    message(STATUS "Found csproj file: ${csproj_name} ${csproj_file}")
    include_external_msproject(${csproj_name} "${csproj_file}")

    set_target_properties(${csproj_name} PROPERTIES FOLDER "Game")

endforeach() 

file(GLOB_RECURSE csproj_files "${UOENGINE_MAIN_SRC_DIR}/Source/Tools/*.csproj")

foreach(csproj_file IN LISTS csproj_files)

    get_filename_component(csproj_name ${csproj_file} NAME)

    message(STATUS "Found csproj file: ${csproj_name} ${csproj_file}")
    include_external_msproject(${csproj_name} "${csproj_file}")

    set_target_properties(${csproj_name} PROPERTIES FOLDER "Tools")

endforeach() 

