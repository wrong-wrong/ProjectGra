using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGra
{
    public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static WeaponSlot CurrentHoveredSlot;

        [SerializeField] Image bgImg;
        [SerializeField] Image iconImg;

        //TODO a field to store current weapon data
        public WeaponConfigInfoCom slotWeaponConfig;
        private Transform iconTransform;
        private Transform bgTransform;  // dont starve ��������ק��ʱ���������icon��parent��һ��dragLayer����ק��֮���ٰ�parent���û�bgTransform��Ӧ����Ϊ���Ż�

        #region UnityFunction
        public void Start()
        {
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
                SwapWeaponSlot(this, CurrentHoveredSlot);
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
        //public void InitSlot(CurrentPlayerWeaponConfigInfoBuffer config)
        //{
        //    slotWeaponConfig = config;
        //    iconImg.color = slotWeaponConfig.
        //}

        public static void SwapWeaponSlot(WeaponSlot dragged, WeaponSlot slot2)
        {
            var tmp = slot2.slotWeaponConfig;
            slot2.slotWeaponConfig = dragged.slotWeaponConfig;
            dragged.slotWeaponConfig = tmp;
        }

    }

}