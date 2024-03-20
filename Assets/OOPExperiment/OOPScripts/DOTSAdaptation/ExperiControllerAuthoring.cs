using Unity.Entities;
using UnityEngine;

namespace OOPExperiment
{

    public class ExperiControllerAuthoring : MonoBehaviour
    {
        public bool ExperiExecute;
        public class Baker : Baker<ExperiControllerAuthoring>
        {
            public override void Bake(ExperiControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                if(authoring.ExperiExecute) AddComponent<ExperiExecuteTag>(entity);
            }
        }
    }

    public struct ExperiExecuteTag : IComponentData { }
}