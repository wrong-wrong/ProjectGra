using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class NoticeSingleUnit : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI waveNumberText;
        [SerializeField] RectTransform noticeRect;
        [SerializeField] Image iconImg;
        private int currentCodingWave;
        public void Hide()
        {
            noticeRect.localScale = Vector3.zero;
        }
        public void Show(Sprite iconSprite, int codingWave)
        {
            noticeRect.localScale = Vector3.one;

            if (currentCodingWave == codingWave) return;

            iconImg.sprite = iconSprite;
            waveNumberText.text = (codingWave + 1).ToString();
        }
    }
}