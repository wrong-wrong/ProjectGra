using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    public class WeaponCategoryManager : MonoBehaviour
    {
        [SerializeField]private List<int> CategoryCount;
        public void Start()
        {
            //Debug.Log("WeaponCategoryManager - Start");
            CanvasMonoSingleton.Instance.OnWeaponAddOrDeleteAction += ModifyWeaponCategoryBonus;
            CategoryCount = new List<int>(SOConfigSingleton.Instance.WeaponCategorySOList.Count);
            for(int i = 0; i < CategoryCount.Capacity; i++)
            {
                CategoryCount.Add(0);
            }
        }
        public void OnDestroy()
        {
            CanvasMonoSingleton.Instance.OnWeaponAddOrDeleteAction -= ModifyWeaponCategoryBonus;
        }
        public void ModifyWeaponCategoryBonus(int weaponIdx, bool isAddingWeapon)
        {
            //using ManagedConfigCom to get CategoryIdx List of the weapon
            var categoryList = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponCategoryIdxListMap[weaponIdx];
            //using category Idx to get category SO config;
            for(int i = 0; i < categoryList.Count; i++)
            {
                var cateIdx = categoryList[i];
                var cateConfig = SOConfigSingleton.Instance.WeaponCategorySOList[cateIdx];

                var affectedAttributeIdxList = cateConfig.AffectedAttributeIdxList;
                var bonusValueList = cateConfig.BonusValueInDifferentCountList;

                // Need to know current category's weapon count, and whether add or minus one according to isAddingWeapon
                var currentCount = CategoryCount[cateIdx] + (isAddingWeapon ? 1 : -1);
                if (CategoryCount[cateIdx] < 0) 
                {
                    CategoryCount[cateIdx] = 0;
                    Debug.LogWarning("CategoryCount[cateIdx] of category should not under zero");
                }
                if (currentCount < 0)
                {
                    currentCount = 0;
                    Debug.LogWarning("activated currentCount of category should not under zero");
                }
                //One category may offer bonus to more than one attribute
                for (int j = 0; j < affectedAttributeIdxList.Count; j++)
                {
                    var attributeIdx = affectedAttributeIdxList[j];
                    //Debug.Log("CategoryCount[cateIdx] : " + CategoryCount[cateIdx]);
                    //Debug.Log("currentCount : " + currentCount);
                    int previousBonusValue;
                    int currentBonusValue;
                    if (CategoryCount[cateIdx] - 1 < 0)
                    {
                        previousBonusValue = 0;
                    }
                    else
                    {
                        previousBonusValue = bonusValueList[j][CategoryCount[cateIdx] - 1];
                    }
                    if(currentCount - 1<0)
                    {
                        currentBonusValue = 0;
                    }
                    else
                    {
                        currentBonusValue = bonusValueList[j][currentCount - 1];
                    }
                    // Player Attribute should be modified by value of  (+ CurrentBonusValue - PreviousBonusValue), while value can be indexed by count of the category
                    PlayerDataModel.Instance.AddAttributeValWith(attributeIdx, currentBonusValue - previousBonusValue);
                    Debug.Log("CateManager - Called AddAttribute attriIdx :" + attributeIdx + " ; adding value : " + (currentBonusValue - previousBonusValue));
                }

                CategoryCount[cateIdx] = currentCount;

            }
        }
        public int GetCategoryActivatedCount(int categoryIdx)
        {
            return CategoryCount[categoryIdx];
        }
    }
}