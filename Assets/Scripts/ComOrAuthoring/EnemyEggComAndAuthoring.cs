using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyEggComAndAuthoring : MonoBehaviour
    {
        //public GameObject ToBeHatchedEntityPrefab;
        public class Baker : Baker<EnemyEggComAndAuthoring>
        {
            public override void Bake(EnemyEggComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new EnemyEggTimerCom { Timer = 5f});
                AddComponent<EnemyEggToBeHatched>(entity);
            }
        }
    }
    public struct EnemyEggTimerCom : IComponentData 
    {
        public float Timer;
    }
    public struct EnemyEggToBeHatched : IComponentData 
    {
        public int PrefabIdx;
    }
}