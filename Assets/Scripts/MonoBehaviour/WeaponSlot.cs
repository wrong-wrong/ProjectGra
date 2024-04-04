using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGra
{
    public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static WeaponSlot CurrentHoveredSlot;
        public bool isMainSlot;
        [SerializeField] Image bgImg;
        [SerializeField] Image iconImg;

        //TODO a field to store current weapon data
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
        public void InitSlot(int idx)
        {
            WeaponIdx = idx;
            if (WeaponIdx == -1) 
            { 
                iconImg.color = Color.black;
            }
            else
            {
                var config = WeaponSOConfigSingleton.Instance.MapCom.wpNativeHashMap[idx];
                iconImg.color = new Color(config.color.x, config.color.y, config.color.z);
            }
        }
        public static void SwapWeaponSlot(WeaponSlot dragged, WeaponSlot slot2)
        {
            var tmp = slot2.WeaponIdx;
            slot2.InitSlot(dragged.WeaponIdx);
            dragged.InitSlot(tmp);
            if(dragged.isMainSlot || slot2.isMainSlot) { CanvasMonoSingleton.Instance.UpdateMainWeaponInfo(); }
        }
        
    }

}