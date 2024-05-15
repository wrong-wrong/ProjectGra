using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGra
{
    public class ChoiceSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
    {
        public static event EventHandler<ChoiceSlotInfoEventArgs> ChoiceSlotChosen;
        public static Action<int> ChoiceSlotConfirm;
        public static ChoiceSlot CurrentSlot;

        [SerializeField] Image iconImg;
        private int _idx;
        private ChoiceSlotType _type;

        public void Destroy()
        {
            Destroy(gameObject);
        }
        public void InitSlot(int Idx, ChoiceSlotType type)
        {
            _idx = Idx;
            _type = type;
            switch (type)
            {
                case ChoiceSlotType.Character:
                    iconImg.sprite = SOConfigSingleton.Instance.CharacterPresetSOList[Idx].CharacterSprite;
                    break;
                case ChoiceSlotType.Weapon:
                    iconImg.sprite = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponIconSpriteMap[Idx];
                    //iconImg.color = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponColorInsteadOfIconMap[Idx];
                    break;
                case ChoiceSlotType.Difficulty:
                    iconImg.sprite = SOConfigSingleton.Instance.DifficultySpriteList[Idx];
                    break;
            }
        }
        
        public void TriggerEvent()
        {
            ChoiceSlotChosen?.Invoke(this, new ChoiceSlotInfoEventArgs
            {
                Idx = _idx,
                SlotType = _type
            });
        }
        #region Unity function
        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerEvent(); // just in case...
            ChoiceSlotConfirm?.Invoke(_idx);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            CurrentSlot = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CurrentSlot = this;
            TriggerEvent();
        }
        #endregion

    }

    public class ChoiceSlotInfoEventArgs : EventArgs
    {
        public ChoiceSlotType SlotType;
        public int Idx;
    }
    public enum ChoiceSlotType
    {
        Character,
        Weapon,
        Difficulty
    }
}