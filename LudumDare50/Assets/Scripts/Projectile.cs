using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : PoolableObject
{
    [field: SerializeField]
    public float Speed { get; private set; }

    [SerializeField] private float _sizeMod = 1;
    public float SizeMod 
    {
        get => _sizeMod;
        set
        {
            _sizeMod = value;
            ArtRoot.transform.localScale = Vector3.one * value;
        }
    }

    [field: SerializeField, Tooltip("If negative, this projectile doesn't despawn after travelling a set distance.")]
    public float Range { get; set; }

    [field: SerializeField, Tooltip("If negative, this projectile doesn't despawn after a set amount of time.")]
    public float Lifetime { get; set; }

    [field: SerializeField, Tooltip("How many entities can this projectile penetrate?")]
    public int Penetration { get; set; }    

    [field: SerializeField]
    public float MinDamage { get; set; }
    [field: SerializeField]
    public float MaxDamage { get; set; }    

    [field: SerializeField]
    public LayerMask HitLayer { get; set; }

    [field: SerializeField]
    public GameObject ArtRoot { get; set; }

    public GameObject FiredBy { get; set; }
    public Transform Target { get; set; }
    public Vector3 InheritedVelocity { get; set; }

    protected float _elapsedTime;
    protected float _elapsedRange;
    protected int _numImpacts;

    public virtual void OnHit(GameObject hitObject)
    {
        _hitObjects.Add(hitObject);

        ++_numImpacts;
        if (_numImpacts > Penetration)
        { 
            ObjectPool.Instance.ReleaseInstance(this);
        }
    }

    public virtual void OnMiss()
    {
        ObjectPool.Instance.ReleaseInstance(this);
    }

	private void Awake()
	{
        Init();
	}

    public override void OnRequested()
    {
        base.OnRequested();
        Init();
    }

	protected virtual void Init()
	{
        _numImpacts = 0;
        _hitObjects.Clear();

        _elapsedTime = 0;
        _elapsedRange = 0;
        _prevPrevPosition = transform.position;
        _prevPosition = transform.position;
	}

    private List<GameObject> _hitObjects = new List<GameObject>();

    private Vector3 _prevPosition;
    private Vector3 _prevPrevPosition;
	private void FixedUpdate()
    {
        _prevPrevPosition = _prevPosition;
        _prevPosition = transform.position;

        _elapsedRange += Vector3.Distance(_prevPrevPosition, _prevPosition);
        if (Range > 0 && _elapsedRange >= Range)
        {
            OnMiss();
            return;
        }

        _elapsedTime += Time.fixedDeltaTime;
        if (Lifetime > 0 && _elapsedTime >= Lifetime)
        {
            OnMiss();
            return;
        }

        CalculateMovement();
    }

    protected abstract void CalculateMovement();

    protected bool ShouldRegisterHit(GameObject hitObject)
    {
        return hitObject != this.FiredBy && !_hitObjects.Contains(hitObject) && ((1 << hitObject.layer) & HitLayer.value) > 0;
    }
}
