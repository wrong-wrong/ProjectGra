using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class ShopUIManager : MonoBehaviour
    {
        [SerializeField] GameObject gameItemPrefab;
        [SerializeField] string RerollString;
        [SerializeField] TextMeshProUGUI RerollPriceText;
        [SerializeField] int rerollPrice;
        private int rerollTimes;
        [Header("WeaponSlot")]
        [SerializeField] List<WeaponSlot> weaponSlotList;
        [Header("ShopItem")]
        [SerializeField] List<ShopItem> shopItemList;
        [SerializeField] TextMeshProUGUI shopMaterialCountText;
        [SerializeField] TextMeshProUGUI MainWeaponInfoText;
        [SerializeField] Button ShopContinueButton;
        [SerializeField] Button ShopRerollButton;
        [Header("Item Scroll View's Content RectTransform")]
        [SerializeField] RectTransform content;
        [Header("Player Attribute")]
        [SerializeField] TextMeshProUGUI playerAttributeText;




        private Vector3 tmpShopItemPosition; // for swap;
        private ShopItem tmpShopItem;
        private void Awake()
        {
            Debug.LogWarning("Current reroll is adding player's material");
            weaponSlotList[0].isMainSlot = true;
            ShopContinueButton.onClick.AddListener(CanvasMonoSingleton.Instance.ShopContinueButtonActionWrapper);
            ShopRerollButton.onClick.AddListener(RerollButtonClicked);
            PlayerDataModel.Instance.OnMaterialChanged += UpdateMaterialCount;
            PlayerDataModel.Instance.OnPlayerAttributeChanged += UpdateShopPlayerAttribute;
            for (int i = 0, n = shopItemList.Count; i < n; i++)
            {
                shopItemList[i].Init(this);
            }
            for(int i = 0, n = weaponSlotList.Count; i < n; ++i)
            {
                weaponSlotList[i].Init(this);
            }

        }
        private void OnDestroy()
        {
            ShopContinueButton.onClick.RemoveAllListeners();
            ShopRerollButton.onClick.RemoveAllListeners();
            PlayerDataModel.Instance.OnMaterialChanged -= UpdateMaterialCount;
            PlayerDataModel.Instance.OnPlayerAttributeChanged -= UpdateShopPlayerAttribute;
        }
        public void ShowShop()
        {
            UpdateMainWeaponInfo();
            ResetReroll();
            UpdateAllShopItemBuyState();
        }
        public void SetSlotWeaponIdx(int4 wpIdx, bool4 isMeleeWp)
        {
            for(int i = 0, n = weaponSlotList.Count;i < n; ++i)
            {
                weaponSlotList[i].InitSlot(wpIdx[i], isMeleeWp[i]);
            }
        }
        public int4 GetSlotWeaponIdx()
        {
            return new int4(weaponSlotList[0].WeaponIdx, weaponSlotList[1].WeaponIdx, weaponSlotList[2].WeaponIdx, weaponSlotList[3].WeaponIdx);
        }
        public int4 GetSlotWeaponLevelInShop()
        {
            return new int4(weaponSlotList[0].WeaponLevel, weaponSlotList[1].WeaponLevel, weaponSlotList[2].WeaponLevel, weaponSlotList[3].WeaponLevel);
        }
        public bool4 GetSlotWeaponIsMelee()
        {
            return new bool4(weaponSlotList[0].IsMeleeWeapon, weaponSlotList[1].IsMeleeWeapon, weaponSlotList[2].IsMeleeWeapon, weaponSlotList[3].IsMeleeWeapon);
        }

        public void AddGameItem(int itemIdx, int itemLevel,int currentPrice, int costMaterialCount)
        {
            //currentPrice to set item info, costMaterialCount to update player's material count;
            var item = Instantiate(gameItemPrefab, content);
            item.gameObject.transform.SetAsFirstSibling();
            item.GetComponent<SingleGameItem>().InitWithItemIdxAndLevel(itemIdx, itemLevel,currentPrice);
            PlayerDataModel.Instance.AddMaterialValWith(-costMaterialCount);
            var currrentItem = SOConfigSingleton.Instance.ItemSOList[itemIdx];
            for (int i = 0, n = currrentItem.AffectedAttributeIdx.Count; i < n; i++)
            {
                PlayerDataModel.Instance.AddAttributeValWith(currrentItem.AffectedAttributeIdx[i], currrentItem.BonusedValueList[i]);
            }
            //content.SetAsFirstSibling(content)
            //setParent and then SetAsFirstSibling
        }
        internal void RecycleGameItemWithGO(int itemIdx, int currentPrice, GameObject calledItemSlot)
        {
            PlayerDataModel.Instance.AddMaterialValWith(currentPrice);
            var itemConfig = SOConfigSingleton.Instance.ItemSOList[itemIdx];
            for (int i = 0, n = itemConfig.AffectedAttributeIdx.Count; i < n; i++)
            {
                //When you recycle an item, you modify player's attribute 
                PlayerDataModel.Instance.AddAttributeValWith(itemConfig.AffectedAttributeIdx[i], -itemConfig.BonusedValueList[i]);
            }
            Destroy(calledItemSlot);
        }


        public void RecycleWeaponFromSlot(int slotIdx)
        {
            // Weapon Category Bonus Should change
            CanvasMonoSingleton.Instance.OnWeaponAddOrDelete(weaponSlotList[slotIdx].WeaponIdx, false);
            //OnWeaponAddOrDelete?.Invoke(weaponSlotList[slotIdx].WeaponIdx, false);

            PlayerDataModel.Instance.AddMaterialValWith(weaponSlotList[slotIdx].CurrentPrice);
            weaponSlotList[slotIdx].InitSlot(-1,false,0);
        }
        public void CombineWeaponFromTo(int combineSlotIdx, int callSlotIdx)
        {
            // Weapon Category Bonus Should change
            CanvasMonoSingleton.Instance.OnWeaponAddOrDelete(weaponSlotList[combineSlotIdx].WeaponIdx, false);
            //OnWeaponAddOrDelete?.Invoke(weaponSlotList[combineSlotIdx].WeaponIdx, false);

            weaponSlotList[callSlotIdx].WeaponLevel++;
            weaponSlotList[callSlotIdx].InitSlot();
            weaponSlotList[combineSlotIdx].InitSlot(-1, false, 0);
        }
        public void ShowInfoMiniWindow(WeaponSlot slot)
        {
            var showCombine = false;
            var calledSlotIdx = -1;
            var combineSlotIdx = -1;
            for (int i = 0, n = weaponSlotList.Count; i < n; ++i)
            {
                if (slot == weaponSlotList[i])
                {
                    calledSlotIdx = i;
                }
                else if (slot.WeaponIdx == weaponSlotList[i].WeaponIdx && slot.WeaponLevel == weaponSlotList[i].WeaponLevel && slot.WeaponLevel != 3)
                {
                    combineSlotIdx = i;
                    showCombine = true;
                    //break;
                }
            }

            CanvasMonoSingleton.Instance.ShowAndInitInfoWindowWithWeapon(slot.WeaponIdx, slot.WeaponLevel, slot.CurrentPrice,showCombine, combineSlotIdx, calledSlotIdx, slot.gameObject.transform.position);
        }

        private void UpdateAllShopItemBuyState()
        {
            for (int i = 0, n = shopItemList.Count; i < n; ++i)
            {
                shopItemList[i].UpdateBuyButtonState(PlayerDataModel.Instance.playerMaterialCount);
            }
        }
        public bool CheckWeaponSlotTryBuyShopItem(int weaponIdx, bool isMeleeWp, int weaponLevel, int price)
        {
            WeaponSlot tmp = null;
            for (int i = 0, n = weaponSlotList.Count; i < n; ++i)
            {
                if (weaponSlotList[i].WeaponIdx == -1)
                {
                    weaponSlotList[i].CurrentPrice = price;
                    weaponSlotList[i].InitSlot(weaponIdx,isMeleeWp, weaponLevel);
                    PlayerDataModel.Instance.AddMaterialValWith(-price);
                    UpdateAllShopItemBuyState();
                    // Weapon Category Bonus Should change
                    //OnWeaponAddOrDelete?.Invoke(weaponIdx, true);
                    CanvasMonoSingleton.Instance.OnWeaponAddOrDelete(weaponIdx, true);
                    return true;
                }
                else if (weaponSlotList[i].WeaponIdx == weaponIdx && weaponSlotList[i].WeaponLevel == weaponLevel)
                {
                    tmp = weaponSlotList[i];
                }
            }
            if (tmp == null)
            {
                return false;
            }
            else
            {
                tmp.WeaponLevel++;
                tmp.InitSlot();
                PlayerDataModel.Instance.AddMaterialValWith(-price);
                UpdateAllShopItemBuyState();
                //UpdateMaterialCount();
                return true;
            }
        }

        private void UpdateMaterialCount(int count)
        {
            shopMaterialCountText.text = count.ToString();
            UpdateAllShopItemBuyState();
        }

        private void SwapShopItem(int idx1, int idx2)
        {
            tmpShopItem = shopItemList[idx1];
            shopItemList[idx1] = shopItemList[idx2];
            shopItemList[idx2] = tmpShopItem;
            tmpShopItemPosition = shopItemList[idx1].transform.localPosition;
            shopItemList[idx1].transform.localPosition = shopItemList[idx2].transform.localPosition;
            shopItemList[idx2].transform.localPosition = tmpShopItemPosition;
        }
        private void ResetReroll()
        {
            rerollTimes = 0;
            UpdateRerollButton();
            Reroll();
        }
        private void UpdateRerollButton()
        {
            rerollPrice = CanvasMonoSingleton.Instance.GetRerollPrice(rerollTimes);
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            strBuilder.Append(rerollPrice);
            RerollPriceText.text = RerollString + strBuilder.ToString();
            strBuilder.Clear();
        }
        private void RerollButtonClicked()
        {
            PlayerDataModel.Instance.AddMaterialValWith(rerollPrice);
            ++rerollTimes;
            UpdateRerollButton();
            Reroll();
        }
        private void Reroll()
        {
            for (int i = 0, n = shopItemList.Count, notlocked = -1; i < n; ++i)
            {
                if (!shopItemList[i].isLock)
                {
                    if (notlocked == -1) notlocked = i;
                    shopItemList[i].Reroll(PlayerDataModel.Instance.playerMaterialCount);
                }
                else
                {
                    if (notlocked != -1)
                    {
                        SwapShopItem(i, notlocked);
                        notlocked = i;
                    }
                }
            }
        }
        public void UpdateMainWeaponInfo()
        {
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            if (weaponSlotList[0].WeaponIdx == -1)
            {
                return;
            }
            var config = SOConfigSingleton.Instance.WeaponMapCom.wpNativeHashMap[weaponSlotList[0].WeaponIdx];
            var managedConfig = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponNameMap[weaponSlotList[0].WeaponIdx];
            strBuilder.Append(managedConfig);
            strBuilder.AppendLine();
            strBuilder.Append(config.BasicDamage);
            strBuilder.AppendLine();
            strBuilder.Append(config.DamageBonus.x);
            strBuilder.AppendLine();
            strBuilder.Append(config.DamageBonus.y);
            strBuilder.AppendLine();
            strBuilder.Append(config.DamageBonus.z);
            strBuilder.AppendLine();
            strBuilder.Append(config.DamageBonus.w);
            strBuilder.AppendLine();
            strBuilder.Append(config.WeaponCriticalHitChance);
            strBuilder.AppendLine();
            strBuilder.Append(config.WeaponCriticalHitRatio);
            strBuilder.AppendLine();
            strBuilder.Append(config.Range);
            strBuilder.AppendLine();
            strBuilder.Append(config.Cooldown);
            strBuilder.AppendLine();
            MainWeaponInfoText.text = strBuilder.ToString();
            strBuilder.Clear();
        }
        public void UpdateShopPlayerAttribute()
        {
            Debug.Log("ShopUIManager - UpdateShopPlayerAttribute");
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for(int i = 0; i < 13; ++i)
            {
                var val = PlayerDataModel.Instance.attributeValueList[i];
                if (val < 0)
                {
                    strBuilder.Append("<color=\"red\">");
                    strBuilder.Append(val);
                    strBuilder.Append("</color>");
                }
                else if (val > 0)
                {
                    strBuilder.Append("<color=\"green\">");
                    strBuilder.Append(val);
                    strBuilder.Append("</color>");
                }
                else
                {
                    strBuilder.Append(val);
                }
                strBuilder.AppendLine();
            }
            playerAttributeText.text = strBuilder.ToString();
            strBuilder.Clear();

        }


    }
}