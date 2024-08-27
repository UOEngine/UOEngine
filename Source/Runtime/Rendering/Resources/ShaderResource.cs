using Silk.NET.SPIRV.Reflect;
using Silk.NET.Vulkan;
using System.Collections.Generic;

namespace UOEngine.Runtime.Rendering.Resources
{
    readonly struct DescriptorSetLayoutData
    {
        readonly public uint                               SetNumber;
        readonly public List<DescriptorSetLayoutBinding>   Bindings;

        public DescriptorSetLayoutData(uint setNumber, List<DescriptorSetLayoutBinding> bindings)
        {
            SetNumber = setNumber;
            Bindings = bindings;
        }
    }

    public class ShaderResource
    {
        public ReflectShaderModule                      VertexShaderModule { get; private set; }
        public ReflectShaderModule                      FragmentShaderModule { get; private set; }

        public IReadOnlyList<DescriptorSetLayoutBinding>   DescriptorSetLayouts => _descriptorSetLayoutsInfo;

        public ReadOnlySpan<DescriptorSetLayoutBinding>    Test => _descriptorSetLayoutsInfo;

        private DescriptorSetLayoutBinding[]                _descriptorSetLayoutsInfo = [];

        public ShaderResource(ReadOnlySpan<byte> vertexByteCode, ReadOnlySpan<byte> fragmentByteCode)
        {
            VertexShaderModule = Create(vertexByteCode);
            FragmentShaderModule = Create(fragmentByteCode);
        }

        public void Generate()
        {
            Generate(VertexShaderModule);
            Generate(FragmentShaderModule);
        }

        public unsafe void Generate(ReflectShaderModule shaderModule)
        {
            uint numDescriptors = 0;

            Reflect.GetApi().EnumerateDescriptorSets(&shaderModule, &numDescriptors, null);

            ReflectDescriptorSet* reflectDescriptorSets = stackalloc ReflectDescriptorSet[(int)numDescriptors];

            Reflect.GetApi().EnumerateDescriptorSets(&shaderModule, &numDescriptors, &reflectDescriptorSets);

            _descriptorSetLayoutsInfo = new DescriptorSetLayoutBinding[numDescriptors];

            int index = 0;

            for (int i = 0; i < numDescriptors; i++)
            {
                ReflectDescriptorSet set = reflectDescriptorSets[i];

                //List<DescriptorSetLayoutBinding> layoutBindings = new((int)set.BindingCount);

                for (int j = 0; j < set.BindingCount; j++)
                {
                    DescriptorBinding binding = *set.Bindings[j];
                    DescriptorSetLayoutBinding layoutBinding = new()
                    {
                        Binding = binding.Binding,
                        DescriptorCount = 1,
                        DescriptorType = (Silk.NET.Vulkan.DescriptorType)binding.DescriptorType,
                        StageFlags = (ShaderStageFlags)shaderModule.ShaderStage
                    };

                    _descriptorSetLayoutsInfo[index++] = layoutBinding;
                }

                //_descriptorSetLayoutsInfo.Add(new(set.Set, layoutBindings));
            }

        }

        private static unsafe ReflectShaderModule Create(ReadOnlySpan<byte> byteCode)
        {
            ReflectShaderModule reflectShaderModule;

            Reflect.GetApi().CreateShaderModule((nuint)byteCode.Length, byteCode, &reflectShaderModule);

            return reflectShaderModule;
        }
    }
}
