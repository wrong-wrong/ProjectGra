using Unity.Entities;
using Unity.Physics.Systems;

namespace ProjectGra
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial class MySysGrpUpdateAfterPhysicsSysGrp : ComponentSystemGroup
    {

    }
}