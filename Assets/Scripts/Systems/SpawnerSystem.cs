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
    public EntityQuery Query;
    public JobHandle handle;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
        Random = new Random((uint)state.WorldUnmanaged.Time.ElapsedTime + 1234);
        Query = state.GetEntityQuery(ComponentType.ReadOnly<Soldier>());
    }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var soldierQuery = SystemAPI.QueryBuilder().WithAll<Soldier>().Build();
        if (soldierQuery.IsEmpty)
        {
            foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
            {
                var entities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(spawner.ValueRO.Count,ref state.WorldUnmanaged.UpdateAllocator);
                // Create a lot of entities ! 
                state.EntityManager.Instantiate(spawner.ValueRO.Prefab, entities);
            }
            // this job need to  be out the foreach loop
            var setPosJob = new SetEntityPositionJob { random = Random };
            setPosJob.ScheduleParallel();
        }
        
    }
    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}

[BurstCompile]
[WithAll(typeof(Soldier))]
partial struct SetEntityPositionJob: IJobEntity
{
    public Random random;
    
    [BurstCompile]
    public void Execute( ref LocalTransform transform, in Soldier soldier )
    {
        // Generate a random angle in radians
        float randomAngle = random.NextFloat(0, 2 * math.PI);
        float randomRadius = random.NextFloat(0, soldier.InitialRadius);
        float x = soldier.InitialPos.x + randomRadius * math.cos(randomAngle);
        float z = soldier.InitialPos.z + randomRadius * math.sin(randomAngle);
        transform.Position = new float3(x,0,z);
    }
}