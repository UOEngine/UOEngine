uoengine_add_library(Renderer)

target_link_libraries(Renderer PRIVATE d3d12.lib dxcompiler.lib dxgi.lib Core Memory)

add_dependencies(Renderer Dxc)
