using UnityEngine;

public class Projectile : PoolableObject
{
    [field: SerializeField]
    public float Speed { get; private set; }

    [field: SerializeField]
    public float SizeMod { get; private set; }

    [field: SerializeField]
    public float Range { get; private set; }

    [field: SerializeField]
    public int Penetration { get; private set; }    

    [field: SerializeField]
    public float MinDamage { get; private set; }
    [field: SerializeField]
    public float MaxDamage { get; private set; }    

    [field: SerializeField]
    public LayerMask HitLayer { get; set; }

    [field: SerializeField]
    public GameObject ArtRoot { get; private set; }

    public GameObject FiredBy { get; set; }
    public Transform Target { get; set; }
    public Vector3 InheritedVelocity { get; set; }

    private float _elapsedRange;
    private int _numImpacts;

    public virtual void OnHit()
    {
        ++_numImpacts;
        if (_numImpacts >= Penetration)
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
        Init();
        base.OnRequested();
    }

	private void Init()
	{
        _elapsedRange = 0;
        _numImpacts = 0;
        ArtRoot.transform.localScale = Vector3.one * SizeMod;
	}

	private void FixedUpdate()
    {
        var delta = (transform.right * Speed + InheritedVelocity) * Time.fixedDeltaTime;
        var deltaMagnitude = delta.magnitude;

        var hitInfo = Physics2D.Raycast(transform.position, delta, deltaMagnitude, HitLayer.value);
        if (hitInfo.transform && hitInfo.transform.gameObject != this.FiredBy)
        {
            transform.position = hitInfo.point;

            var health = hitInfo.transform.root.GetComponent<Health>();
            if (health != null)
            {
                var damage = Random.Range(MinDamage, MaxDamage);

                health.Damage(damage, FiredBy);
            }

            OnHit();
        }
        else
        {
            transform.position += delta;
        }

        _elapsedRange += deltaMagnitude;
        if (_elapsedRange >= Range)
        {
            OnMiss();
        }
    }
}
