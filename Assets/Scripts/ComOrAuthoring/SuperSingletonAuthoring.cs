using UnityEngine;
using Unity.Entities;
namespace ProjectGra
{
    public struct SuperSingletonTag : IComponentData { }
    public class SuperSingletonAuthoring : MonoBehaviour
    {
        public class Baker : Baker<SuperSingletonAuthoring>
        {
            public override void Bake(SuperSingletonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<SuperSingletonTag>(entity);
                AddComponent<EnemyHpAndDmgModifierWithDifferentDifficulty>(entity);
            }
        }
    }

}