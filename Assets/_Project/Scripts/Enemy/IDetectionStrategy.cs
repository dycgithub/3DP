using UnityEngine;
using ImprovedTimers;

public interface IDetectionStrategy
{
    bool Execute(Transform player, Transform detector, CountdownTimer timer);
}
