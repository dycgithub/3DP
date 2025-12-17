using UnityEngine;

public class LootSpawnManager : EntitySpawnManager
{
    [SerializeField] LootData[] LootData;
    
    EntitySpawner<Loot> spawner;
    protected override void Awake()
    {
        base.Awake();
        spawner = new EntitySpawner<Loot>(new EntityFactory<Loot>(LootData), spawnPointStrategy);
    }
    public override void Spawn() => spawner.Spawn();
    public void Spawn(float health)
    { 
        if(health<=0f) Spawn();
    }
}