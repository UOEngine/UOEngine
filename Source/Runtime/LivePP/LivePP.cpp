#include "LivePP/LivePP.h"

#include "LivePP/API/x64/LPP_API_x64_CPP.h"

namespace
{
	lpp::LppDefaultAgent LPPAgent;
}

LivePP::LivePP()
{
	LPPAgent = lpp::LppCreateDefaultAgent(nullptr, L"D:\\UODev\\UOEngineGitHub\\ThirdParty\\LPP_2_9_1\\LivePP");

	if (lpp::LppIsValidDefaultAgent(&LPPAgent) == false)
	{
		return;
	}

	LPPAgent.EnableModule(lpp::LppGetCurrentModulePath(), lpp::LPP_MODULES_OPTION_ALL_IMPORT_MODULES, nullptr, nullptr);
}

LivePP::~LivePP()
{
	lpp::LppIsValidDefaultAgent(&LPPAgent);
}
