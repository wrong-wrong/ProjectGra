using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectGra
{
    public class AttributeInfoVisual : MonoBehaviour
    {
        //[SerializeField] 
        [SerializeField] List<TextMeshProUGUI> playerAttributeTextList;
        public void OnEnable()
        {
            //Debug.Log("AttributeInfoVisual - OnEnable");
            PlayerDataModel.Instance.OnPlayerAttributeChanged += UpdateSinglePlayerAttribute;
        }
        public void OnDisable()
        {
            PlayerDataModel.Instance.OnPlayerAttributeChanged -= UpdateSinglePlayerAttribute;
        }
        public void UpdateSinglePlayerAttribute()
        {
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for (int i = 0; i < 11; ++i)
            {
                strBuilder.Append(PlayerDataModel.Instance.attributeValueList[i]);
                playerAttributeTextList[i].text = strBuilder.ToString();
                if (PlayerDataModel.Instance.attributeValueList[i] < 0) playerAttributeTextList[i].color = Color.red;
                else playerAttributeTextList[i].color = Color.green;
                strBuilder.Clear();
            }
        }
    }
}