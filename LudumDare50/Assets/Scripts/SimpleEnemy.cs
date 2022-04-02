using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [SerializeField] private Transform _target = default;

    [SerializeField] private float _moveSpeed = 3F;
    [SerializeField] private float _strikeRange = 1F;

    [SerializeField] private Animator _animator = default;
    [SerializeField] private SpriteRenderer _renderer = default;
    private Rigidbody2D _rb2d;

    private Coroutine _bunchingAvoidance;

    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();

        if (!_animator)
        {
            _animator = GetComponent<Animator>();
        }

        if (!_renderer)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        _target = GameObject.FindWithTag("Player").transform;

		_bunchingAvoidance = StartCoroutine(BunchingAvoidance());
	}

    private float _distanceToTarget;
    private Vector3 _moveDir;
    private bool _isMoving;
    private void Update()
    {
        var toFrom = _target.position - transform.position;
        _moveDir = toFrom.normalized;
        _distanceToTarget = toFrom.magnitude;

        _isMoving = _distanceToTarget > _strikeRange;
        _animator.SetBool("IsMoving", _isMoving);

        if (_moveDir.x > 0 && _renderer.flipX)
        {
            _renderer.flipX = false;
        }
        else
        if (_moveDir.x < 0 && !_renderer.flipX)
        {
            _renderer.flipX = true;
        }
    }

	private void FixedUpdate()
	{
        if (_isMoving)
        {
            var avoidanceScale = Mathf.Lerp(0.8F, 0.5F, Mathf.Clamp01(_distanceToTarget / 5));
            var avoidance = -_vectorToAvgNeighbor * avoidanceScale;
            var desiredMovement = _moveDir;

            var combined = Vector3.ClampMagnitude(avoidance + desiredMovement, 1);

            _rb2d.velocity = combined * _moveSpeed;
            //transform.position += _moveDir * _moveSpeed * Time.fixedDeltaTime;
        }
        else
        {
            _rb2d.velocity = Vector2.zero;
        }
    }

    [SerializeField] private float _avoidanceRadius = 2;
    [SerializeField] private LayerMask _enemyLayer = default;
    private Vector3 _vectorToAvgNeighbor;
    private IEnumerator BunchingAvoidance()
    {
        while (true)
        { 
            _vectorToAvgNeighbor = Vector3.zero;

            var filter = new ContactFilter2D() { layerMask = _enemyLayer };
            var results = new List<Collider2D>();
            if (Physics2D.OverlapCircle(transform.position, _avoidanceRadius, filter, results) > 0)
            {
                foreach (var result in results)
                {
                    if (result.gameObject.tag == "Player")
                    {
                        continue;  // We found the player. Skip.
                    }
                    if (result.gameObject == gameObject)
                    {
                        continue;  // We found ourselves. Skip.
                    }

                    var dirToNeighbor = result.gameObject.transform.position - transform.position;
                    _vectorToAvgNeighbor += dirToNeighbor;
                }
                _vectorToAvgNeighbor /= results.Count;
            }
            yield return new WaitForSeconds(0.1F);
        }
    }

	private void OnDisable()
	{
        StopCoroutine(_bunchingAvoidance);
	}
}
