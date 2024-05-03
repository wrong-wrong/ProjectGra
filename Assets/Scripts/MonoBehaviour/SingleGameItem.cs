using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGra
{
    public class SingleGameItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image background;
        [SerializeField] Image iconImg;
        [SerializeField] TextMeshProUGUI countText;
        private int itemIdx;
        private int itemLevel;
        private int currentPrice;
        private int basePrice;
        private bool isAtShopUI;
        private int count;
        public void Awake()
        {
            CanvasMonoSingleton.Instance.OnWeaponCanvasGroupShowIsCurrentShopUI += SetIsAtShopUIFlag;
            CanvasMonoSingleton.Instance.OnNewWaveBegin += OnNewWaveBegin;
            isAtShopUI = true;
        }
        public void OnDestroy()
        {
            CanvasMonoSingleton.Instance.OnWeaponCanvasGroupShowIsCurrentShopUI -= SetIsAtShopUIFlag;
            CanvasMonoSingleton.Instance.OnNewWaveBegin -= OnNewWaveBegin;
        }
        public bool Recycle()
        {
            if(--count <= 0)
            {
                Destroy(gameObject);
                return true;
            }
            else if(count > 1) 
            {
                var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
                strBuilder.Append('X');
                countText.text = strBuilder.Append(count).ToString();
                strBuilder.Clear();
            }
            else
            {
                countText.text = null;
            }
            return false;
        }
        public void AddCount()
        {
            ++count;
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            countText.text = strBuilder.Append(count).ToString();
            strBuilder.Clear();
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
            count = 1;
            InitVisual(itemLevel);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if(isAtShopUI)CanvasMonoSingleton.Instance.ShowAndInitInfoWindowWithItem(itemIdx, itemLevel, currentPrice, this, this.gameObject.transform.position);
            else CanvasMonoSingleton.Instance.ShowAndInitInfoWindowWithItem(itemIdx, itemLevel, this, this.gameObject.transform.position);
        }
        public void InitVisual(int level)
        {
            background.color = SOConfigSingleton.Instance.levelBgColor[level];
            iconImg.sprite = SOConfigSingleton.Instance.ItemSOList[itemIdx].ItemSprite;
            countText.text = null;
        }
    }
}