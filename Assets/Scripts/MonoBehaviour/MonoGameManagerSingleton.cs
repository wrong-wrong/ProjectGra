using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra{

    public class MonoGameManagerSingleton : MonoBehaviour
    {
        public static MonoGameManagerSingleton Instance;
        public int CurrentDifficulty;
        public List<SpawningScriptableObjectConfig> SpawningSOList;
        public bool IsSelectionDone;
        private void Awake()
        {
            if(Instance != null) 
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        // after select difficulty, ECS should get the corresponding SpawningSO and store its info in a SpawningConfig BufferComponent
    }

}