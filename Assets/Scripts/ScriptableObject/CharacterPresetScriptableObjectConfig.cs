using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyCharacterPresetSO")]
    public class CharacterPresetScriptableObjectConfig : ScriptableObject
    {

        public Sprite CharacterSprite;
        public string CharacterName;
        public List<int> AffectedAttributeIdx;
        //0MaxHp 
        //1HpRegain
        //2Armor
        //3Speed %
        //4Range 
        //5CriticalHitChance %
        //6Damage %
        //7Melee %
        //8Ranged %
        //9Element %
        //10AttackSpeed %
        //11 life steal %
        //12 dodge %
        public List<int> BonusedValueList;
    }
}