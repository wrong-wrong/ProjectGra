using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyWeaponCategorySOConfig")]
    public class WeaponCategoryScriptableObjectConfig : ScriptableObject
    {
        public string CategoryName;
        public List<int> AffectedAttributeIdxList;
        public List<int4> BonusValueInDifferentCountList;
    }
}