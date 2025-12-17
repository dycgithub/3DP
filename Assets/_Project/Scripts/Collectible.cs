using UnityEngine;

public class Collectible:Entity
{
    [SerializeField] int score; // FIXME set using Factory
    //[SerializeField] IntEventChannel scoreChannel;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //scoreChannel.Invoke(score);
            Destroy(gameObject);
        }
    }
}