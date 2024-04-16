using UnityEngine;

namespace ProjectGra
{
    public class AudioManager : MonoBehaviour
    {
        public ParticleSystem particle;
        public AudioSource audioSource;
        public static AudioManager Instance;
        public void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        public void RequireAudioPlayedAt(Vector3 worldPos)
        {
            audioSource.transform.position = worldPos;
            audioSource.Play();
            particle.transform.position = worldPos;
            particle.Play();
        }
    }
}