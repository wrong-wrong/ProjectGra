using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGra
{
    public class TestingEntityComAndAuthoring : MonoBehaviour
    {
        public Mesh mesh;
        public class Baker : Baker<TestingEntityComAndAuthoring>
        {
            public override void Bake(TestingEntityComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<TestingEntityTag>(entity);
                AddComponent(entity, new URPMaterialPropertyBaseColor { Value = new float4(1,1,1,1)});
                //AddComponent<SpawningTimer>(entity);
                //var entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
                //var batchMeshId = entitiesGraphicsSystem.RegisterMesh(authoring.mesh);
                //AddComponent(entity, new BakedBatchMeshId { bakedBatchMeshId = batchMeshId });
                //Debug.Log("Trying to bake mesh to BatchMeshID : " + batchMeshId.ToString());
                //Debug.Log(batchMeshId.ToString());
            }
        }
    }
    public struct TestingEntityTag : IComponentData { }
    //public struct SpawningTimer : IComponentData, IEnableableComponent
    //{
    //    public float time;
    //}
}