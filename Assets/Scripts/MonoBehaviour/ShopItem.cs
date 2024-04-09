using ProjectGra;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
//using UnityEngine.UIElements;
public class ShopItem : MonoBehaviour
{
    static string lockedString = "(OvO)";
    static string unlockString = "Lock";
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI weaponInfoText;
    [SerializeField] TextMeshProUGUI itemInfoText;
    [SerializeField] TextMeshProUGUI lockButtonText;
    [SerializeField] RectTransform wpInfoTextRect;
    [SerializeField] RectTransform wpFixedTextRect;
    [SerializeField] RectTransform itemInfoTextRect;
    [SerializeField] Button lockButton;
    [SerializeField] Button buyButton;
    [SerializeField] Image icon;
    [SerializeField] Image iconBackground;
    [SerializeField] Image backgroundImage;
    private int currentPrice;
    private int weaponIdx;
    private int contentLevel;
    public bool isLock;
    private bool isMeleeWp;
    private bool isWeapon;
    private ShopUIManager shopUIManager;
    public void Init(ShopUIManager shopUIManager)
    {
        this.shopUIManager = shopUIManager;
    }
    public void Start()
    {
        lockButton.onClick.AddListener(Lock);
        buyButton.onClick.AddListener(Buy);
        isLock = false;
    }
    public void OnDestroy()
    {
        lockButton.onClick.RemoveAllListeners();
        buyButton.onClick.RemoveAllListeners();
    }

    private void ChangeBackgroundColor()
    {
        backgroundImage.color = SOConfigSingleton.Instance.levelBgColor[contentLevel];
        iconBackground.color = SOConfigSingleton.Instance.levelBgColorLight[contentLevel];
    }
    public void UpdateBuyButtonState(int material)
    {
        if(material < currentPrice)
        {
            priceText.color = Color.red;
            buyButton.interactable = false;
        }
        else
        {
            priceText.color = Color.white;
            buyButton.interactable = true;
        }
    }
    public void Reroll(int playerMaterialCount)
    {
        if (SOConfigSingleton.Instance.ShopItemRerollNextTypeIsWeapon()) 
        {
            RerollWeapon();
        }
        else
        {
            RerollItem();
        }
        UpdateBuyButtonState(playerMaterialCount);

    }

    int itemIdx;
    private void RerollItem()
    {
        isWeapon = false;
        rectTransform.localScale = Vector3.one;
        itemInfoTextRect.localScale = Vector3.one;
        wpInfoTextRect.localScale = Vector3.zero;
        wpFixedTextRect.localScale = Vector3.zero;
        itemIdx = SOConfigSingleton.Instance.GetRandomItemConfigIdx();
        currentPrice = SOConfigSingleton.Instance.GetItemCurrentPrice(itemIdx);
        var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;

        priceText.text = strBuilder.Append(currentPrice).ToString();
        strBuilder.Clear();
        //Init after set config
        var currentItem = SOConfigSingleton.Instance.ItemSOList[itemIdx];
        contentLevel = currentItem.ItemLevel;
        itemIdx = currentItem.ItemIdx;
        icon.sprite = currentItem.ItemSprite;
        nameText.text = currentItem.ItemName;
        for (int i = 0, n = currentItem.AffectedAttributeIdx.Count; i < n; ++i)
        {
            if (currentItem.BonusedValueList[i] > 0) strBuilder.Append("+");
            strBuilder.Append(currentItem.BonusedValueList[i]);
            strBuilder.Append(CanvasMonoSingleton.IdxToAttributeName[currentItem.AffectedAttributeIdx[i]]);
            strBuilder.AppendLine();
        }
        itemInfoText.text = strBuilder.ToString();
        strBuilder.Clear();
        ChangeBackgroundColor();
    }
    private void RerollWeapon()
    {
        isWeapon = true;
        rectTransform.localScale = Vector3.one;
        wpInfoTextRect.localScale = Vector3.one;
        wpFixedTextRect.localScale = Vector3.one;
        itemInfoTextRect.localScale = Vector3.zero;

        contentLevel = SOConfigSingleton.Instance.GetRandomLevel(); // should pass in player's level;
        var config = SOConfigSingleton.Instance.GetRandomWeaponConfig(); // should pass in player's level;
        icon.sprite = null;
        isMeleeWp = config.IsMeleeWeapon;
        weaponIdx = config.WeaponIndex;
        currentPrice = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponBasePriceMap[weaponIdx];
        priceText.text = currentPrice.ToString();
        nameText.text = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponNameMap[weaponIdx].ToString();
        //Setting Text
        var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
        var calculatedDamageAfterBonus = (int)((1 + PlayerDataModel.Instance.GetDamage())
            * (config.BasicDamage + math.csum(config.DamageBonus * PlayerDataModel.Instance.GetDamageBonus())));
        var calculatedCritHitChance = PlayerDataModel.Instance.GetCritHitChance() + config.WeaponCriticalHitChance;
        var calculatedCooldown = config.Cooldown * math.clamp(1 -PlayerDataModel.Instance.GetAttackSpeed(), 0.2f, 2f);
        var calculatedRange = PlayerDataModel.Instance.GetRange()+ config.Range;   //used to set spawnee's timer
        strBuilder.Append(calculatedDamageAfterBonus);
        strBuilder.Append('|');
        strBuilder.Append(config.BasicDamage);
        strBuilder.AppendLine();

        strBuilder.Append(calculatedCritHitChance);
        strBuilder.Append('|');
        strBuilder.Append(config.WeaponCriticalHitChance);
        strBuilder.AppendLine();

        strBuilder.Append(config.WeaponCriticalHitRatio);
        strBuilder.AppendLine();

        strBuilder.Append(calculatedCooldown);
        strBuilder.Append('|');
        strBuilder.Append(config.Cooldown);
        strBuilder.AppendLine();

        strBuilder.Append(calculatedRange);
        strBuilder.Append('|');
        strBuilder.Append(config.Range);
        strBuilder.AppendLine();

        strBuilder.Append(config.DamageBonus);

        weaponInfoText.text = strBuilder.ToString();
        strBuilder.Clear();
        ChangeBackgroundColor();
    }
    public void Lock()
    {
        isLock = !isLock;
        if (!isLock)
        {
            lockButtonText.text = unlockString;
            buyButton.enabled = true;
        }
        else
        {
            lockButtonText.text = lockedString;
            buyButton.enabled = false;
        }
    }
    public void Buy()
    {
        if (isWeapon)
        {
            if (shopUIManager.CheckWeaponSlotTryBuyShopItem(weaponIdx, isMeleeWp, contentLevel, currentPrice))
            {
                rectTransform.localScale = Vector3.zero;
            }
            else
            {
                Debug.Log("No empty slot!!!");
            }
        }
        else
        {
            shopUIManager.AddGameItem(itemIdx, contentLevel, currentPrice,currentPrice);
            rectTransform.localScale = Vector3.zero;
        }
    }

}
