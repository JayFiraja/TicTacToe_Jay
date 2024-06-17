using UnityEngine;

/// <summary>
/// Base class for any SubManager, must implement
/// </summary>
public abstract class SubManager : MonoBehaviour
{
    /// <summary>
    /// Injected Game Manager reference
    /// </summary>
    public GameManager GameManager
    {
        get;
        private set;
    }
    
    /// <summary>
    /// Holds special instances for this submanager
    /// </summary>
    [SerializeField]
    protected Transform content_T;

    /// <summary>
    /// Called by GameManager instance to initialize its SubManagers.
    /// </summary>
    public virtual void SubManagerInitialize(GameManager gameManager)
    {
        GameManager = gameManager;
    }

    /// <summary>
    /// Called every frame by GameManager
    /// </summary>
    public virtual void UpdateManager()
    {
    }

    private void OnDestroy()
    {
        UnSubscribe();
    }

    /// <summary>
    /// Free to override by submanager for unsubscribing purposes.
    /// i.e Action -= Method
    /// </summary>
    public virtual void UnSubscribe()
    {
    }
}

