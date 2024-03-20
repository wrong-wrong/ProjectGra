using Unity.Entities;
using UnityEngine;

namespace OOPExperiment
{
    public class PalumonAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PalumonAuthoring>
        {
            public override void Bake(PalumonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new ExperiPalumonHealthPoint { HealthPoint = 100f });
                AddComponent(entity, new ExperiPalumonTypeComponent { Type = ExperiPalumonType.Water });
                AddComponent(entity, new ExperiPalumonSpeed { MoveSpeed = 3f});
                AddComponent(entity,
                    new ExperiPalumonResistance
                    {
                        WaterResistance = 0.8f,
                        FireResistance = 0.8f,
                        GrassResistance = 0.8f,
                        GroundResistance = 0.8f,
                        EletricResistance = 0.8f,
                        IceResistance = 0.8f,
                        DragonResistance = 0.8f,
                        DarkResistance = 0.8f
                    });
                AddComponent<ExperiPalumonTargetEntity>(entity);
            }
        }

    }

}