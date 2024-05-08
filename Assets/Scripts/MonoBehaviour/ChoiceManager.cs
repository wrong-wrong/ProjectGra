using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    public class ChoiceManager : MonoBehaviour
    {
        [SerializeField] RectTransform infoContainerRect;
        [SerializeField] RectTransform slotContainerRect;
        [SerializeField] GameObject ChoiceSlotPrefab;
        [SerializeField] GameObject InfoWindowPrefab;
        [SerializeField] float infoWindowWidth;
        [SerializeField] float slotWidth;
        [SerializeField] float infoWindowSpacing;
        [SerializeField] float slotSpacing;
        private List<ChoiceSlot> slotList;
        private List<ChoiceInfoWindow> infoList;
        private float slotContainerCurrentPreferredPosX;
        private float infoContainerCurrentPreferredPosX;
        private ChoiceSlotType currentSlotType;
        private void Awake()
        {
            slotList = new List<ChoiceSlot>();
            infoList = new List<ChoiceInfoWindow>();


        }
        private void onChoiceSlotConfirm(int Idx)
        {
            switch (currentSlotType)
            {
                case ChoiceSlotType.Character:
                    currentSlotType = ChoiceSlotType.Weapon;
                    ApplyCharacterPreset(Idx);
                    InitSlotsWithCurrentSlotType();
                    break;
                case ChoiceSlotType.Weapon:
                    currentSlotType = ChoiceSlotType.Difficulty;
                    ApplyWeaponPreset(Idx);
                    InitSlotsWithCurrentSlotType();
                    break;
                case ChoiceSlotType.Difficulty:
                    ApplyDifficultyPreset(Idx);
                    FinishPresetChoosing();
                    break;
            }
        }

        private void ApplyDifficultyPreset(int Idx)
        {
            MonoGameManagerSingleton.Instance.CurrentDifficulty = Idx;
        }

        private void ApplyWeaponPreset(int Idx)
        {
            MonoGameManagerSingleton.Instance.CurrentWeaponPresetIdx = Idx;
            CanvasMonoSingleton.Instance.OnWeaponAddOrDelete(Idx, true);
        }

        private void ApplyCharacterPreset(int Idx)
        {
            var config = SOConfigSingleton.Instance.CharacterPresetSOList[Idx];
            for(int i = 0; i < config.AffectedAttributeIdx.Count; ++i)
            {
                PlayerDataModel.Instance.AddAttributeValWith(config.AffectedAttributeIdx[i], config.BonusedValueList[i]);   
            }
        }

        private void FinishPresetChoosing()
        {
            ChoiceSlot.ChoiceSlotConfirm -= onChoiceSlotConfirm;
            // destory all info window and choice slot
            for (int i = slotList.Count - 1; i >= 0; i--)
            {
                Destroy(slotList[i].gameObject);
            }
            infoList[infoList.Count - 1].RemoveListener();
            for (int i = infoList.Count - 1; i >= 0; i--)
            {
                Destroy(infoList[i].gameObject);
            }
            slotList.Clear();
            infoList.Clear();
            MonoGameManagerSingleton.Instance.IsPresetChoosingDone = true;
        }
        public void ResetState()
        {
            ChoiceSlot.ChoiceSlotConfirm += onChoiceSlotConfirm;
            currentSlotType = ChoiceSlotType.Character;
            PlayerDataModel.Instance.ResetData();
            InitSlotsWithCurrentSlotType();

        }
        private void InitSlotsWithCurrentSlotType()
        {
            switch (currentSlotType)
            {
                case ChoiceSlotType.Character:
                    InitSlotsWithCharacter();
                    break;
                case ChoiceSlotType.Weapon:
                    InitSlotsWithWeapon();
                    break;
                case ChoiceSlotType.Difficulty:
                    InitSlotsWithDifficulty();
                    break;
            }
        }
        private void InitSlotsWithCharacter()
        {
            // modify slot 
            //var currentCount = slotList.Count;
            var characterSOCount = SOConfigSingleton.Instance.CharacterPresetSOList.Count;
            for (int i = 0; i < characterSOCount; i++)
            {
                var go = Instantiate(ChoiceSlotPrefab, slotContainerRect);
                var slotCom = go.GetComponent<ChoiceSlot>();
                slotCom.InitSlot(i, currentSlotType);
                slotList.Add(slotCom);
            }
            // modify info
            AddInfoWindowAndReposition();
        }
        private void InitSlotsWithWeapon()
        {
            // modify slot 
            var currentCount = slotList.Count;
            var weaponSOCount = SOConfigSingleton.Instance.WeaponMapCom.wpNativeHashMap.Count;
            for (int i = 0; i < weaponSOCount && i < currentCount; i++)
            {
                slotList[i].InitSlot(i, currentSlotType);
            }
            if (weaponSOCount > currentCount)
            {
                for (int i = currentCount; i < weaponSOCount; i++)
                {
                    var go = Instantiate(ChoiceSlotPrefab, slotContainerRect);
                    var slotCom = go.GetComponent<ChoiceSlot>();
                    slotCom.InitSlot(i, currentSlotType);
                    slotList.Add(slotCom);
                }
            }
            if (weaponSOCount < currentCount)
            {
                for (int i = currentCount - 1; i >= weaponSOCount; --i)
                {
                    var slot = slotList[i];
                    slotList.RemoveAt(i);
                    Destroy(slot.gameObject);
                }
            }
            // modify info
            AddInfoWindowAndReposition();
        }
        private void InitSlotsWithDifficulty()
        {
            // modify slot 
            var currentCount = slotList.Count;
            var difficultyCount = SOConfigSingleton.Instance.DifficultyDescriptionList.Count;
            for (int i = 0; i < difficultyCount && i < currentCount; i++)
            {
                slotList[i].InitSlot(i, currentSlotType);
            }
            if (difficultyCount > currentCount)
            {
                for (int i = currentCount; i < difficultyCount; i++)
                {
                    var go = Instantiate(ChoiceSlotPrefab, slotContainerRect);
                    var slotCom = go.GetComponent<ChoiceSlot>();
                    slotCom.InitSlot(i, currentSlotType);
                    slotList.Add(slotCom);
                }
            }
            if (difficultyCount < currentCount)
            {
                for (int i = currentCount - 1; i >= difficultyCount; --i)
                {
                    var slot = slotList[i];
                    slotList.RemoveAt(i);
                    Destroy(slot.gameObject);
                }
            }
            // modify info
            AddInfoWindowAndReposition();
        }
        private void AddInfoWindowAndReposition()
        {
            // remove previous info window's listener
            if(infoList.Count > 0)infoList[infoList.Count - 1].RemoveListener();

            // add info window
            var infoWindow = Instantiate(InfoWindowPrefab, infoContainerRect);
            infoList.Add(infoWindow.GetComponent<ChoiceInfoWindow>());
            // trigger first slot's event
            slotList[0].TriggerEvent();
            // reposition info container and slot container

            infoContainerCurrentPreferredPosX = -((infoList.Count - 1) * (infoWindowWidth + infoWindowSpacing) + infoWindowWidth)/2;
            slotContainerCurrentPreferredPosX = -((slotList.Count - 1) * (slotWidth+ slotSpacing) + slotWidth)/2;

            //infoContainerRect.RePosition(infoContainerCurrentPreferredPosX);
            //slotContainerRect.RePosition(slotContainerCurrentPreferredPosX);
            infoContainerRect.localPosition = new Vector3(infoContainerCurrentPreferredPosX, infoContainerRect.localPosition.y);
            slotContainerRect.localPosition = new Vector3(slotContainerCurrentPreferredPosX, slotContainerRect.localPosition.y);

        }


    }
}