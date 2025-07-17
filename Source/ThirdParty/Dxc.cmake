FetchContent_Declare(
  DxCompiler
  URL			https://github.com/microsoft/DirectXShaderCompiler/releases/download/v1.8.2505.1/dxc_2025_07_14.zip
  URL_HASH		SHA256=9ad895a6b039e3a8f8c22a1009f866800b840a74b50db9218d13319e215ea8a4
)

FetchContent_MakeAvailable(DxCompiler)

file(GLOB_RECURSE header_files
        "${dxcompiler_SOURCE_DIR}/inc/*.h"
)

add_custom_target(Dxc
	COMMAND ${CMAKE_COMMAND} -E copy_if_different "${dxcompiler_SOURCE_DIR}/bin/x64/dxcompiler.dll" "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}/$<CONFIG>"
	SOURCES ${header_files}
)

set_target_properties(Dxc PROPERTIES FOLDER "ThirdParty")