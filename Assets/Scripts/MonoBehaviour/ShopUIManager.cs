using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class ShopUIManager : MonoBehaviour
    {
        [SerializeField] GameObject gameItemPrefab;
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
        [SerializeField] List<TextMeshProUGUI> playerAttributeTextList;

        private Vector3 tmpShopItemPosition; // for swap;
        private ShopItem tmpShopItem;
        private void Start()
        {
            weaponSlotList[0].isMainSlot = true;
            ShopContinueButton.onClick.AddListener(CanvasMonoSingleton.Instance.ShopContinueButtonActionWrapper);
            ShopRerollButton.onClick.AddListener(Reroll);
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
            Reroll();
            UpdateAllShopItemBuyState();
        }
        public void SetSlotWeaponIdx(int idx, int idx1, int idx2, int idx3)
        {
            weaponSlotList[0].InitSlot(idx, 0);
            weaponSlotList[1].InitSlot(idx1, 1);
            weaponSlotList[2].InitSlot(idx2, 2);
            weaponSlotList[3].InitSlot(idx3, 3);
        }
        public void GetSlowWeaponIdx(out int idx, out int idx1, out int idx2, out int idx3)
        {
            idx = weaponSlotList[0].WeaponIdx;
            idx1 = weaponSlotList[1].WeaponIdx;
            idx2 = weaponSlotList[2].WeaponIdx;
            idx3 = weaponSlotList[3].WeaponIdx;
        }

        public void AddGameItem(int itemIdx, int itemLevel,int currentPrice)
        {
            var item = GameObject.Instantiate(gameItemPrefab, content);
            item.gameObject.transform.SetAsFirstSibling();
            item.GetComponent<SingleGameItem>().InitWithItemIdxAndLevel(itemIdx, itemLevel,currentPrice);
            //content.SetAsFirstSibling(content)
            //setParent and then SetAsFirstSibling
        }



        public void RecycleWeaponFromSlot(int slotIdx)
        {
            PlayerDataModel.Instance.AddMaterialValWith(weaponSlotList[slotIdx].CurrentPrice);
            weaponSlotList[slotIdx].InitSlot(-1, 0);
        }
        public void CombineWeaponFromTo(int combineSlotIdx, int callSlotIdx)
        {
            weaponSlotList[callSlotIdx].WeaponLevel++;
            weaponSlotList[callSlotIdx].InitSlot();
            weaponSlotList[combineSlotIdx].InitSlot(-1, 0);
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

            CanvasMonoSingleton.Instance.ShowAndInitInfoWindowWithWeapon(slot.WeaponIdx, slot.WeaponLevel, showCombine, combineSlotIdx, calledSlotIdx, slot.gameObject.transform.position);
        }

        private void UpdateAllShopItemBuyState()
        {
            for (int i = 0, n = shopItemList.Count; i < n; ++i)
            {
                shopItemList[i].UpdateBuyButtonState(PlayerDataModel.Instance.playerMaterialCount);
            }
        }
        public bool CheckWeaponSlotTryBuyShopItem(int weaponIdx, int weaponLevel, int price)
        {
            WeaponSlot tmp = null;
            for (int i = 0, n = weaponSlotList.Count; i < n; ++i)
            {
                if (weaponSlotList[i].WeaponIdx == -1)
                {
                    weaponSlotList[i].WeaponIdx = weaponIdx;
                    weaponSlotList[i].WeaponLevel = weaponLevel;
                    weaponSlotList[i].CurrentPrice = price;
                    weaponSlotList[i].InitSlot();
                    PlayerDataModel.Instance.AddMaterialValWith(-price);
                    UpdateAllShopItemBuyState();
                    //UpdateMaterialCount();
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
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for(int i = 0; i < 11; ++i)
            {
                strBuilder.Append(PlayerDataModel.Instance.attributeValueList[i]);
                playerAttributeTextList[i].text = strBuilder.ToString();
                if (PlayerDataModel.Instance.attributeValueList[i] < 0) playerAttributeTextList[i].color = Color.red;
                strBuilder.Clear();
            }
        }
    }
}