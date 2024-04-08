using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    public class SOConfigSingleton : MonoBehaviour
    {
        //public List<WeaponScriptableObjectConfig> WeaponSOList;
        //public Dictionary<int, Color> WeaponIdxToColor;

        public WeaponIdxToConfigCom WeaponMapCom;
        public WeaponManagedAndMonoOnlyConfigCom WeaponManagedConfigCom;
        public static SOConfigSingleton Instance;
        public List<Color> levelBgColor;
        public List<Color> levelBgColorLight;
        public List<UpgradeScriptableObjectConfig> UpgradeSOList;
        private int weaponCount;
        private Random random;

        public List<ItemScriptableObjectConfig> ItemSOList;
        public void Awake()
        {
            if(Instance != null) 
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        public void InitWeaponSOSingleton()
        {
            weaponCount = WeaponMapCom.wpNativeHashMap.Count;
            random = Random.CreateFromIndex(0);
        }
        //TODO : return weapon idx according to player's level
        public WeaponConfigInfoCom GetRandomWeaponConfig()
        {
            return WeaponMapCom.wpNativeHashMap[random.NextInt(weaponCount)];
        }
        //TODO : return weapon level according to player's level
        public int GetRandomLevel()
        {
            return random.NextInt(4);
        }
        public int GetRandomAttributeIdx()
        {
            return random.NextInt(11);
        }
        public int GetRandomItemConfigIdx()
        {
            return random.NextInt(ItemSOList.Count);
        }
        public int GetItemCurrentPrice(int ItemIdx)
        {
            return ItemSOList[ItemIdx].ItemBasePrice;
        }
        public bool ShopItemRerollNextTypeIsWeapon()
        {
            return random.NextFloat() > 0.4f;
        }
    }
}