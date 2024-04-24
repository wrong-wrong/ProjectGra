using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyUpgradeSO")]
    public class UpgradeScriptableObjectConfig : ScriptableObject
    {
        //public int AffectedAttributeIdx;   
        //First SO in the config list should be affecting first attribute

        public string UpgradeName;
        public int4 BonusValueInFourLevel;
    }
}