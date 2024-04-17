using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyAudioSO")]
    public class AudioScriptableObjectConfig : ScriptableObject
    {
        public AudioEnum audioEnum;
        public List<AudioClip> audioClips;
    }
}