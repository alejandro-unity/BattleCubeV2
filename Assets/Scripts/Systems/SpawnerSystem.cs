using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct SpawnerSystem : ISystem
{   
    public Random Random;
    private bool initialized;
    private JobHandle jobHandle1;
    private JobHandle jobHandle2;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
        Random = new Random((uint)state.WorldUnmanaged.Time.ElapsedTime + 1234);
    }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile (Debug = true )]
    public void OnUpdate(ref SystemState state)
    {
        var soldierQuery = SystemAPI.QueryBuilder().WithAll<Soldier>().Build();
        if (soldierQuery.IsEmpty)
        {
            foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
            {
                var entities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(spawner.ValueRO.Count, ref state.WorldUnmanaged.UpdateAllocator);
                // Create a lot of entities ! 
                state.EntityManager.Instantiate(spawner.ValueRO.Prefab, entities);
            }
        }
        else
        {
            if(!initialized)
            {
                foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
                {
                    var setEntityPosition = new SetEntityPositionJob
                    {
                        random = Random, 
                        circle = spawner.ValueRO.InitialRadius,
                        initialPosition = spawner.ValueRO.InitialPosition,
                        is3DMov = spawner.ValueRO.is3DMov

                    };

                    // Create a query that includes both the Solider and the LocalTransform components
                    EntityQuery newSoldierQuery;
                    if(spawner.ValueRO.Team == (int)SpawnerAuthoring.Teams.TeamA)
                        newSoldierQuery = SystemAPI.QueryBuilder().WithAll<Soldier, LocalTransform, SoldierTeamA>().Build();
                    else 
                        newSoldierQuery = SystemAPI.QueryBuilder().WithAll<Soldier, LocalTransform, SoldierTeamB>().Build();

                    setEntityPosition.ScheduleParallel (newSoldierQuery);
                    Debug.Log("fire job ");
                }
                initialized = true;
            }
            
            
            if(jobHandle1.IsCompleted/* && jobHandle2.IsCompleted*/)
                state.Enabled = false;
        }
        
    }
}

[BurstCompile]
[WithAll(typeof(Soldier))]
[WithAll(typeof(LocalTransform))]
partial struct SetEntityPositionJob: IJobEntity
{
    public Random random;
    public int circle;
    public float3 initialPosition;
    public int is3DMov;

    [BurstCompile]
    public void Execute( ref LocalTransform transform, in Soldier soldier )
    {
        // Generate a random angle in radians
        float randomAngle = random.NextFloat(0, 2 * math.PI);
        float randomRadius = random.NextFloat(0, circle);
        float x = initialPosition.x + randomRadius * math.cos(randomAngle);
        float z = initialPosition.z + randomRadius * math.sin(randomAngle);
        float y = 0;
        if(is3DMov == 1) y = random.NextFloat(0, circle);
        transform.Position = new float3(x,y,z);
    }
}