using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemySummonedExplosionComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<EnemySummonedExplosionComAndAuthoring>
        {
            public override void Bake(EnemySummonedExplosionComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SummonedExplosionCom { CurrentState = SummonExplosionState.Summoning});
            }
        }
    }

    public struct SummonedExplosionCom : IComponentData
    {
        public float CurrentSpeed;
        public float3 OriginalPosition;
        public SummonExplosionState CurrentState;
        public Entity FollowingEntity;
    }

    public enum SummonExplosionState
    {
        Summoning,  // follow the sommoner, slowly getting bigger
        Flying,     // follow the player, slowly getting smaller, and changing speed to stop speed then go to next state
        Explode,    // stay still but quickly getting bigger
        Collapse,   // slowly getting smaller
    }
}