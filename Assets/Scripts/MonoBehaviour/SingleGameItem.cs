using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGra
{
    public class SingleGameItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image background;
        [SerializeField] Image iconImg;
        private int itemIdx;
        private int itemLevel;
        private int currentPrice;
        public void InitWithItemIdxAndLevel(int itemIdx, int itemLevel, int currentPrice)
        {
            this.itemIdx = itemIdx;
            this.itemLevel = itemLevel;
            this.currentPrice = currentPrice;
            InitVisual(itemLevel);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            CanvasMonoSingleton.Instance.ShowAndInitInfoWindowWithItem(itemIdx, itemLevel, currentPrice, this.gameObject, this.gameObject.transform.position);
        }
        public void InitVisual(int level)
        {
            background.color = SOConfigSingleton.Instance.levelBgColor[level];
            iconImg.sprite = SOConfigSingleton.Instance.ItemSOList[itemIdx].ItemSprite;
        }
    }
}