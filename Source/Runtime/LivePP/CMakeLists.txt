if(USE_LIVEPP)

message(STATUS "Live++ support is enabled")

uoengine_add_library(LivePP)

set(LIVEPP_SDK "${UOENGINE_THIRD_PARTY_DIR}/LPP_2_9_1")

target_include_directories(LivePP PRIVATE ${LIVEPP_SDK})

endif()
