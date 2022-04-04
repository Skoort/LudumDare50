using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3F;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rb2d;

    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        if (!_rb2d)
        { 
            _rb2d = GetComponent<Rigidbody2D>();
        }

        if (!_animator)
        { 
            _animator = GetComponent<Animator>();
        }

        if (!_renderer)
        { 
            _renderer = GetComponent<SpriteRenderer>();
        }
    }

    private Vector3 _moveDir;
    private void Update()
    {
        if (CanMove)
        { 
            _moveDir = new Vector3(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"));

            var isMoving = _moveDir.magnitude > 0.1F;
            _animator.SetBool("IsMoving", isMoving);

            if (_moveDir.x > 0 && _renderer.flipX)
            {
                _renderer.flipX = false;
            } else
            if (_moveDir.x < 0 && !_renderer.flipX)
            { 
                _renderer.flipX = true;
            }
        }
    }

	private void FixedUpdate()
	{
        if (CanMove)
        {
            var clampedMoveDir = Vector3.ClampMagnitude(_moveDir, 1);

            _rb2d.velocity = clampedMoveDir * _moveSpeed;
        }
        else
        {
            _rb2d.velocity = Vector2.zero;
        }
    }
}
