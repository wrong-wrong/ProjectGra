using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    public class WeaponSOConfigSingleton : MonoBehaviour
    {
        //public List<WeaponScriptableObjectConfig> WeaponSOList;
        //public Dictionary<int, Color> WeaponIdxToColor;

        public AllWeaponMap weaponMap;
        public static WeaponSOConfigSingleton Instance;
        public void Awake()
        {
            if(Instance != null) 
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            //for(int i = 0,n = WeaponSOList.Count; i < n; ++i)
            //{
            //    WeaponIdxToColor[WeaponSOList[i].WeaponIndex] = WeaponSOList[i].color;
            //}
        }
        public void Start()
        {
            
        }

    }
}