
uoengine_add_shared_library(NativeInterop)

target_link_libraries(NativeInterop
	Engine
	Renderer
)

set_target_properties(NativeInterop PROPERTIES OUTPUT_NAME "UOEngine.Native")

add_custom_command(
    TARGET NativeInterop
    POST_BUILD
    COMMAND dotnet run --project "${UOENGINE_TOOLS_SRC_DIR}/BindingsGenerator/BindingsGenerator.csproj"
)