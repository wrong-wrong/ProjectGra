using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    public class ParticleManager : MonoBehaviour
    {

        public static ParticleManager Instance;
        [SerializeField] GameObject particlePrefab;
        [SerializeField] List<ParticleSystem> particleRingList;
        public int MaxParticleCount;

        private int _particleHead;
        private int _particleTail;
        private int _particleLength;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            particleRingList = new List<ParticleSystem>(MaxParticleCount);
            for (int i = 0; i < MaxParticleCount; ++i)
            {
                particleRingList.Add(Instantiate(particlePrefab).GetComponent<ParticleSystem>());
            }
            _particleHead = 0;
            _particleTail = 0;
            _particleLength = MaxParticleCount;
            this.enabled = false;

        }
        public void FixedUpdate()
        {
            var particlePosList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList;
            if (particlePosList.Length != 0)
            {
                //Debug.Log("ParticlePosList.Length : " + particlePosList.Length);
                var enumList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList;
                //Debug.Log("Buffer length :" + disSqList.Length);
                for (int i = 0, n = particlePosList.Length; i < n; ++i)
                {

                    if (!RequireParticlePlayedAt(particlePosList[i], enumList[i]))
                    {
                        Debug.Log("Break At requesting particle");
                        break;
                    }
                }
                particlePosList.Clear();
                enumList.Clear();
            }

            if (_particleHead != _particleTail)
            {
                for (int i = _particleHead, j = _particleTail; i != j; i = (i + 1) % _particleLength)
                {
                    if (!particleRingList[_particleHead].isPlaying)
                    {
                        _particleHead = (_particleHead + 1) % _particleLength;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        public bool RequireParticlePlayedAt(Vector3 worldPos, ParticleEnum particleEnum = default)
        {
            if ((_particleTail + 1) % _particleLength != _particleHead)
            {

                particleRingList[_particleTail].transform.position = worldPos;
                particleRingList[_particleTail].Play();
                _particleTail = (_particleTail + 1) % _particleLength;
                return true;
            }
            else
            {
                //Debug.Log("No available popup text");
                return false;
            }
        }
    }
    public enum ParticleEnum
    {
        Default,
    }
}