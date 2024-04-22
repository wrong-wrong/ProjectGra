using UnityEngine;

namespace OOPExperiment
{
    public class PalumonFollow : MonoBehaviour
    {
        [SerializeField]private Transform targetTransform;
        private Vector3 tarDirMulSpeedV3 = Vector3.one;
        private void Update()
        {
            var deltatime = Time.deltaTime;
            transform.position += tarDirMulSpeedV3 * deltatime;
        }

    }

}