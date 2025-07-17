FetchContent_Declare(
  mimalloc
  GIT_REPOSITORY https://github.com/microsoft/mimalloc.git
  GIT_TAG        dfa50c37d951128b1e77167dd9291081aa88eea4 # v3.1.5
)

FetchContent_MakeAvailable(mimalloc)

#add_subdirectory(${mimalloc_SOURCE_DIR} ${mimalloc_BINARY_DIR} EXCLUDE_FROM_ALL)
#set_target_properties(mimalloc-static PROPERTIES FOLDER "ThirdParty")

get_directory_property(MIMALLOC_TARGETS DIRECTORY ${mimalloc_SOURCE_DIR} BUILDSYSTEM_TARGETS)

foreach(tgt IN LISTS MIMALLOC_TARGETS)

    #if(TARGET mimalloc-static)
     #   set_target_properties(mimalloc-static PROPERTIES FOLDER "ThirdParty")
   # else()
        set_target_properties(${tgt} PROPERTIES FOLDER "ThirdParty")
   # endif()

endforeach()
