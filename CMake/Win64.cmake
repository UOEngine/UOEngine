# /GR-
# Disables Run-Time Type Information (RTTI) — disables use of dynamic_cast and typeid.

# /Gm-
# Disables minimal rebuild — it's obsolete and can cause problems with parallel builds.

# /fp:except-
# Disables floating-point exceptions (makes floating-point behavior faster and less strict).

# /nologo
# Hides the MSVC startup banner when compiling.

set(CMAKE_CXX_FLAGS "/GR- /Gm- /fp:except /MP /nologo")

message(STATUS "CMAKE_SYSTEM_NAME: ${CMAKE_SYSTEM_NAME}")
message(STATUS "CMAKE_SIZEOF_VOID_P: ${CMAKE_SIZEOF_VOID_P}")