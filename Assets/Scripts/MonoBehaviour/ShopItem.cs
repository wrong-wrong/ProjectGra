using ProjectGra;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
public class ShopItem : MonoBehaviour
{
    static StringBuilder stringBuilder;
    //[SerializeField]
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI weaponNameText;
    [SerializeField] TextMeshProUGUI weaponInfoText;
    [SerializeField] Button lockButton;
    [SerializeField] Button buyButton;
    [SerializeField] Image weaponIcon;
    [SerializeField] Image backgroundImage;
    private int currentPrice;
    private int weaponIdx;
    private int weaponLevel;
    public bool isLock;
    public void Start()
    {
        lockButton.onClick.AddListener(Lock);
        stringBuilder = new StringBuilder(50);
        isLock = false;
    }
    public void OnDestroy()
    {
        lockButton.onClick.RemoveAllListeners();
    }

    private void ChangeBackgroundColor()
    {
        backgroundImage.color = WeaponSOConfigSingleton.Instance.bgColor[weaponLevel];
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
        weaponLevel = WeaponSOConfigSingleton.Instance.GetRandomLevel(2); // should pass in player's level;
        var config = WeaponSOConfigSingleton.Instance.GetRandomWeaponConfig(2); // should pass in player's level;
        weaponIdx = config.WeaponIndex;
        currentPrice = WeaponSOConfigSingleton.Instance.ManagedConfigCom.weaponBasePriceMap[weaponIdx];
        priceText.text = currentPrice.ToString();
        weaponNameText.text = WeaponSOConfigSingleton.Instance.ManagedConfigCom.weaponNameMap[weaponIdx].ToString();
        //Setting Text
        var mainAttribute = CanvasMonoSingleton.Instance.mainAttribute;
        var damageAttribute = CanvasMonoSingleton.Instance.damagedAttribute;
        var calculatedDamageAfterBonus = (int)((1 + damageAttribute.DamagePercentage)
            * (config.BasicDamage + math.csum(config.DamageBonus * damageAttribute.MeleeRangedElementAttSpd)));
        var calculatedCritHitChance = damageAttribute.CriticalHitChance + config.WeaponCriticalHitChance;
        var calculatedCooldown = config.Cooldown * math.clamp(1 - damageAttribute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
        var calculatedRange = mainAttribute.Range + config.Range;   //used to set spawnee's timer
        stringBuilder.Append(calculatedDamageAfterBonus);
        stringBuilder.Append('|');
        stringBuilder.Append(config.BasicDamage);
        stringBuilder.AppendLine();

        stringBuilder.Append(calculatedCritHitChance);
        stringBuilder.Append('|');
        stringBuilder.Append(config.WeaponCriticalHitChance);
        stringBuilder.AppendLine();

        stringBuilder.Append(config.WeaponCriticalHitRatio);
        stringBuilder.AppendLine();

        stringBuilder.Append(calculatedCooldown);
        stringBuilder.Append('|');
        stringBuilder.Append(config.Cooldown);
        stringBuilder.AppendLine();

        stringBuilder.Append(calculatedRange);
        stringBuilder.Append('|');
        stringBuilder.Append(config.Range);
        stringBuilder.AppendLine();

        stringBuilder.Append(config.DamageBonus);
        
        weaponInfoText.text = stringBuilder.ToString();
        stringBuilder.Clear();
        ChangeBackgroundColor();
        UpdateBuyButtonState(playerMaterialCount);
    }
    public void Lock()
    {
        isLock = !isLock;
    }
    public void Buy()
    {

    }

}
