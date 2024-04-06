using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class InfoMimiWindow : MonoBehaviour
    {
        public Image Icon;
        public Image backgroundImage;
        public List<Image> buttonBgList;
        public Button CombineButton;
        public Button RecycleButton;
        public Button CancelButton;
        public TextMeshProUGUI RecycleText;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI InfoText;

        public RectTransform CombineButtonRect;
        public RectTransform WindowRect;

        private int combineSlotIdx;
        private int calledSlotIdx;
        public void Awake()
        {
            CancelButton.onClick.AddListener(() => { WindowRect.localScale = Vector3.zero; });
            CombineButton.onClick.AddListener(OnCombineButtonClicked);
            RecycleButton.onClick.AddListener(OnRecycleButtonClicked);
        }

        private void OnRecycleButtonClicked()
        {
            CanvasMonoSingleton.Instance.RecycleWeaponFromSlot(calledSlotIdx);
            WindowRect.localScale = Vector3.zero;
        }
        private void OnCombineButtonClicked()
        {
            CanvasMonoSingleton.Instance.CombineWeaponFromTo(combineSlotIdx, calledSlotIdx);
            WindowRect.localScale = Vector3.zero;
        }
        public void OnDestroy()
        {
            CancelButton.onClick.RemoveAllListeners();
            CombineButton.onClick.RemoveAllListeners();
            RecycleButton.onClick.RemoveAllListeners();
        }

        public void InitInfoMimiWindowAndShotAtPosition(PlayerAtttributeDamageRelated damageAttribute, float range, int weaponIdx, int weaponLevel,bool isCanCombine, int combineSlotIdx, int calledSlotIdx,Vector3 showPos)
        {
            this.calledSlotIdx = calledSlotIdx;
            this.combineSlotIdx = combineSlotIdx;
            WindowRect.localScale = Vector3.one;
            WindowRect.position = showPos;
            if(isCanCombine)
            {
                CombineButtonRect.localScale = Vector3.one;
                WindowRect.rect.Set(0, 0, 350, 450);
            }
            else
            {
                CombineButtonRect.localScale = Vector3.zero;
                WindowRect.rect.Set(0, 0, 350, 400);
            }
            var config = WeaponSOConfigSingleton.Instance.MapCom.wpNativeHashMap[weaponIdx];
            var monoOnlyConfig = WeaponSOConfigSingleton.Instance.ManagedConfigCom;
            var color = WeaponSOConfigSingleton.Instance.levelBgColorLight[weaponLevel];
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for (int i = 0, n = buttonBgList.Count; i < n; i++)
            {
                buttonBgList[i].color = color;
            }
            backgroundImage.color = WeaponSOConfigSingleton.Instance.levelBgColor[weaponLevel];
            NameText.text = monoOnlyConfig.weaponNameMap[weaponIdx];
            strBuilder.Append("Recycle (");
            strBuilder.Append(monoOnlyConfig.weaponBasePriceMap[weaponIdx]);
            strBuilder.Append(")");
            RecycleText.text = strBuilder.ToString();
            strBuilder.Clear();
            Icon.color = monoOnlyConfig.weaponColorInsteadOfIconMap[weaponIdx];
            var calculatedDamageAfterBonus = (int)((1 + damageAttribute.DamagePercentage)
            * (config.BasicDamage + math.csum(config.DamageBonus * damageAttribute.MeleeRangedElementAttSpd)));
            var calculatedCritHitChance = damageAttribute.CriticalHitChance + config.WeaponCriticalHitChance;
            var calculatedCooldown = config.Cooldown * math.clamp(1 - damageAttribute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
            var calculatedRange = range + config.Range;   //used to set spawnee's timer
            strBuilder.Append(calculatedDamageAfterBonus);
            strBuilder.Append('|');
            strBuilder.Append(config.BasicDamage);
            strBuilder.AppendLine();
            strBuilder.Append('x');
            strBuilder.Append(config.WeaponCriticalHitRatio);
            strBuilder.Append('(');
            strBuilder.Append(calculatedCritHitChance);
            strBuilder.Append("chance)");
            strBuilder.AppendLine();

            strBuilder.Append(calculatedCooldown);
            strBuilder.AppendLine();

            strBuilder.Append(calculatedRange);
            strBuilder.Append('|');
            strBuilder.Append(config.Range);
            strBuilder.AppendLine();
            InfoText.text = strBuilder.ToString();
            strBuilder.Clear();
        }
    }
}