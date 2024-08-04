set glslsc=C:/VulkanSDK/1.3.236.0/Bin/glslc.exe 

set path=..\Binaries\x64\Debug\net8.0\Shaders

%glslsc% simple.vert -o %path%\vert.spv
%glslsc% simple.frag -o %path%\frag.spv

pause