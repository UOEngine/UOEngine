uoengine_add_library(Memory)

set(MIMALLOC "${UOENGINE_THIRD_PARTY_DIR}/mimalloc-3.0.1")

target_include_directories(Memory PRIVATE "${MIMALLOC}/include")
target_link_libraries(Memory PRIVATE mimalloc-static)