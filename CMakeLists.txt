cmake_minimum_required(VERSION 4.0)

project(UOEngine VERSION 0.0
                LANGUAGES CXX CSharp)

set(CMAKE_CXX_STANDARD 20)        # Set the standard
set(CMAKE_CXX_STANDARD_REQUIRED ON)  # Enforce the standard

option(USE_LIVEPP    "Enable Live++ support")
option(ENABLE_ASAN   "Enable address sanitiser (ASAN)" OFF)

cmake_policy(SET CMP0141 NEW)

set(UOENGINE_MAIN_SRC_DIR     ${CMAKE_CURRENT_SOURCE_DIR})
set(UOENGINE_THIRD_PARTY_DIR "${UOENGINE_MAIN_SRC_DIR}/ThirdParty")

include(${UOENGINE_MAIN_SRC_DIR}/CMake/UOEngine.cmake)
include(${UOENGINE_MAIN_SRC_DIR}/CMake/Win64.cmake)

set(CMAKE_CONFIGURATION_TYPES "Debug;Development")
set(CMAKE_BINARY_DIR "${UOENGINE_MAIN_SRC_DIR}/Intermediate")

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${UOENGINE_MAIN_SRC_DIR}/Binaries/Win64")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}")
# /GR-
# Disables Run-Time Type Information (RTTI) — disables use of dynamic_cast and typeid.

# /Gm-
# Disables minimal rebuild — it's obsolete and can cause problems with parallel builds.

# /fp:except-
# Disables floating-point exceptions (makes floating-point behavior faster and less strict).

# /nologo
# Hides the MSVC startup banner when compiling.

set(CMAKE_CXX_FLAGS "/GR- /Gm- /fp:except /MP /nologo")

if(ENABLE_ASAN)
    #message(STATUS "Using ASAN.")
    #set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /fsanitize=address")
endif()

file(GLOB_RECURSE cmake_files "${UOENGINE_MAIN_SRC_DIR}/Source/*/CMakeLists.txt")

foreach(cmake_file IN LISTS cmake_files)
    get_filename_component(dir ${cmake_file} DIRECTORY)

    message(STATUS "Found CMakeLists.txt in directory: ${dir}")

    add_subdirectory(${dir})

endforeach()

file(GLOB_RECURSE cmake_files "${UOENGINE_MAIN_SRC_DIR}/Source/ThirdParty/*.*")

foreach(cmake_file IN LISTS cmake_files)
    message(STATUS "Found third party cmake file: ${cmake_file}")

    include(${cmake_file})

endforeach()

add_custom_target(CMake SOURCES CMakeLists.txt)

add_subdirectory("${UOENGINE_THIRD_PARTY_DIR}/mimalloc-3.0.1" EXCLUDE_FROM_ALL)
set_target_properties(mimalloc-static PROPERTIES FOLDER "ThirdParty")

include_external_msproject(UOEngine "${UOENGINE_MAIN_SRC_DIR}/Source/Game/UOEngine.Editor/UOEngine.Editor.csproj")
set_target_properties(UOEngine PROPERTIES FOLDER "Game")
