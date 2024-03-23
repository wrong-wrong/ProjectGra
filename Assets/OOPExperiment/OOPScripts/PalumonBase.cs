using UnityEngine;

namespace OOPExperiment
{
    public class PalumonBase : MonoBehaviour
    {
        public Transform Target;
        public float MoveSpeed;

        public PalumonType palumonType;

        public float WaterResistance;
        public float FireResistance;
        public float GrassResistance;
        public float GroundResistance;
        public float EletricResistance;
        public float IceResistance;
        public float DragonResistance;
        public float DarkResistance;

        public float HealthPoint;
        public PalumonBase()
        {
            HealthPoint = 100;
            WaterResistance = 0.8f;
            FireResistance = 0.8f;
            GrassResistance = 0.8f;
            GroundResistance = 0.8f;
            EletricResistance = 0.8f;
            IceResistance = 0.8f;
            DragonResistance = 0.8f;
            DarkResistance = 0.8f;
        }

        public void GetHurt(PalumonType Type, int damage)
        {
            float resistance = 0.8f;
            switch (Type)
            {
                case PalumonType.Water:
                    resistance = WaterResistance;
                    break;
                case PalumonType.Fire:
                    resistance = FireResistance;
                    break;
                case PalumonType.Grass:
                    resistance = GrassResistance;
                    break;
                case PalumonType.Ground:
                    resistance = GroundResistance;
                    break;
                case PalumonType.Eletric:
                    resistance = EletricResistance;
                    break;
                case PalumonType.Ice:
                    resistance = IceResistance;
                    break;
                case PalumonType.Dragon:
                    resistance = DragonResistance;
                    break;
                case PalumonType.Dark:
                    resistance = DarkResistance;
                    break;
            }
            HealthPoint -= damage * (1 - resistance);
            //Debug.Log(HealthPoint);
        }
        public void Update()
        {
            if(Target != null)
            {
                FollowTarget();
            }
        }
        public void FollowTarget()
        {
            Vector3 dir = Target.position - transform.position;
            transform.position += dir.normalized * Time.deltaTime * MoveSpeed;
            //transform.rotation = Quaternion.LookRotation(dir);
        }

    }


}