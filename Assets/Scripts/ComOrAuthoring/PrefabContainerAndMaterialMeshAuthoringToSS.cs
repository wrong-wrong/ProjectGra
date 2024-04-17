using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGra
{
    public class PrefabContainerAndMaterialMeshAuthoringToSS : MonoBehaviour
    {
        public GameObject PlayerPrefab;
        public GameObject MaterialPrefab;
        public GameObject ItemPrefab;
        public GameObject SummonExplosionPrefab;
        public GameObject NormalEnemySpawneePrefab;
        public GameObject ScalingSpawneePrefab;

        public Mesh EnemyEggMesh;
        public Mesh EnemyEliteEggAndShootMesh;
        public Mesh EnemyEliteShootMesh;
        public Mesh EnemyEliteSprintAndShootMesh;
        public Mesh EnemyNormalMeleeMesh;
        public Mesh EnemyNormalRangedMesh;
        public Mesh EnemyNormalSprintMesh;
        public Mesh EnemySummonerMesh;
        public class Baker : Baker<PrefabContainerAndMaterialMeshAuthoringToSS>
        {
            public override void Bake(PrefabContainerAndMaterialMeshAuthoringToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PrefabContainerCom
                {
                    MaterialPrefab = GetEntity(authoring.MaterialPrefab, TransformUsageFlags.Renderable),
                    ItemPrefab = GetEntity(authoring.ItemPrefab, TransformUsageFlags.Renderable),
                    SummonExplosionPrefab = GetEntity(authoring.SummonExplosionPrefab, TransformUsageFlags.Dynamic),
                    NormalEnemySpawneePrefab = GetEntity(authoring.NormalEnemySpawneePrefab, TransformUsageFlags.Dynamic),
                    ScalingSpawneePrefab = GetEntity(authoring.ScalingSpawneePrefab, TransformUsageFlags.Dynamic),
                    PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic),
                });
                AddComponentObject(entity, new MeshContainerCom
                {
                    EnemyEggMesh = authoring.EnemyEggMesh,
                    EnemyEliteEggAndShootMesh = authoring.EnemyEliteEggAndShootMesh,
                    EnemyEliteShootMesh = authoring.EnemyEliteShootMesh,
                    EnemyEliteSprintAndShootMesh = authoring.EnemyEliteSprintAndShootMesh,
                    EnemyNormalMeleeMesh = authoring.EnemyNormalMeleeMesh,
                    EnemyNormalRangedMesh = authoring.EnemyNormalRangedMesh,
                    EnemyNormalSprintMesh = authoring.EnemyNormalSprintMesh,
                    EnemySummonerMesh = authoring.EnemySummonerMesh,
                });
            }
        }
    }
    public struct PrefabContainerCom : IComponentData
    {
        public Entity PlayerPrefab;
        public Entity MaterialPrefab;
        public Entity ItemPrefab;
        public Entity SummonExplosionPrefab;
        public Entity NormalEnemySpawneePrefab;
        public Entity ScalingSpawneePrefab;
    }
    public class MeshContainerCom : IComponentData
    {
        public Mesh EnemyEggMesh;
        public Mesh EnemyEliteEggAndShootMesh;
        public Mesh EnemyEliteShootMesh;
        public Mesh EnemyEliteSprintAndShootMesh;
        public Mesh EnemyNormalMeleeMesh;
        public Mesh EnemyNormalRangedMesh;
        public Mesh EnemyNormalSprintMesh;
        public Mesh EnemySummonerMesh;
    }

    public struct BatchMeshIDContainer : IComponentData
    {
        public BatchMeshID EnemyEggMeshID;
        public BatchMeshID EnemyEliteEggAndShootMeshID;
        public BatchMeshID EnemyEliteShootMeshID;
        public BatchMeshID EnemyEliteSprintAndShootMeshID;
        public BatchMeshID EnemyNormalMeleeMeshID;
        public BatchMeshID EnemyNormalRangedMeshID;
        public BatchMeshID EnemyNormalSprintMeshID;
        public BatchMeshID EnemySummonerMeshID;
    }
}