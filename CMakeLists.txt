cmake_minimum_required(VERSION 4.0)

project(UOEngine VERSION 0.0
                LANGUAGES CXX CSharp)

set(CMAKE_CXX_STANDARD 20)        # Set the standard
set(CMAKE_CXX_STANDARD_REQUIRED ON)  # Enforce the standard

option(USE_LIVEPP "Enable Live++ support")

cmake_policy(SET CMP0141 NEW)

set(UOENGINE_MAIN_SRC_DIR     ${CMAKE_CURRENT_SOURCE_DIR})
set(UOENGINE_THIRD_PARTY_DIR "${UOENGINE_MAIN_SRC_DIR}/ThirdParty")

include(${UOENGINE_MAIN_SRC_DIR}/CMake/UOEngine.cmake)
include(${UOENGINE_MAIN_SRC_DIR}/CMake/Win64.cmake)

set(CMAKE_CONFIGURATION_TYPES "Debug;Development")
set(CMAKE_BINARY_DIR "${UOENGINE_MAIN_SRC_DIR}/Intermediate")

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

include_external_msproject(UOEngine "${UOENGINE_MAIN_SRC_DIR}/Source/Game/UOEngine.csproj")
set_target_properties(UOEngine PROPERTIES FOLDER "Game")
