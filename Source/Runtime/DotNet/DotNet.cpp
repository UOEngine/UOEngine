#include "DotNet/DotNet.h"

#include <windows.h>

#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>

#include "Core/Containers/String.h"

namespace
{
	// Globals to hold hostfxr exports
	hostfxr_initialize_for_dotnet_command_line_fn	InitForCmdLineFunction = nullptr;
	hostfxr_initialize_for_runtime_config_fn		InitForConfigFunction = nullptr;
	hostfxr_get_runtime_delegate_fn					HostFxrGetRuntimeFunction = nullptr;
	hostfxr_run_app_fn								RunAppFunction = nullptr;
	hostfxr_close_fn								HostFxrCloseFunction = nullptr;

	void* LoadLibrary(const char_t* path)
	{
		HMODULE h = ::LoadLibraryW(path);

		return (void*)h;
	}

	void* GetExport(void* h, const char* name)
	{
		void* f = ::GetProcAddress((HMODULE)h, name);

		return f;
	}

	// Using the nethost library, discover the location of hostfxr and get exports
	bool LoadHostFxr(const char_t* assembly_path)
	{
		get_hostfxr_parameters params{ sizeof(get_hostfxr_parameters), assembly_path, nullptr };
		// Pre-allocate a large buffer for the path to hostfxr
		char_t buffer[MAX_PATH];
		size_t buffer_size = sizeof(buffer) / sizeof(char_t);
		int rc = get_hostfxr_path(buffer, &buffer_size, &params);

		if (rc != 0)
			return false;

		// Load hostfxr and get desired exports
		// NOTE: The .NET Runtime does not support unloading any of its native libraries. Running
		// dlclose/FreeLibrary on any .NET libraries produces undefined behavior.
		void* lib = LoadLibrary(buffer);

		InitForCmdLineFunction = (hostfxr_initialize_for_dotnet_command_line_fn)GetExport(lib, "hostfxr_initialize_for_dotnet_command_line");
		InitForConfigFunction = (hostfxr_initialize_for_runtime_config_fn)GetExport(lib, "hostfxr_initialize_for_runtime_config");
		HostFxrGetRuntimeFunction = (hostfxr_get_runtime_delegate_fn)GetExport(lib, "hostfxr_get_runtime_delegate");
		RunAppFunction = (hostfxr_run_app_fn)GetExport(lib, "hostfxr_run_app");
		HostFxrCloseFunction = (hostfxr_close_fn)GetExport(lib, "hostfxr_close");

		return (InitForConfigFunction && HostFxrGetRuntimeFunction && HostFxrCloseFunction);
	}

	// Load and initialize .NET Core and get desired function pointer for scenario
	load_assembly_and_get_function_pointer_fn GetDotNetLoadAssembly(const wchar_t* config_path)
	{
		// Load .NET Core
		void* load_assembly_and_get_function_pointer = nullptr;
		hostfxr_handle cxt = nullptr;

		int rc = InitForConfigFunction(config_path, nullptr, &cxt);

		if (rc != 0 || cxt == nullptr)
		{
			GAssert(false);

			HostFxrCloseFunction(cxt);
			
			return nullptr;
		}

		// Get the load assembly function pointer
		rc = HostFxrGetRuntimeFunction(cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer);
		if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
		{
			GAssert(false);
		}

		HostFxrCloseFunction(cxt);

		return (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
	}
}

DotNet& DotNet::sGet()
{
	static DotNet instance;

	return instance;
}

bool DotNet::Init()
{
	wchar_t buffer[MAX_PATH];
	
	// Get the full path of the executable
	DWORD length = GetModuleFileName(NULL, buffer, MAX_PATH);

	if (LoadHostFxr(buffer) == false)
	{
		return false;
	}

	//String config_path;
	
	// Hard coding these for now.
	const wchar_t* config_path = L"D:\\UODev\\UOEngineGitHub\\Intermediate\\Win64\\Binaries\\x64\\Debug\\x64\\Debug\\UOEngine.runtimeconfig.json";
	const char_t* assembly_path = L"D:\\UODev\\UOEngineGitHub\\Intermediate\\Win64\\Binaries\\x64\\Debug\\x64\\Debug\\UOEngine.dll";

	load_assembly_and_get_function_pointer_fn LoadAssemblyAndGetFunctionPointerFunction = GetDotNetLoadAssembly(config_path);

	HRESULT native_initialise_result = LoadAssemblyAndGetFunctionPointerFunction(assembly_path, TEXT("UOEngine.Game, UOEngine"), TEXT("NativeInitialise"), UNMANAGEDCALLERSONLY_METHOD, nullptr, (void**)&mGameInitialise);
	HRESULT game_update_result = LoadAssemblyAndGetFunctionPointerFunction(assembly_path, TEXT("UOEngine.Game, UOEngine"), TEXT("NativeUpdate"), UNMANAGEDCALLERSONLY_METHOD, nullptr, (void**)&mGameUpdate);

	if (FAILED(native_initialise_result) || FAILED(game_update_result))
	{
		GAssert(false);

		return false;
	}

	bool init_okay = mGameInitialise();

	if (init_okay == false)
	{
		GAssert(false);

		return false;
	}

	return true;
}

bool DotNet::LoadAssembly()
{
	return false;
}

void DotNet::ManagedUpdate(float DeltaSeconds)
{
	if (mGameUpdate == nullptr)
	{
		return;
	}

	mGameUpdate(DeltaSeconds);
}
