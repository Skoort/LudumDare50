using UnityEngine;
using UnityEngine.Events;

// A PoolableObject should only be created via the ObjectPool.
public class PoolableObject : MonoBehaviour
{
    public PoolableObject Prefab { get; set; }

    [SerializeField] private bool _shouldToggleActiveness = true;
    [SerializeField] private UnityEvent _onReclaimed;
    [SerializeField] private UnityEvent _onRequested;

    public void OnReclaimed()
    {
        if (_shouldToggleActiveness)
        {
            gameObject.SetActive(false);
        }
        _onReclaimed?.Invoke();
    }

    public virtual void OnRequested()
    {
        if (_shouldToggleActiveness)
        {
            gameObject.SetActive(true);
        }
        _onRequested?.Invoke();
    }
}
