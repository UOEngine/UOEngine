#pragma once

class ID3D12GraphicsCommandList;

class RenderContext
{
public:

								RenderContext();

	void						Begin();
	void						End();

	void						BeginRenderPass();
	void						EndRenderPass();

private:

	ID3D12GraphicsCommandList* GraphicsCommandList;

};

