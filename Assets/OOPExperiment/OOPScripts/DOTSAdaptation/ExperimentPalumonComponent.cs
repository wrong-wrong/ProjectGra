using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace OOPExperiment
{
    public enum ExperiPalumonType
    {
        Water,
        Fire,
        Grass,
        Ground,
        Eletric,
        Ice,
        Dragon,
        Dark,
    }
    //public struct ExperiPalumon
    public struct ExperiPalumonSpeed : IComponentData
    {
        public float MoveSpeed;
    }
    public struct ExperiPalumonTypeComponent : IComponentData 
    {
        public ExperiPalumonType Type;
    }
    public struct ExperiPalumonResistance : IComponentData
    {
        public float WaterResistance;
        public float FireResistance;
        public float GrassResistance;
        public float GroundResistance;
        public float EletricResistance;
        public float IceResistance;
        public float DragonResistance;
        public float DarkResistance;
    }
    public struct ExperiPalumonHealthPoint : IComponentData
    {
        public float HealthPoint;
    }
    public struct ExperiPalumonTargetEntity : IComponentData
    {
        public Entity targetEntity;
    }

}