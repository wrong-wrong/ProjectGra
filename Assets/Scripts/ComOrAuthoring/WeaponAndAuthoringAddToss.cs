using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    public class WeaponAndAuthoringAddToss : MonoBehaviour
    {

        public List<WeaponScriptableObjectConfig> WeaponSOList;

        public class Baker : Baker<WeaponAndAuthoringAddToss>
        {
            public override void Bake(WeaponAndAuthoringAddToss authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var normalConfigBuffer = AddBuffer<WeaponConfigInfoCom>(entity);
                var wpNameMap = new Dictionary<int, string>();
                var wpBasePriceMap = new Dictionary<int, int>();
                var wpColorMap = new Dictionary<int, Color>();
                for (int i = 0, count = authoring.WeaponSOList.Count; i < count; ++i)
                {
                    var so = authoring.WeaponSOList[i];
                    Debug.Log(so.name);
                    Debug.Log(so.SpawneePrefabs.name);
                    normalConfigBuffer.Add(new WeaponConfigInfoCom
                    {
                        //color = new float3(so.color.r, so.color.g, so.color.b),
                        WeaponIndex = so.WeaponIndex,
                        WeaponPrefab = GetEntity(so.WeaponModel, TransformUsageFlags.Dynamic),
                        SpawneePrefab = GetEntity(so.SpawneePrefabs, TransformUsageFlags.Dynamic),
                        DamageBonus = new float4
                        {
                            x = so.MeleeBonus,
                            y = so.RangedBonus,
                            z = so.ElementBonus,
                            w = so.AttackSpeedBonus
                        },
                        BasicDamage = so.BasicDamage,
                        WeaponCriticalHitChance = so.WeaponCriticalHitChance,
                        WeaponCriticalHitRatio = so.WeaponCriticalHitRatio,
                        Cooldown = so.Cooldown,
                        Range = so.Range,
                        WeaponPositionOffset = so.WeaponPositionOffsetRelativeToCameraTarget,
                    });
                    wpColorMap[so.WeaponIndex] = so.color;
                    wpNameMap[so.WeaponIndex] = so.WeaponName;
                    wpBasePriceMap[so.WeaponIndex] = so.BasePrice;
                }
                AddComponentObject<WeaponManagedAndMonoOnlyConfigCom>(entity, new WeaponManagedAndMonoOnlyConfigCom
                {
                    weaponNameMap = wpNameMap,
                    weaponBasePriceMap = wpBasePriceMap,
                    weaponColorInsteadOfIconMap = wpColorMap,
                });
                Debug.Log("Weapon Baking");
            }
        }
    }

    public struct WeaponIdxToConfigCom : IComponentData
    {
        public NativeHashMap<int, WeaponConfigInfoCom> wpNativeHashMap;
    }
    [InternalBufferCapacity(0)]
    public struct WeaponConfigInfoCom : IBufferElementData
    {
        //readonly 
        public int WeaponIndex;
        public Entity WeaponPrefab;  //this is Prefab
        public Entity SpawneePrefab;
        public float3 WeaponPositionOffset;
        public int BasicDamage;
        public float4 DamageBonus;
        public float WeaponCriticalHitChance;
        public float WeaponCriticalHitRatio;
        public float Cooldown;
        public float Range;
    }
    public class WeaponManagedAndMonoOnlyConfigCom : IComponentData
    {
        public Dictionary<int, Color> weaponColorInsteadOfIconMap; 
        public Dictionary<int, string> weaponNameMap;
        public Dictionary<int, int> weaponBasePriceMap;
    }


    public struct MainWeaponState : IComponentData
    {
        public int WeaponIndex; // -1 indicating the weapon not set

        //field underneath needs to change and set when drag to another slot;
        public Entity WeaponModel; // read by follow system  // needs to be the instantiated Entity
        public float3 WeaponPositionOffset; // read by follow system

        //InGameState 
        public float RealCooldown;

        //Need to be set according to Weapon Config and Player Attribute
        public float Cooldown;                //affected by player's attribute  ,used by spawnee system
        public int DamageAfterBonus;          //affected by player's attribute  ,need to set to spawnee
        public float WeaponCriticalHitChance; //affected by player's attribute  ,used by spawnee system
        public float WeaponCriticalHitRatio;

        //Need to set spawnee's curDamage & timer
        public Entity SpawneePrefab;

        //dont set at init
        public LocalTransform mainWeaponLocalTransform;  // set by follow system , read by other system

        //Useless field
        public float Range;
        public int BasicDamage;
        public float4 DamageBonus;
    }

    [InternalBufferCapacity(3)]
    public struct AutoWeaponState : IBufferElementData
    {
        public int WeaponIndex; // -1 indicating the weapon not set
        public LocalTransform autoWeaponLocalTransform;  // set by follow system , read by Spawnee system

        //field underneath needs to change and set when drag to another slot;
        public Entity WeaponModel; // read by follow system
        public float3 WeaponPositionOffset; // read by follow system

        //InGameState 
        public float RealCooldown;  // used by Spawnee system

        //Need to be set according to Weapon Config and Player Attribute
        public float WeaponCriticalHitChance; //affected by player's attribute  ,used by spawnee system to generate critHit
        public float WeaponCriticalHitRatio;  //affected by player's attribute  ,used by spawnee system when critHit
        public float Cooldown;                //affected by player's attribute  ,used by spawnee system to set weapon's cd  
        public float Range;                   //affected by player's attribute  ,need to set to spawnee
        public int DamageAfterBonus;          //affected by player's attribute  ,need to set to spawnee
        //set spawneePrefab 's curDamage & timer
        public Entity SpawneePrefab;          //used by spawnee system to set spawnee's curDamage

        //only used when needs to update attribute 
        public int BasicDamage;
        public float4 DamageBonus;
    }

}