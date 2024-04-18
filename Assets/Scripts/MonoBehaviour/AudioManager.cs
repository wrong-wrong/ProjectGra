using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] GameObject audioPrefab;
        public static AudioManager Instance;
        public int MaxAudioSourceCount;
        [SerializeField] List<AudioSource> audioSourceRingList;
        [SerializeField] List<AudioScriptableObjectConfig> audioSOList;
        private Dictionary<AudioEnum, List<AudioClip>> audioEnumToClipList;
        private int _audioHead;
        private int _audioTail;
        private int _audioLength;

        private Random random;
        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSourceRingList = new List<AudioSource>(MaxAudioSourceCount);
            for(int i = 0; i < MaxAudioSourceCount; ++i)
            {
                audioSourceRingList.Add(Instantiate(audioPrefab).GetComponent<AudioSource>());
            }

            _audioHead = 0;
            _audioTail = 0;
            _audioLength = MaxAudioSourceCount;


            // init random
            random = Random.CreateFromIndex(0);
            //Debug.Log(audioSOList.Count);
            //Debug.Log(audioSOList[0].audioEnum);
            //Debug.Log(audioSOList[0].audioClips.Count);
            audioEnumToClipList = new Dictionary<AudioEnum, List<AudioClip>>(audioSOList.Count);
            //// populate the dictionary with so
            for (int i = 0; i < audioSOList.Count; ++i)
            {
                audioEnumToClipList[audioSOList[i].audioEnum] = audioSOList[i].audioClips;
            }
            //Debug.Log(audioEnumToClipList.Keys.ToString());
            //Debug.Log(audioEnumToClipList.Values.ToString());
            Debug.LogWarning("Using Play() to play default particle, play particle related to the particle enum");
            this.enabled = false;

        }

        public void FixedUpdate()
        {
            var audioPosList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList;
            if (audioPosList.Length != 0)
            {
                //Debug.Log("Audio Pos List Length : " +  audioPosList.Length);
                var enumList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList;
                //Debug.Log("Buffer length :" + disSqList.Length);
                for (int i = 0, n = audioPosList.Length; i < n; ++i)
                {

                    if (!RequireAudioPlayedAt(audioPosList[i], enumList[i]))
                    {
                        //Debug.Log("Require audio play failed");
                        //Debug.Log("Break At requesting popup text");
                        break;
                    }
                }
                audioPosList.Clear();
                enumList.Clear();
            }

            
            if (_audioHead != _audioTail)
            {
                //
                for (int i = _audioHead, j = _audioTail; i != j; i = (i + 1) % _audioLength)
                {
                    if (!audioSourceRingList[_audioHead].isPlaying)
                    {
                        _audioHead = (_audioHead + 1) % _audioLength;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //Debug.Log(ringListHead + " - " + ringListTail);
            //var deltatime = Time.deltaTime;



        }
        public bool RequireAudioPlayedAt(Vector3 worldPos, AudioEnum audioEnum = default)
        {
            if ((_audioTail + 1) % _audioLength != _audioHead)
            {
                if (!audioEnumToClipList.TryGetValue(audioEnum, out List<AudioClip> clipList))
                {
                    //Debug.Log("audioEnum : " +  audioEnum + " - Not found in enumToClipList");
                    return true;
                }
                //Debug.Log("AudioEnum : " + audioEnum + "ClipListCount :" + clipList.Count);
                //audioSourceRingList[_audioTail].
                audioSourceRingList[_audioTail].transform.position = worldPos;
                //Debug.LogWarning("Using Play() to play default audio clip, should use PlayOnShot with audioClip related to the audio enum");
                if(clipList.Count > 1)
                {
                    audioSourceRingList[_audioTail].PlayOneShot(clipList[random.NextInt(clipList.Count)]);
                }
                else
                {
                    audioSourceRingList[_audioTail].PlayOneShot(clipList[0]);
                }
                _audioTail = (_audioTail + 1) % _audioLength;
                return true;
            }
            else
            {
                //Debug.Log("No available popup text");
                return false;
            }
        }

    }
    public enum AudioEnum
    {
        NormalShoot,
        Explode,
    }

}