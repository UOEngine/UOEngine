
uoengine_add_shared_library(NativeInterop)

target_link_libraries(NativeInterop
	Engine
	Renderer
)

set_target_properties(NativeInterop PROPERTIES OUTPUT_NAME "UOEngine.Native")