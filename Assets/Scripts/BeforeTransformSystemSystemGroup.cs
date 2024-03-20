using Unity.Entities;
using Unity.Transforms;
namespace ProjectGra
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class BeforeTransformSystemSystemGroup : ComponentSystemGroup { }
}