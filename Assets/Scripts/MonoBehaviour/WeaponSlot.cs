using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGra
{
    public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static WeaponSlot CurrentHoveredSlot;
        public static List<Color> bgColorList;
        public bool isMainSlot;
        [SerializeField] Image bgImg;
        [SerializeField] Image iconImg;
        public int WeaponLevel; //0 - common, 1 - uncommon, 2 - rare, 3 - legendary
        public int WeaponIdx;
        private Transform iconTransform;
        private Transform bgTransform;  // dont starve 里面在拖拽的时候会先设置icon的parent到一个dragLayer，拖拽玩之后再把parent设置回bgTransform，应该是为了优化

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
        public void InitSlot(int idx,int level)
        {
            WeaponIdx = idx;
            WeaponLevel = level;
            if (WeaponIdx == -1) 
            { 
                iconImg.color = Color.black;
                bgImg.color = WeaponSOConfigSingleton.Instance.bgColor[0];
            }
            else
            {
                var config = WeaponSOConfigSingleton.Instance.MapCom.wpNativeHashMap[idx];
                iconImg.color = new Color(config.color.x, config.color.y, config.color.z);
                bgImg.color = WeaponSOConfigSingleton.Instance.bgColor[level];
            }
        }
        public static void SwapWeaponSlot(WeaponSlot dragged, WeaponSlot slot2)
        {
            var tmpIdx = slot2.WeaponIdx;
            var tmpLevel = slot2.WeaponLevel;
            slot2.InitSlot(dragged.WeaponIdx,dragged.WeaponLevel);
            dragged.InitSlot(tmpIdx,tmpLevel);
            if(dragged.isMainSlot || slot2.isMainSlot) { CanvasMonoSingleton.Instance.UpdateMainWeaponInfo(); }
        }
        
    }

}