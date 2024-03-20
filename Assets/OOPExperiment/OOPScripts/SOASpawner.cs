using System.Collections.Generic;
using UnityEngine;

namespace OOPExperiment
{
    public class SOASpawner : MonoBehaviour
    {
        public int palumonNumber;
        public WaterResistance waterResistanceSOA;
        public PalumonHealthPoint palumonHealthPointSOA;
        public void Start()
        {
            waterResistanceSOA = new WaterResistance();
            palumonHealthPointSOA = new PalumonHealthPoint();
            waterResistanceSOA.waterResistanceList = new List<float>(palumonNumber);
            palumonHealthPointSOA.palumonHealthList = new List<float>(palumonNumber);
            for(int i = 0; i < palumonNumber; i++)
            {
                waterResistanceSOA.waterResistanceList.Add(0.8f);
                palumonHealthPointSOA.palumonHealthList.Add(100f);
            }
        }
        public void Damage(int damage)
        {
            for(int i = 0; i < palumonNumber; i++)
            {
                palumonHealthPointSOA.palumonHealthList[i] -= damage * (1 - waterResistanceSOA.waterResistanceList[i]);
            }
        }
        public void Update()
        {
            if(Input.GetKeyUp(KeyCode.Space))
            {
                var begintime = Time.realtimeSinceStartup;
                Damage(3);
                Debug.Log(Time.realtimeSinceStartup - begintime);
            }
        }

    }
}