using Unity.Entities;
namespace ProjectGra
{
    [UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public partial class MySysGrpUpdateBeforeFixedStepSysGrp : ComponentSystemGroup
    {
        
    }

}