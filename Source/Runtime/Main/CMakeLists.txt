
uoengine_add_executable(Main)
#
add_compile_definitions(Main _UNICODE=1)
#
target_link_libraries(Main PRIVATE Engine)
