using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class SpawnerAuthoring : MonoBehaviour
{
    public enum Teams { TeamA = 0 , TeamB = 1  };

    public GameObject prefab;
    public int count;
    public int initialRadius;
    public Vector3 initialPosition;
    public Teams team;
    // Create an entity that has the prefabs 
    private class ConfigAuthoringBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new Spawner
            {
                Prefab  = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                Count = authoring.count,
                Team = (int)authoring.team, 
                InitialRadius = authoring.initialRadius,
                InitialPosition =  authoring.transform.position
            });
        }
    }
}

public struct Soldier: IComponentData
{
}
public struct SoldierTeamA: IComponentData
{
}
public struct SoldierTeamB: IComponentData
{
}
public struct Target : IComponentData
{
    public Entity Value;
}
public struct SoldierOrientation : IComponentData
{
    public float3 Value;
}
public struct SoldierAlive : IComponentData
{
    public int Value;
}

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public int Count;
    public int InitialRadius;
    public float3 InitialPosition;
    public int Team;
}
