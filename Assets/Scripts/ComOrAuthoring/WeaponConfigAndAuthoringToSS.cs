using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    public class WeaponConfigAndAuthoringToSS : MonoBehaviour
    {

        public List<WeaponScriptableObjectConfig> WeaponSOList;

        public class Baker : Baker<WeaponConfigAndAuthoringToSS>
        {
            public override void Bake(WeaponConfigAndAuthoringToSS authoring)
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
                    if (!so.IsMeleeWeapon)
                    {
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
                            IsMeleeWeapon = so.IsMeleeWeapon,
                        });
                    }
                    else
                    {
                        Debug.Log("This is Melee Weapon");
                        normalConfigBuffer.Add(new WeaponConfigInfoCom
                        {
                            WeaponIndex = so.WeaponIndex,
                            WeaponPrefab = GetEntity(so.WeaponModel, TransformUsageFlags.Dynamic),
                            //SpawneePrefab = Entity.Null,
                            WeaponPositionOffset = so.WeaponPositionOffsetRelativeToCameraTarget,
                            BasicDamage = so.BasicDamage,
                            DamageBonus = new float4
                            {
                                x = so.MeleeBonus,
                                y = so.RangedBonus,
                                z = so.ElementBonus,
                                w = so.AttackSpeedBonus
                            },
                            WeaponCriticalHitChance = so.WeaponCriticalHitChance,
                            WeaponCriticalHitRatio = so.WeaponCriticalHitRatio,
                            Cooldown = so.Cooldown,
                            Range = so.Range,
                            IsMeleeWeapon = so.IsMeleeWeapon
                        });
                    }

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
        public float3 WeaponPositionOffset;
        public int BasicDamage;
        public float4 DamageBonus;
        public float WeaponCriticalHitChance;
        public float WeaponCriticalHitRatio;
        public float Cooldown;
        public float Range;
        public Entity WeaponPrefab;  //this is Prefab
        public Entity SpawneePrefab;
        public bool IsMeleeWeapon;
    }
    public class WeaponManagedAndMonoOnlyConfigCom : IComponentData
    {
        public Dictionary<int, Color> weaponColorInsteadOfIconMap; 
        public Dictionary<int, string> weaponNameMap;
        public Dictionary<int, int> weaponBasePriceMap;
    }


    public struct MainWeapon : IComponentData
    {
        public int WeaponIndex; // -1 indicating the weapon not set
        public float3 WeaponPositionOffset; // read by follow system

        //InGameState 
        public float RealCooldown;
        //Melee weapon only
        public float MeleeShootingTimer;
        public float3 MeleeTargetPosition;
        public float MeleeRealShootingTimer;  // used to lerp 
        public float3 MeleeOriginalPosition;

        //Need to be set according to Weapon Config and Player Attribute
        public float Cooldown;                //affected by player's attribute  ,used by spawnee system
        public int DamageAfterBonus;          //affected by player's attribute  ,need to set to spawnee
        public float WeaponCriticalHitChance; //affected by player's attribute  ,used by spawnee system
        public float WeaponCriticalHitRatio;
        //Used when weapon is melee type
        public float Range;


        //Ranged weapon type only
        // this LocalTransform should also be set using value of model prefab, 
        // because we are using this transform to spawn spawnee and setting its value in cameraTargetFollow and then using it to set model's transform 
        public LocalTransform mainWeaponLocalTransform;  // set by follow system , read by other system 
        public Entity SpawneePrefab;

        public Entity WeaponModel; // read by follow system  // needs to be the instantiated Entity
        public WeaponState WeaponCurrentState;
        public bool IsMeleeWeapon;
        //Useless field
        //public int BasicDamage;
        //public float4 DamageBonus;
    }

    [InternalBufferCapacity(3)]
    public struct AutoWeaponBuffer : IBufferElementData
    {
        public int WeaponIndex; // -1 indicating the weapon not set
        public float3 WeaponPositionOffset; // read by follow system
        //InGameState 
        public float RealCooldown;  // used by Spawnee system
        //Need to be set according to Weapon Config and Player Attribute
        public float WeaponCriticalHitChance; //affected by player's attribute  ,used by spawnee system to generate critHit
        public float WeaponCriticalHitRatio;  //affected by player's attribute  ,used by spawnee system when critHit
        public float Cooldown;                //affected by player's attribute  ,used by spawnee system to set weapon's cd  
        public float Range;                   //affected by player's attribute  ,need to set to spawnee
        public int DamageAfterBonus;          //affected by player's attribute  ,need to set to spawnee
        
        //Melee weapon only
        public float MeleeShootingTimer;
        public float3 MeleeTargetPosition;
        public float MeleeRealShootingTimer;  // used to lerp 
        public float3 MeleeOriginalPosition;
        //Ranged weapon only 
        //set spawneePrefab 's curDamage & timer
        // this LocalTransform should also be set using value of model prefab, 
        // because we are using this transform to spawn spawnee and setting its value in cameraTargetFollow and then using it to set model's transform 
        public LocalTransform autoWeaponLocalTransform;  // set by follow system , read by Spawnee system
        public Entity SpawneePrefab;          //used by spawnee system to set spawnee's curDamage

        public Entity WeaponModel; // read by follow system

        public WeaponState WeaponCurrentState;
        public bool IsMeleeWeapon;
        //only used when needs to update attribute 
        //public int BasicDamage;
        //public float4 DamageBonus;
    }
    //[InternalBufferCapacity(3)]
    //public struct AutoWeaponStateMachineBuffer : IBufferElementData
    //{
    //    public AutoWeaponState CurrentState;
    //}
    public enum WeaponState
    {
        None,
        Thrust,
        Retrieve,
        Cooldown,
    }

}