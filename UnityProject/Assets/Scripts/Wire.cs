using System.Collections;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public Rigidbody2D Rigidbody2D { get; private set; } = null;
    private SpriteRenderer SpriteRenderer { get; set; } = null;

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();       
    }

    private void Start()
    {
        Rigidbody2D.simulated = false;
        SpriteRenderer.enabled = false;
    }

    public void SetEnable( bool _flag )
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler( Vector3.zero );
        Rigidbody2D.linearVelocity = Vector2.zero;
        Rigidbody2D.angularVelocity = 0;

        Rigidbody2D.simulated = _flag;
        SpriteRenderer.enabled = _flag;
    }
}
