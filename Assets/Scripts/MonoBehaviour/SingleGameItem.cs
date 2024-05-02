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
        private int basePrice;
        private bool isAtShopUI;
        public void Awake()
        {
            CanvasMonoSingleton.Instance.OnWeaponCanvasGroupShowIsCurrentShopUI += SetIsAtShopUIFlag;
            CanvasMonoSingleton.Instance.OnNewWaveBegin += OnNewWaveBegin;
        }
        public void OnDestroy()
        {
            CanvasMonoSingleton.Instance.OnWeaponCanvasGroupShowIsCurrentShopUI -= SetIsAtShopUIFlag;
            CanvasMonoSingleton.Instance.OnNewWaveBegin -= OnNewWaveBegin;
        }
        private void OnNewWaveBegin(int codingWave)
        {
            currentPrice = CanvasMonoSingleton.Instance.CalculateFinalPrice(basePrice);
        }
        public void SetIsAtShopUIFlag(bool IsAtShopUI)
        {
            isAtShopUI = IsAtShopUI;
        }
        public void InitWithItemIdxAndLevel(int itemIdx, int itemLevel, int basePrice)
        {
            this.itemIdx = itemIdx;
            this.itemLevel = itemLevel;
            this.basePrice = basePrice;
            currentPrice = CanvasMonoSingleton.Instance.CalculateFinalPrice(basePrice);
            InitVisual(itemLevel);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if(isAtShopUI)CanvasMonoSingleton.Instance.ShowAndInitInfoWindowWithItem(itemIdx, itemLevel, currentPrice, this.gameObject, this.gameObject.transform.position);
            else CanvasMonoSingleton.Instance.ShowAndInitInfoWindowWithItem(itemIdx, itemLevel, this.gameObject, this.gameObject.transform.position);
        }
        public void InitVisual(int level)
        {
            background.color = SOConfigSingleton.Instance.levelBgColor[level];
            iconImg.sprite = SOConfigSingleton.Instance.ItemSOList[itemIdx].ItemSprite;
        }
    }
}