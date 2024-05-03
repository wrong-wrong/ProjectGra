using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class InGameUIManager : MonoBehaviour
    {
        [SerializeField] Image healthBar;
        [SerializeField] Image experienceBar;
        [SerializeField] Image weaponCooldownFillingImg;
        [SerializeField] RectTransform ingameUIBackground;
        [SerializeField] TextMeshProUGUI inGameMaterialCountText;
        [SerializeField] TextMeshProUGUI healthBarText;
        [SerializeField] TextMeshProUGUI expBarText;
        [SerializeField] TextMeshProUGUI countDownText;
        [SerializeField] TextMeshProUGUI inGameWaveText;

        [SerializeField] RectTransform crateContainer;
        [SerializeField] RectTransform upgradeContainer;
        [SerializeField] GameObject legendaryCrateIconPrefab;
        [SerializeField] GameObject normalCrateIconPrefab;
        [SerializeField] GameObject upgradeIconPrefab;
        private List<GameObject> iconList;
        private int ingameUIMaxHp;
        private int ingameUIMaxExp;
        private int currentPlayerLevel;
        private int lastTotalExp;
        private int currentExp;
        private float countdownTimer;
        private int lastCountdown;
        private bool updateCountdown;
        private void Awake()
        {
            iconList = new List<GameObject>(5);
        }
        private void Update()
        {
            if (updateCountdown)
            {
                var deltatime = Time.deltaTime;
                if ((countdownTimer -= deltatime) < lastCountdown)
                {
                    lastCountdown = (int)countdownTimer;
                    var stringBuilder = CanvasMonoSingleton.Instance.stringBuilder;
                    stringBuilder.Append(lastCountdown);
                    countDownText.text = stringBuilder.ToString();
                    stringBuilder.Clear();
                }
            }
        }
        public void DestroyAllIcon()
        {
            for(int i = iconList.Count - 1; i >= 0; i--)
            {
                Destroy(iconList[i]);
                iconList.RemoveAt(i);
            }
        }
        private void AddLegendaryCrateIcon()
        {
            var icongo = Instantiate(legendaryCrateIconPrefab, crateContainer);
            iconList.Add(icongo);
        }
        private void AddNormalCrateIcon()
        {
            var icongo = Instantiate(normalCrateIconPrefab, crateContainer);
            iconList.Add(icongo);
        }
        private void AddUpgradeIcon()
        {
            var icongo = Instantiate(upgradeIconPrefab, upgradeContainer);
            iconList.Add(icongo);
        }
        public void UpdateWaveText(string text)
        {
            inGameWaveText.text = text;
        }
        public void StartCountdownTimer(float totalTimer)
        {
            var stringBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            countdownTimer = totalTimer;
            lastCountdown = (int)totalTimer;
            stringBuilder.Append(lastCountdown);
            countDownText.text = stringBuilder.ToString();
            stringBuilder.Clear();
            updateCountdown = true;
        }
        public void StartCountdown()
        {
            updateCountdown = true;
        }
        public void StopCountdown()
        {
            updateCountdown = false;
        }
        public void InitIngameUIWeaponCooldown()
        {
            weaponCooldownFillingImg.fillAmount = 1f;
        }
        public void ShowIngameUIBackground()
        {
            ingameUIBackground.localScale = Vector3.one;
        }
        public void HideIngameUIBackground()
        {
            ingameUIBackground.localScale = Vector3.zero;
        }
        //public void InGameUIUpdateCountdown(int countdown)
        //{

        //}
        public void IngameUIWeaponCooldownFilling(float fillAmount)
        {
            weaponCooldownFillingImg.fillAmount = fillAmount;
        }
        public void IngameUISetMaterial(int materialCount)
        {
            var stringBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            stringBuilder.Append(materialCount);
            inGameMaterialCountText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }
        public void IngameUIInit(int maxHp, int maxExp)
        {
            this.ingameUIMaxHp = maxHp;
            this.ingameUIMaxExp = maxExp;
            IngameUIUpdatePlayerHp(maxHp);
            IngameUIUpdatePlayerExp(0);
            IngameUIUpdatePlayerMaterial(0);
        }
        public void IngameUIUpdatePlayerHp(int hp)
        {
            var stringBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            healthBar.fillAmount = (float)hp / ingameUIMaxHp;
            stringBuilder.Append(hp);
            stringBuilder.Append(" / ");
            stringBuilder.Append(ingameUIMaxHp);
            healthBarText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }
        public int IngameUIUpdatePlayerExp(int currentTotalExp)
        {
            int ans = 0;
            currentExp += currentTotalExp - lastTotalExp;
            lastTotalExp = currentTotalExp;
            if (currentExp > ingameUIMaxExp)
            {
                currentExp -= ingameUIMaxExp;
                ++currentPlayerLevel;
                ans = 1;
                ingameUIMaxExp = (currentPlayerLevel + 3) * (currentPlayerLevel + 3);
                expBarText.text = "LV." + currentPlayerLevel;
                AddUpgradeIcon();
            }
            experienceBar.fillAmount = (float)currentExp / ingameUIMaxExp;
            return ans;
        }
        public void IngameUIUpdatePlayerMaterial(int materialsCount)
        {
            var stringBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            stringBuilder.Append(materialsCount);
            inGameMaterialCountText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }
        public void IngameUIAddNormalCrateIcon(int normal)
        {
            AddNormalCrateIcon();
        }
        public void IngameUIAddLegendaryCrateIcon(int legendary)
        {
            AddLegendaryCrateIcon();
        }
        public void IngameUIAddUpgradeIcon(int legendary)
        {
            AddUpgradeIcon();
        }
    }
}