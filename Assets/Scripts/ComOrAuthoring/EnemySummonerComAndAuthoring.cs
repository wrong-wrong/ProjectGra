using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemySummonerComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<EnemySummonerComAndAuthoring>
        {
            public override void Bake(EnemySummonerComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnemySummonerAttack>(entity);
                AddComponent<EnemySummonerDeath>(entity);
                AddComponent<EnemySummonerMovement>(entity);
            }
        }
    }
    public struct EnemySummonerDeath : IComponentData
    {
        public float Timer;
    }

    public struct EnemySummonerAttack : IComponentData
    {
        public float AttackCooldown;
    }
    public struct EnemySummonerMovement : IComponentData
    {
        public float2 tarDirMulSpeed;
        public float floatingTimer;
    }
}