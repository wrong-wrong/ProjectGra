using ProjectGra;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
public class ShopItem : MonoBehaviour
{
    string lockedString = "(OvO)";
    string unlockString = "Lock";
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI weaponNameText;
    [SerializeField] TextMeshProUGUI weaponInfoText;
    [SerializeField] TextMeshProUGUI lockButtonText;
    [SerializeField] Button lockButton;
    [SerializeField] Button buyButton;
    [SerializeField] Image weaponIcon;
    [SerializeField] Image backgroundImage;
    private int currentPrice;
    private int weaponIdx;
    private int weaponLevel;
    public bool isLock;
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
        backgroundImage.color = SOConfigSingleton.Instance.levelBgColor[weaponLevel];
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
        rectTransform.localScale = Vector3.one;
        weaponLevel = SOConfigSingleton.Instance.GetRandomLevel(); // should pass in player's level;
        var config = SOConfigSingleton.Instance.GetRandomWeaponConfig(); // should pass in player's level;
        weaponIdx = config.WeaponIndex;
        currentPrice = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponBasePriceMap[weaponIdx];
        priceText.text = currentPrice.ToString();
        weaponNameText.text = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponNameMap[weaponIdx].ToString();
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
        UpdateBuyButtonState(playerMaterialCount);
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
        if (shopUIManager.CheckWeaponSlotTryBuyShopItem(weaponIdx, weaponLevel, currentPrice))
        {
            rectTransform.localScale = Vector3.zero;
        }
    }

}
