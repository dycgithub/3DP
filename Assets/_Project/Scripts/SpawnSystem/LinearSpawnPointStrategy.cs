using UnityEngine;

public class LinearSpawnPointStragegy: ISpawnPointStrategy
{
    private int index = 0;
    private Transform[] spwanPoints;
    
    public LinearSpawnPointStragegy(Transform[] spawnPoints)
    {
        this.spwanPoints = spawnPoints;
    }
    public Transform NextSpawnPoint()
    {
        Transform result = spwanPoints[index];
        index = (index + 1) % spwanPoints.Length;
        return result;
    }
}