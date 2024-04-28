using Unity.Entities;
using UnityEngine;
namespace ProjectGra
{
    public class SpawneeComAndAuthoring : MonoBehaviour
    {
        public float SpawneeTimer;
        public bool IsMovedBySpawneeMove;
        public bool IsControlledBySpawneeTimer;
        public int BasicDamage;
        //public bool 
        public class Baker : Baker<SpawneeComAndAuthoring>
        {
            public override void Bake(SpawneeComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                

                AddComponent<SpawneeTimer>(entity, new SpawneeTimer { Value = authoring.SpawneeTimer});
                if (!authoring.IsControlledBySpawneeTimer)
                {
                    SetComponentEnabled<SpawneeTimer>(entity, false);
                }
                AddComponent(entity, new AttackCurDamage { damage = authoring.BasicDamage});
                AddComponent<SpawneeMoveTag>(entity);
                if (authoring.IsMovedBySpawneeMove) SetComponentEnabled<SpawneeMoveTag>(entity, true);
                else SetComponentEnabled<SpawneeMoveTag>(entity, false);
            }
        }
    }
    public struct SpawneeMoveTag : IComponentData,IEnableableComponent { }
    public struct SpawneeTimer : IComponentData, IEnableableComponent
    {
        public float Value;
    }
    public struct AttackCurDamage : IComponentData
    {
        public int damage;
    }


}
