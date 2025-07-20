# /GR-
# Disables Run-Time Type Information (RTTI) — disables use of dynamic_cast and typeid.

# /Gm-
# Disables minimal rebuild — it's obsolete and can cause problems with parallel builds.

# /fp:except-
# Disables floating-point exceptions (makes floating-point behavior faster and less strict).

# /nologo
# Hides the MSVC startup banner when compiling.

set(CMAKE_CXX_FLAGS "/GR- /Gm- /fp:except /MP /nologo")

if(ENABLE_ASAN)
    message(STATUS "Enabling ASAN support")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /fsanitize=address")
endif()
