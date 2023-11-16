using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class SoldierAuthoring : MonoBehaviour
    {
        public int team;
        private class SoldierAuthoringBaker : Baker<SoldierAuthoring>
        {
            public override void Bake(SoldierAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                
                if (authoring.team == (int)SpawnerAuthoring.Teams.TeamA) AddComponent(entity, new SoldierTeamA());
                if (authoring.team == (int)SpawnerAuthoring.Teams.TeamB) AddComponent(entity, new SoldierTeamB());
                    
                AddComponent(entity, new SoldierOrientation());
                AddComponent(entity, new Soldier ());
                AddComponent(entity, new SoldierAlive{ Value = 1 });
                
            }
        }
    }
}