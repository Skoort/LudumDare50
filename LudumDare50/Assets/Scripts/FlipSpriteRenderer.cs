using UnityEngine;

public class FlipSpriteRenderer : MonoBehaviour
{
	[SerializeField] private SpriteRenderer _renderer = default;

	private void Update()
    {
        _renderer.flipX = transform.right.x < 0;
    }
}
