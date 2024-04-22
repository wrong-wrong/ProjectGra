using System.Collections.Generic;
using UnityEngine;

namespace OOPExperiment
{
    public class Spawner : MonoBehaviour
    {
        public Transform TargetTransform;

        public GameObject PalumonPrefab;
        public bool SpawnPalumonGameObject = false;
        public int NumberOfPalumon;
        //public List<GameObject> GOpalumonList;
        public List<PalumonBase> palumonList;


        void Start()
        {
            Debug.LogWarning("Spawner not working.");

            //palumonList = new List<PalumonBase>(10000);
            //for (int i = 0; i < NumberOfPalumon; ++i)
            //{
            //    var palu = Instantiate(PalumonPrefab).GetComponent<PalumonBase>();
            //    //palu.Target = TargetTransform;
            //    palumonList.Add(palu);
            //}

            //for(int i = 0; i < NumberOfPalumon; ++i)
            //{
            //    Instantiate(PalumonPrefab).GetComponent<PalumonBase>().Target = TargetTransform;
            //}


            //if (SpawnPalumonGameObject)
            //{
            //    GOpalumonList = new List<GameObject>(NumberOfPalumon);
            //    for (int i = 0; i < NumberOfPalumon; ++i)
            //    {
            //        GOpalumonList.Add(Instantiate(PalumonPrefab));
            //    }
            //}
            //else
            //{
            //    palumonList = new List<PalumonBase>(NumberOfPalumon);
            //    for (int i = 0; i < NumberOfPalumon; ++i)
            //    {
            //        palumonList.Add(new PalumonBase());
            //    }
            //}
        }

        private void Update()
        {
            //for (int i = 0, n = 10000; i < n; ++i)
            //{
            //    palumonList[i].FollowTarget();
            //}
        }

        //void Update()
        //{
        //    if (Input.GetKeyUp(KeyCode.Space))
        //    {
        //        if (SpawnPalumonGameObject)
        //        {
        //            float begintime = Time.realtimeSinceStartup;
        //            Damage();
        //            Debug.Log(Time.realtimeSinceStartup - begintime);
        //        }
        //        else
        //        {
        //            float begintime = Time.realtimeSinceStartup;
        //            Damage();
        //            Debug.Log(Time.realtimeSinceStartup - begintime);
        //        }
        //    }
        //}

        //public void Damage()
        //{
        //    if (SpawnPalumonGameObject)
        //    {
        //        for (int i = 0; i < NumberOfPalumon; ++i)
        //        {
        //            GOpalumonList[i].GetComponent<PalumonBase>().GetHurt(PalumonType.Water, 3);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < NumberOfPalumon; ++i)
        //        {
        //            palumonList[i].GetHurt(PalumonType.Dark, 3);
        //        }
        //    }
        //}
    }

}