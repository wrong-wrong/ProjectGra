using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class MeleeWeaponComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<MeleeWeaponComAndAuthoring>
        {
            public override void Bake(MeleeWeaponComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<AttackCurDamage>(entity);

            }
        }
    }
    
}