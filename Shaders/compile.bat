set glslsc=C:/VulkanSDK/1.3.236.0/Bin/glslc.exe 

%glslsc% simple.vert -o vert.spv
%glslsc% simple.frag -o frag.spv

pause