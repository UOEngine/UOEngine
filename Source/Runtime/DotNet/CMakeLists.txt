uoengine_add_library(DotNet)

set(DOTNET_SDK "${UOENGINE_THIRD_PARTY_DIR}/Microsoft.NETCore.App.Host.win-x64/9.0.2/runtimes/win-x64/native")

target_include_directories(DotNet PRIVATE ${DOTNET_SDK})
target_link_libraries(DotNet PRIVATE ${DOTNET_SDK}/nethost.lib)