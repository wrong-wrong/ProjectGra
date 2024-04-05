using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    public class WeaponSOConfigSingleton : MonoBehaviour
    {
        //public List<WeaponScriptableObjectConfig> WeaponSOList;
        //public Dictionary<int, Color> WeaponIdxToColor;

        public WeaponIdxToConfigCom MapCom;
        public WeaponManagedConfigCom ManagedConfigCom;
        public static WeaponSOConfigSingleton Instance;
        public List<Color> bgColor;
        private int weaponCount;
        private Random random;
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
            weaponCount = MapCom.wpNativeHashMap.Count;
            random = Random.CreateFromIndex(0);
        }
        //TODO : return weapon idx according to player's level
        public WeaponConfigInfoCom GetRandomWeaponConfig(int playerLevel)
        {
            return MapCom.wpNativeHashMap[random.NextInt(weaponCount)];
        }
        //TODO : return weapon level according to player's level
        public int GetRandomLevel(int playerLevel)
        {
            return random.NextInt(4);
        }
    }
}