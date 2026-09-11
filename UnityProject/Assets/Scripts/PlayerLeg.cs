using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLeg : MonoBehaviour
{
    [SerializeField]
    private bool m_IsGround = true;
    public bool IsGround => m_IsGround;
    private HingeJoint2D HingeJoint2D { get; set; } = null;
    private List<GameObject> m_HitObjectList = new List<GameObject>();
    private Rigidbody2D Rigidbody2D { get; set; } = null;

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        HingeJoint2D = GetComponent<HingeJoint2D>();
    }

    private void OnCollisionEnter2D( Collision2D _collision )
    {
        if( m_HitObjectList.Find( ( obj ) => obj == _collision.gameObject ) == null ) m_HitObjectList.Add( _collision.gameObject );

        m_IsGround = m_HitObjectList.Count > 0;
    }

    private void OnCollisionStay2D( Collision2D _collision )
    {
        if( m_HitObjectList.Find( ( obj ) => obj == _collision.gameObject ) == null ) m_HitObjectList.Add( _collision.gameObject );

        m_IsGround = m_HitObjectList.Count > 0;
    }

    private void OnCollisionExit2D( Collision2D _collision )
    {
        m_HitObjectList.Remove( _collision.gameObject );

        m_IsGround = m_HitObjectList.Count > 0;
    }

    public void SetMotorSpeed( float _speed )
    {
        JointMotor2D jointMotor2D = HingeJoint2D.motor;
        jointMotor2D.motorSpeed = _speed;
        HingeJoint2D.motor = jointMotor2D;
    }
}
