using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGra
{
    public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler,IPointerClickHandler
    {
        public static WeaponSlot CurrentHoveredSlot;
        public bool isMainSlot;
        [SerializeField] Image bgImg;
        [SerializeField] Image iconImg;
        public int WeaponLevel; //0 - common, 1 - uncommon, 2 - rare, 3 - legendary
        public int WeaponIdx;
        public int CurrentPrice;
        private Transform iconTransform;
        private Transform bgTransform;  // dont starve ��������ק��ʱ���������icon��parent��һ��dragLayer����ק��֮���ٰ�parent���û�bgTransform��Ӧ����Ϊ���Ż�,��ʵ����Ϊ�˷�ֹ�ڵ�
        private ShopUIManager shopUIManager;
        public void Init(ShopUIManager shopUIManager)
        {
            this.shopUIManager = shopUIManager;
        }

        #region UnityFunction
        public void Awake()
        {
            isMainSlot = false;
            iconTransform = iconImg.transform;
            bgTransform = bgImg.transform;
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            //Check weapon data , if null ,return;
            iconTransform.SetParent(CanvasMonoSingleton.Instance.DragLayer);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Check weapon data , if null ,return;
            iconTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            iconTransform.SetParent(bgTransform);
            ResetIcon();
            //Check weapon data , if null ,return;
            if (CurrentHoveredSlot != null && CurrentHoveredSlot != this)
            {
                this.SwapWeaponSlot(this, CurrentHoveredSlot);
            }

        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            CurrentHoveredSlot = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CurrentHoveredSlot = null;
        }
        #endregion
        public void ResetIcon()
        {
            iconTransform.localPosition = Vector3.zero;
        }
        public void InitSlot()
        {
            if (WeaponIdx == -1)
            {
                iconImg.color = Color.black;
                bgImg.color = SOConfigSingleton.Instance.levelBgColor[0];
            }
            else
            {
                iconImg.color = SOConfigSingleton.Instance.WeaponManagedConfigCom.weaponColorInsteadOfIconMap[WeaponIdx];
                bgImg.color = SOConfigSingleton.Instance.levelBgColor[WeaponLevel];
            }
        }
        public void InitSlot(int idx, int level)
        {
            WeaponIdx = idx;
            WeaponLevel = level;
            InitSlot();
        }
        public void SwapWeaponSlot(WeaponSlot dragged, WeaponSlot slot2)
        {
            var tmpIdx = slot2.WeaponIdx;
            var tmpLevel = slot2.WeaponLevel;
            slot2.InitSlot(dragged.WeaponIdx, dragged.WeaponLevel);
            dragged.InitSlot(tmpIdx, tmpLevel);
            if (dragged.isMainSlot || slot2.isMainSlot) { shopUIManager.UpdateMainWeaponInfo(); }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(WeaponIdx != -1) shopUIManager.ShowInfoMiniWindow(this);
        }
    }

}