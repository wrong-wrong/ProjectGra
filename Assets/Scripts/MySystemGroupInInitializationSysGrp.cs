using Unity.Entities;

namespace ProjectGra
{
    [UpdateInGroup(typeof(InitializationSystemGroup),OrderLast = true)]
    public partial class MySystemGroupInInitializationSysGrp : ComponentSystemGroup
    {

    }

}