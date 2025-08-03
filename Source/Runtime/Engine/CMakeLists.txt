
uoengine_add_library(Engine)

target_link_libraries(Engine 
	PRIVATE $<IF:$<BOOL:${USE_LIVEPP}>,LivePP,>
	D3D12RHI
)

set_target_properties(Engine PROPERTIES OUTPUT_NAME "Engine")