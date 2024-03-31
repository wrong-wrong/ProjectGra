using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class NormalRangedComAndAuthoring : MonoBehaviour
    {
        //only used for initialization
        public float AttackCooldown;
        public float AttackVal;
        public class Baker:Baker<NormalRangedComAndAuthoring>
        {
            public override void Bake(NormalRangedComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NormalRangedAttack { AttackCooldown = authoring.AttackCooldown, AttackVal = authoring.AttackVal });
                AddComponent(entity, new NormalRangedDeath { });
            }
        }
    }

    public struct NormalRangedDeath : IComponentData
    {
        
    }
    
    public struct NormalRangedAttack : IComponentData
    {
        public float AttackVal;
        public float AttackCooldown;
    }
}