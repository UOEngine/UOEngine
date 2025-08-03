uoengine_add_library(D3D12RHI)

target_link_libraries(D3D12RHI PRIVATE d3d12.lib dxcompiler.lib dxgi.lib Core Memory)

add_dependencies(D3D12RHI Dxc)
