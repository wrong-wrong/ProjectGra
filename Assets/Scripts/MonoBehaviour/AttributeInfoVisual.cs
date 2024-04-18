using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectGra
{
    public class AttributeInfoVisual : MonoBehaviour
    {
        //[SerializeField] 
        [SerializeField] TextMeshProUGUI playerAttributeText;
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
            for (int i = 0; i < 13; ++i)
            {
                var val = PlayerDataModel.Instance.attributeValueList[i];
                if (val < 0)
                {
                    strBuilder.Append("<color=\"red\">");
                    strBuilder.Append(val);
                    strBuilder.Append("</color>");
                }
                else if (val > 0)
                {
                    strBuilder.Append("<color=\"green\">");
                    strBuilder.Append(val);
                    strBuilder.Append("</color>");
                }
                else
                {
                    strBuilder.Append(val);
                }
                strBuilder.AppendLine();
            }
            playerAttributeText.text = strBuilder.ToString();
            strBuilder.Clear();
        }
    }
}