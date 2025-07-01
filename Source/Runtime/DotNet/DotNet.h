#pragma once

class DotNet
{
public:

	static DotNet&				sGet();

	bool						Init();

	bool						LoadAssembly();

	void						ManagedUpdate(float DeltaSeconds);

private:

	using GameInitialiseFunction	= bool (*)(void);
	using GameUpdateFunction		= void (*)(float);

	GameInitialiseFunction		mGameInitialise = nullptr;
	GameUpdateFunction			mGameUpdate = nullptr;
	
};