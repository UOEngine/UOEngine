#include <windows.h>
#if 0
#define WIN32_API					__stdcall
#define DIR_SEPARATOR '\\'
//#define MAX_PATH PATH_MAX

using int32 = LONG;

using string_t = std::basic_string<char_t>;

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
		assert(h != nullptr);
		return (void*)h;
	}

	void* GetExport(void* h, const char* name)
	{
		void* f = ::GetProcAddress((HMODULE)h, name);
		assert(f != nullptr);
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
	load_assembly_and_get_function_pointer_fn GetDotNetLoadAssembly(const char_t* config_path)
	{
		// Load .NET Core
		void* load_assembly_and_get_function_pointer = nullptr;
		hostfxr_handle cxt = nullptr;
		int rc = InitForConfigFunction(config_path, nullptr, &cxt);

		if (rc != 0 || cxt == nullptr)
		{
			assert(false);
			//std::cerr << "Init failed: " << std::hex << std::showbase << rc << std::endl;
			HostFxrCloseFunction(cxt);
			return nullptr;
		}

		// Get the load assembly function pointer
		rc = HostFxrGetRuntimeFunction(cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer);
		if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
		{
			assert(false);
			//std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc << std::endl;
		}

		HostFxrCloseFunction(cxt);

		return (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
	}
}

//std::vector<string_t> convert_argv(int argc, wchar_t* argv[]) 
//{
//	std::vector<string_t> args;
//
//	for (int i = 0; i < argc; ++i) 
//	{
//		int			size_needed = WideCharToMultiByte(CP_UTF8, 0, argv[i], -1, NULL, 0, NULL, NULL);
//		std::string	arg(size_needed, 0);
//
//		
//		WideCharToMultiByte(CP_UTF8, 0, argv[i], -1, &arg[0], size_needed, NULL, NULL);
//		arg.pop_back(); // Remove null terminator
//		args.push_back(arg.c_str());
//	}
//
//	return args;
//}

void PrintHResultMessage(HRESULT hr) {
	char* errorMsg = nullptr;

	FormatMessageA(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		nullptr,
		hr,
		0,  // Default language
		(LPSTR)&errorMsg,
		0,
		nullptr
	);

	if (errorMsg) 
	{
		LocalFree(errorMsg);
	}
}

using GameInitialiseFunction = bool (*)(void);
using GameUpdateFunction = void (*)(float);
using TestFunction = void (*)();

int __cdecl wmain(int argc, wchar_t* argv[])
{
	DotNet dotnet;

	dotnet.Init();

	//while (::IsDebuggerPresent() == false)
	//{
	//	::Sleep(500);
	//}

	// Get the current executable's directory
	// This sample assumes the managed assembly to load and its runtime configuration file are next to the host
	char_t host_path[MAX_PATH] = {};

	auto size = ::GetFullPathNameW(argv[0], sizeof(host_path) / sizeof(char_t), host_path, nullptr);
	assert(size != 0);

	string_t root_path = host_path;

	auto pos = root_path.find_last_of(DIR_SEPARATOR);
	assert(pos != string_t::npos);
	root_path = root_path.substr(0, pos + 1);

	//const string_t app_path = root_path + TEXT("x64\\Debug\\Game.dll");

	const string_t DotNetPath = root_path + TEXT("x64\\Debug\\");
	const string_t ConfigPath = DotNetPath + TEXT("UOEngine.runtimeconfig.json");
	const string_t AssemblyPath = DotNetPath + TEXT("UOEngine.dll");

	if (LoadHostFxr(nullptr) == false)
	{
		assert(false && "Failure: load_hostfxr()");

		return EXIT_FAILURE;
	}

	hostfxr_handle ctx = nullptr;

	//std::vector<string_t> args = convert_argv(argc, argv);

	// Convert to const char* array
	//std::vector<const char_t*> c_args = {app_path.c_str()};

	load_assembly_and_get_function_pointer_fn LoadAssemblyAndGetFunctionPointerFunction = GetDotNetLoadAssembly(ConfigPath.c_str());

	GameInitialiseFunction	GameInitialise = nullptr;
	GameUpdateFunction		GameUpdate = nullptr;
	
	HRESULT NativeInitialiseResult = LoadAssemblyAndGetFunctionPointerFunction(AssemblyPath.c_str(), TEXT("UOEngine.Game, UOEngine"), TEXT("NativeInitialise"), UNMANAGEDCALLERSONLY_METHOD, nullptr, (void**)&GameInitialise);
	HRESULT GameUpdateResult = LoadAssemblyAndGetFunctionPointerFunction(AssemblyPath.c_str(), TEXT("UOEngine.Game, UOEngine"), TEXT("NativeUpdate"), UNMANAGEDCALLERSONLY_METHOD, nullptr, (void**)&GameUpdate);
	
	bool bInitOkay = GameInitialise();

	bool bQuit = false;
	
	while (bQuit == false)
	{
		GameUpdate(1.0f);

		::Sleep(500);
	}

	//int InitResult = InitForCmdLineFunction(c_args.size(), c_args.data(), nullptr, &ctx);

	//assert(InitResult == 0);

	return 0;
}

#endif

#include "Core/Assert.h"
#include "Engine/Engine.h"

using MainFunctionPtr = int(*)();

extern "C" __declspec(dllexport) int MainProgram()
{
	if (GEngine.Init())
	{
		GEngine.Run();
	}

	int32 EngineReturnCode = GEngine.GetReturnCode();

	return EngineReturnCode;
}

int __cdecl wmain(int argc, wchar_t* argv[])
{
	HMODULE Module = GetModuleHandle(nullptr);

	GAssert(Module != nullptr);

	MainFunctionPtr MainFunction = (MainFunctionPtr)GetProcAddress(Module, "MainProgram");

	GAssert(MainFunction != nullptr);

	return MainFunction();
}