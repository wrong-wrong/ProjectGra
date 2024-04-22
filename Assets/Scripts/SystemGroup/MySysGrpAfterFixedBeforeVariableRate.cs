using Unity.Entities;

namespace ProjectGra
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
    public partial class MySysGrpAfterFixedBeforeVariableRate : ComponentSystemGroup
    {

    }
}