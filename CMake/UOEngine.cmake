
function(uoengine_common TARGET)

    cmake_path(GET CMAKE_CURRENT_SOURCE_DIR PARENT_PATH PP)

    target_include_directories(${TARGET} PRIVATE "${PP}")

    target_compile_definitions(${TARGET} PRIVATE UNICODE _UNICODE)

    get_filename_component(DIR ${CMAKE_CURRENT_SOURCE_DIR} DIRECTORY)

    get_filename_component(PARENT_NAME ${DIR} NAME)

    set_target_properties(${TARGET} PROPERTIES FOLDER ${PARENT_NAME})

    get_target_property(SOURCE_FILES ${TARGET} SOURCES)
    
    source_group(" " FILES ${SOURCE_FILES})

endfunction()

function(uoengine_add_executable TARGET)

    file(GLOB_RECURSE source_files
        "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp"
        "${CMAKE_CURRENT_SOURCE_DIR}/*.h"
        "${CMAKE_CURRENT_SOURCE_DIR}/*.inl"
    )

    add_executable(${TARGET} ${source_files} ${ARGN})

    uoengine_common(${TARGET})

endfunction()

function(uoengine_add_library TARGET)

    file(GLOB_RECURSE UOENGINE_SOURCE_FILES
        "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp"
        "${CMAKE_CURRENT_SOURCE_DIR}/*.h"
    )

    add_library(${TARGET} ${UOENGINE_SOURCE_FILES} ${ARGN})

    set_target_properties(${TARGET} PROPERTIES LINKER_LANGUAGE CXX)

    uoengine_common(${TARGET})

endfunction()

function(uoengine_add_shared_library TARGET)

    file(GLOB_RECURSE UOENGINE_SOURCE_FILES
        "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp"
        "${CMAKE_CURRENT_SOURCE_DIR}/*.h"
    )

    add_library(${TARGET} SHARED ${UOENGINE_SOURCE_FILES} ${ARGN})

    set_target_properties(${TARGET} PROPERTIES LINKER_LANGUAGE CXX)

    uoengine_common(${TARGET})

endfunction()

function(uoengine_add_csharp TARGET)

#file(GLOB_RECURSE source_files
#    "${CMAKE_CURRENT_SOURCE_DIR}/*.cs"
#)

 #   add_library(${TARGET} SHARED ${source_files} ${ARGN})

 #include_external_msproject(TARGET
 #   "${CMAKE_SOURCE_DIR}/MyCSharpProject/MyCSharpProject.csproj"
 #   TYPE FAE04EC0-301F-11D3-BF4B-00C04F79EFBC
#)

endfunction()

function(uoengine_add_thirdparty_library TARGET DIR)

    set(DIR "${UOENGINE_THIRD_PARTY_DIR}/${DIR}")

    file(GLOB_RECURSE source_files
        "${DIR}/*.cpp"
        "${DIR}/*.h"
    )

    message(STATUS "files ${source_files}")

    file(GLOB_RECURSE header_files
        "${DIR}/*.h"
    )

    file(GLOB_RECURSE lib_files
    "${DIR}/*.lib"
    )

    add_library(${TARGET} ${source_files})

    set_target_properties(${TARGET} PROPERTIES FOLDER "ThirdParty")

    target_include_directories(${TARGET} INTERFACE ${header_files})
    target_link_libraries(${TARGET} PUBLIC ${lib_files})

    #get_directory_property(targets DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} BUILDSYSTEM_TARGETS)
    #list(GET targets 0 first_target)  # Get the first target
    #message(STATUS "Inferred target: ${first_target}")

endfunction()