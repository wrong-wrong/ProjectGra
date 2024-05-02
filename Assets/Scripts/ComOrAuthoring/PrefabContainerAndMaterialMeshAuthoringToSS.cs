using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGra
{
    public class PrefabContainerAndMaterialMeshAuthoringToSS : MonoBehaviour
    {
        [Header("Collider Prefab")]
        public GameObject EnemyEliteEggAndShootCollider;
        public GameObject EnemyEliteShootCollider;
        public GameObject EnemyEliteSprintAndShootCollider;
        public GameObject EnemyNormalMeleeCollider;
        public GameObject EnemyNormalRangedCollider;
        public GameObject EnemyNormalSprintCollider;
        public GameObject EnemySummonerCollider;

        public GameObject PlayerPrefab;
        public GameObject MaterialPrefab;
        public GameObject NormalCratePrefab;
        public GameObject LegendaryCratePrefab;
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
                    NormalCratePrefab = GetEntity(authoring.NormalCratePrefab, TransformUsageFlags.Renderable),
                    SummonExplosionPrefab = GetEntity(authoring.SummonExplosionPrefab, TransformUsageFlags.Dynamic),
                    NormalEnemySpawneePrefab = GetEntity(authoring.NormalEnemySpawneePrefab, TransformUsageFlags.Dynamic),
                    ScalingSpawneePrefab = GetEntity(authoring.ScalingSpawneePrefab, TransformUsageFlags.Dynamic),
                    PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic),
                    LegendaryCratePrefab = GetEntity(authoring.LegendaryCratePrefab, TransformUsageFlags.Dynamic),
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
                AddComponent(entity, new RealColliderPrefabContainerCom
                {
                    EnemyEliteEggAndShootCollider = GetEntity(authoring.EnemyEliteEggAndShootCollider, TransformUsageFlags.None),
                    EnemyEliteShootCollider = GetEntity(authoring.EnemyEliteEggAndShootCollider, TransformUsageFlags.None),
                    EnemyEliteSprintAndShootCollider = GetEntity(authoring.EnemyEliteSprintAndShootCollider, TransformUsageFlags.None),
                    EnemyNormalMeleeCollider = GetEntity(authoring.EnemyNormalMeleeCollider, TransformUsageFlags.None),
                    EnemyNormalRangedCollider = GetEntity(authoring.EnemyNormalRangedCollider, TransformUsageFlags.None),
                    EnemyNormalSprintCollider = GetEntity(authoring.EnemyNormalSprintCollider, TransformUsageFlags.None),
                    EnemySummonerCollider = GetEntity(authoring.EnemySummonerCollider, TransformUsageFlags.None),
                });
            }
        }
    }
    public struct RealColliderPrefabContainerCom : IComponentData
    {
        public Entity EnemyEliteEggAndShootCollider;
        public Entity EnemyEliteShootCollider;
        public Entity EnemyEliteSprintAndShootCollider;
        public Entity EnemyNormalMeleeCollider;
        public Entity EnemyNormalRangedCollider;
        public Entity EnemyNormalSprintCollider;
        public Entity EnemySummonerCollider;
    }
    public struct PrefabContainerCom : IComponentData
    {
        public Entity PlayerPrefab;
        public Entity MaterialPrefab;
        public Entity NormalCratePrefab;
        public Entity SummonExplosionPrefab;
        public Entity NormalEnemySpawneePrefab;
        public Entity ScalingSpawneePrefab;
        public Entity LegendaryCratePrefab;
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