using Unity.Entities;
using UnityEngine;

namespace ProjectGra.PlayerController
{
    public class CameraTargetMonoSingleton: MonoBehaviour
    {
        public static CameraTargetMonoSingleton instance;

        public Transform CameraTargetTransform;

        public void Awake()
        {
            if(instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;
            //CameraTargetTransform = transform;
        }
    }
}