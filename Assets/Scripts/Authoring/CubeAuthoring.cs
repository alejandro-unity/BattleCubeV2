using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class CubeAuthoring : MonoBehaviour
    {
        private class CubeAuthoringBaker : Baker<CubeAuthoring>
        {
            public override void Bake(CubeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TestCube
                {
                });
            }
        }
    }
}
public struct TestCube : IComponentData
{
}