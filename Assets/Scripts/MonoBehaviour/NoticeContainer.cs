using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class NoticeContainer : MonoBehaviour
    {
        [SerializeField] Sprite hordeSprite;
        [SerializeField] Sprite eliteSprite;
        [SerializeField] TextMeshProUGUI warningText;
        [SerializeField] List<SingleNotice> singleNoticeList;
        List<int> noticeCodingWaveList;
        List<bool> noticeIsEliteList;
        [SerializeField] Button TestButton;
        [SerializeField] int testInt;
        int warningWave;
        //List<KeyValuePair<int,bool>> noticeList;
        private void Awake()
        {
            noticeCodingWaveList = new List<int>(5);
            noticeIsEliteList = new List<bool>(5);
            //noticeList = new List<KeyValuePair<int,bool>>(5);
            TestButton.onClick.AddListener(() => { UpdateNotice(testInt); }) ;
            CanvasMonoSingleton.Instance.OnNewWaveBegin += UpdateNotice;
        }
        private void OnDestroy()
        {
            CanvasMonoSingleton.Instance.OnNewWaveBegin -= UpdateNotice;
        }
        public void AddCodingWaveAndIsElite(int codingWave, bool isElite)
        {
            noticeCodingWaveList.Add(codingWave);
            noticeIsEliteList.Add(isElite);
            //noticeList.Add(new KeyValuePair<int, bool>(codingWave, isElite));
        }
        public void InitAfterInitialize()
        {
            UpdateNotice(0);
        }

        public void Restart()
        {
            noticeCodingWaveList.Clear();
            noticeIsEliteList.Clear();
        }

        public void UpdateNotice(int codingWave)
        {
            ++codingWave;
            if(noticeCodingWaveList.Count == 0)
            {
                Debug.Log("NoticeList.Count Equals to ZERO");
                HideAll();
                return;
            }
            // update the array with codingWave
            if (noticeCodingWaveList[0] < codingWave)
            {
                noticeIsEliteList.RemoveAt(0);
                noticeCodingWaveList.RemoveAt(0);
            }
            // notice less than 3 , then hide
            var cnt = noticeCodingWaveList.Count;
            if (cnt < 3)
            {
                for(int i = 0; i < 3 - cnt; i++)
                {
                    singleNoticeList[i].Hide();
                }
                for(int i = 3 - cnt, j = 0; j < cnt; ++i,++j)
                {
                    if (noticeIsEliteList[j])
                    {
                        singleNoticeList[i].Show(eliteSprite, noticeCodingWaveList[j]);
                    }
                    else
                    {
                        singleNoticeList[i].Show(hordeSprite, noticeCodingWaveList[j]);
                    }
                }
            }
            else // notice is equal to or more than 3, then show the first three
            {
                for(int i = 0; i < 3; ++i)
                {
                    if (noticeIsEliteList[i])
                    {
                        singleNoticeList[i].Show(eliteSprite, noticeCodingWaveList[i]);
                    }
                    else
                    {
                        singleNoticeList[i].Show(hordeSprite, noticeCodingWaveList[i]);
                    }
                }
            }
            UpdateWarningText();

        }

        private void HideAll()
        {
            for (int i = 0; i < 3; i++)
            {
                singleNoticeList[i].Hide();
            }
            warningText.text = null;
        }
        private void UpdateWarningText()
        {
            if (noticeCodingWaveList.Count == 0)
            {
                warningText.text = null;
                return;
            }
            //if(warningWave == noticeCodingWaveList[0])
            //{
            //    return;
            //}
            //warningWave = noticeCodingWaveList[0];
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            if (noticeIsEliteList[0])
            {
                strBuilder.Append("An elite will appear at wave ");
            }
            else
            {
                strBuilder.Append("A horde will appear at wave ");
            }
            strBuilder.Append(noticeCodingWaveList[0] + 1);
            warningText.text = strBuilder.ToString();
            strBuilder.Clear();

        }
    }
}