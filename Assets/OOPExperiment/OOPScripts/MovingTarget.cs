using UnityEngine;

namespace OOPExperiment
{
    public class MovingTarget : MonoBehaviour
    {
        public float Speed;

        Vector3 initPos;
        public void Start()
        {
            initPos = transform.position;
        }
        public void FixedUpdate()
        {
            initPos.x += Time.fixedDeltaTime * Speed;
            transform.position = initPos;
        }
    }
}