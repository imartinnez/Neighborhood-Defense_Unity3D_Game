using System;
using UnityEngine;

/// Acts as a relay to notify other systems about the enemy lifecycle.
/// In this project, it is mainly used to notify when an enemy is destroyed,
/// allowing the RoundManager to update the remaining enemy count.

public class EnemyLifecycleRelay : MonoBehaviour
{
    /// Event invoked when the enemy is destroyed.
    public Action onDeath;

    /// Called automatically by Unity when the GameObject is destroyed.
    /// Triggers the onDeath event if it has listeners.
    void OnDestroy()
    {
        onDeath?.Invoke();
    }
}
