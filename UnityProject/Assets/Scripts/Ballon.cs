using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballon : MonoBehaviour
{
    [SerializeField,Range( 0f, 10f)]
    private float m_RiseVelocity = 1f;

    private Rigidbody2D Rigidbody2D { get; set; } = null;

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Rigidbody2D.velocity= Vector2.up * m_RiseVelocity;
    }
}
