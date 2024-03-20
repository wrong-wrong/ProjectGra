using Unity.Entities;
using UnityEngine;

public class SystemExecuteAuthoring : MonoBehaviour
{
    public bool OOPExperiment;
    public bool TestScene;
    public class Baker : Baker<SystemExecuteAuthoring>
    {
        public override void Bake(SystemExecuteAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            if (authoring.OOPExperiment) AddComponent<OOPExperimentExecuteTag>(entity);
            if (authoring.TestScene) AddComponent<TestSceneExecuteTag>(entity);
        }
    }
}

public struct TestSceneExecuteTag : IComponentData { }
public struct OOPExperimentExecuteTag : IComponentData { }
