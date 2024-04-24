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

        public WeaponIdxToWpDataConfigCom WeaponMapCom;
        public WeaponManagedAndMonoOnlyConfigCom WeaponManagedConfigCom;
        public static SOConfigSingleton Instance;
        public List<Color> levelBgColor;
        public List<Color> levelBgColorLight;
        public List<UpgradeScriptableObjectConfig> UpgradeSOList;
        private int weaponCount;
        private Random random;
        public int Wave;
        public List<ItemScriptableObjectConfig> ItemSOList;
        
        public List<WeaponCategoryScriptableObjectConfig> WeaponCategorySOList;

        public List<float4> RaritiesInDifferentWaveList;

        private List<List<int>> _itemIdxListInFourTiers;
        //// idx of attribute affected by category 0 would be in the list stored at list[0]; list[categoryIdx] is the list of idx of its affecting attribute
        //public List<List<int>> CategoryToAffectedAttributeIdxMappingList;
        //// bonus value of different count in category 0 would be in the list stored at list[0]; list[categoryIdx] is its bonus value Int4
        //public List<List<int4>> CategoryToAffectedAttributeValueMappingList; 

        public void Awake()
        {
            if(Instance != null) 
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("SOConfigSingleton - Awake - CateSOList Count:" + WeaponCategorySOList.Count);

            _itemIdxListInFourTiers = new List<List<int>>(4);
            for(int i = 0; i < 4; ++i)
            {
                _itemIdxListInFourTiers.Add(new List<int>());
            }

            for(int i = 0, n = ItemSOList.Count; i < n; i++)
            {
                var item = ItemSOList[i];
                _itemIdxListInFourTiers[item.ItemLevel].Add(item.ItemIdx);
            }
            for(int i = 0; i < 4; ++i)
            {
                Debug.Log("ItemTier" + i + " have Count :"+_itemIdxListInFourTiers[i].Count);
            }
            for(int i = 0; i < 19; ++i)
            {
                var f = RaritiesInDifferentWaveList[i];
                f[1] += f[0];
                f[2] += f[1];
                f[3] += f[2];
                RaritiesInDifferentWaveList[i] = f;
                //if (f[3] > 1)
                //{
                //    Debug.Log("Rarities bigger than 1 at idx : " + i + " - " + f[3]);
                //}
            }
            Debug.LogWarning("RaritiesList only have 19 elements, be cautious about waves");


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
            //return random.NextInt(4);
            var f = random.NextFloat();
            //Debug.Log("Random Level float - " + f);
            var r = RaritiesInDifferentWaveList[Wave];
            if(f < r[0])
            {
                return 0;
            }else if(f < r[1])
            {
                return 1;
            }else if (f < r[2])
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        public int GetRandomAttributeIdx()
        {
            return random.NextInt(13);
        }
        public int GetRandomItemConfigIdxFromRarities(int tier)
        {
            var list = _itemIdxListInFourTiers[tier];
            return list[random.NextInt(list.Count)];
        }
        public int GetItemBasePrice(int ItemIdx)
        {
            return ItemSOList[ItemIdx].ItemBasePrice;
        }
        public bool ShopItemRerollNextTypeIsWeapon()
        {
            return random.NextFloat() > 0.4f;
        }
    }
}