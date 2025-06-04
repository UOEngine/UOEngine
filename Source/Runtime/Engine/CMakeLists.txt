
uoengine_add_library(Engine)

target_link_libraries(Engine PRIVATE $<IF:$<BOOL:${USE_LIVEPP}>,LivePP,> Renderer)

