using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class ChoiceInfoWindow : MonoBehaviour
    {
        static string characterTypeStr = "Character";
        static string weaponTypeStr = "Weapon";
        static string difficultyTypeStr = "Difficulty";

        [SerializeField] Image iconImg;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI typeText;
        [SerializeField] TextMeshProUGUI weaponInfoText;
        [SerializeField] TextMeshProUGUI normalInfoText;
        [SerializeField] RectTransform weaponFixInfoRect;

        public void Awake()
        {
            ChoiceSlot.ChoiceSlotChosen += onChoiceSlot_ChoiceSlotChosen;
        }
        public void Destroy()
        {
            Destroy(gameObject);
        }
        private void onChoiceSlot_ChoiceSlotChosen(object sender, ChoiceSlotInfoEventArgs e)
        {
            var slotType = e.SlotType;
            switch (slotType)
            {
                case ChoiceSlotType.Character:
                    InitWithCharacter(e.Idx);
                    break;
                case ChoiceSlotType.Weapon:
                    InitWithWeapon(e.Idx);

                    break;
                case ChoiceSlotType.Difficulty:
                    InitWithDifficulty(e.Idx);
                    break;
            }
        }
        public void RemoveListener()
        {
            ChoiceSlot.ChoiceSlotChosen -= onChoiceSlot_ChoiceSlotChosen;
        }

        private void InitWithDifficulty(int idx)
        {
            weaponFixInfoRect.localScale = Vector3.zero;
            weaponInfoText.text = null;

            normalInfoText.text = SOConfigSingleton.Instance.DifficultyDescriptionList[idx];
            iconImg.sprite = SOConfigSingleton.Instance.DifficultySpriteList[idx];
            nameText.text = SOConfigSingleton.Instance.DifficultyNameList[idx];
            typeText.text = difficultyTypeStr;
        }
        private void InitWithWeapon(int idx)
        {
            weaponFixInfoRect.localScale = Vector3.one;
            normalInfoText.text = null;

            var managedConfig = SOConfigSingleton.Instance.WeaponManagedConfigCom;
            iconImg.sprite = null;
            nameText.text = managedConfig.weaponNameMap[idx];
            typeText.text = weaponTypeStr;


            // Setting info text
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            var config = SOConfigSingleton.Instance.WeaponMapCom.wpNativeHashMap[idx];
            var calculatedDamageAfterBonus = (int)((1 + PlayerDataModel.Instance.GetDamage())
            * (config.BasicDamage[0] + math.csum(config.DamageBonus * PlayerDataModel.Instance.GetDamageBonus())));
            var calculatedCritHitChance = PlayerDataModel.Instance.GetCritHitChance() + config.WeaponCriticalHitChance[0];
            var calculatedCooldown = config.Cooldown[0] * math.clamp(1 - PlayerDataModel.Instance.GetAttackSpeed(), 0.2f, 2f);
            var calculatedRange = PlayerDataModel.Instance.GetRange() + config.Range[0];
            strBuilder.Append(calculatedDamageAfterBonus);
            strBuilder.Append('|');
            strBuilder.Append(config.BasicDamage[0]);
            strBuilder.AppendLine();
            strBuilder.Append('x');
            strBuilder.Append(config.WeaponCriticalHitRatio[0]);
            strBuilder.Append('(');
            strBuilder.Append(calculatedCritHitChance);
            strBuilder.Append("chance)");
            strBuilder.AppendLine();
            strBuilder.Append(calculatedCooldown);
            strBuilder.AppendLine();
            strBuilder.Append(calculatedRange);
            strBuilder.Append('|');
            strBuilder.Append(config.Range[0]);
            strBuilder.AppendLine();
            weaponInfoText.text = strBuilder.ToString();
            strBuilder.Clear();
        }
        private void InitWithCharacter(int idx)
        {
            weaponInfoText.text = null;
            weaponFixInfoRect.localScale = Vector3.zero;

            var characterConfig = SOConfigSingleton.Instance.CharacterPresetSOList[idx];
            iconImg.sprite = characterConfig.CharacterSprite;
            nameText.text = characterConfig.CharacterName;
            typeText.text = characterTypeStr;

            //Setting InfoText
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for (int i = 0, n = characterConfig.AffectedAttributeIdx.Count; i < n; ++i)
            {
                if (characterConfig.BonusedValueList[i] > 0)
                {
                    strBuilder.Append("<color=\"green\">");
                    strBuilder.Append("+");
                }
                else
                {
                    strBuilder.Append("<color=\"red\">");
                }
                strBuilder.Append(characterConfig.BonusedValueList[i]);
                strBuilder.Append("</color> ");
                strBuilder.Append(CanvasMonoSingleton.Instance.IdxToAttributeName[characterConfig.AffectedAttributeIdx[i]]);
                strBuilder.AppendLine();
            }
            normalInfoText.text = strBuilder.ToString();
            strBuilder.Clear();
        }

    }
}