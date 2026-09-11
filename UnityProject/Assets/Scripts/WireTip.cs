using System.Collections;
using UnityEngine;

public class WireTip : MonoBehaviour
{
    [SerializeField]
    private Transform m_PrevWire = null;
    [SerializeField]
    private float m_LowerAngleBase = -45f;
    [SerializeField]
    private float m_UpperAngleBase = 45f;

    public Rigidbody2D Rigidbody2D { get; private set; } = null;
    private SpriteRenderer SpriteRenderer { get; set; } = null;
    private HingeJoint2D m_Joint = null;

    public bool IsUseConnectAngleLimit { get; private set; } = false;

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        m_Joint = GetComponent<HingeJoint2D>();
    }

    private void Start()
    {
        Rigidbody2D.simulated = false;
        SpriteRenderer.enabled = false;
    }

    public void SetConnected(Rigidbody2D _target, Vector3 _grabPos, Vector3 _playerPos )
    {
        transform.position = _grabPos;
        m_Joint.enabled = true;
        m_Joint.autoConfigureConnectedAnchor = true;
        m_Joint.connectedBody = _target;
        m_Joint.attachedRigidbody.simulated = true;
        SpriteRenderer.enabled = true;

        if( IsUseConnectAngleLimit )
        {
            float angle = Vector2.Angle( Vector2.right, ( _grabPos - _playerPos ).normalized );
            Rigidbody2D.rotation = angle;
        }

        StartCoroutine(WaitAutoConfigureReelease());
    }

    private IEnumerator WaitAutoConfigureReelease()
    {
        yield return null;

        m_Joint.autoConfigureConnectedAnchor = false;
    }

    public void ReleaseConnected()
    {        
        m_Joint.enabled = false;
        m_Joint.connectedBody = null;
        m_Joint.connectedAnchor = Vector2.zero;
        SpriteRenderer.enabled = false;
        transform.localEulerAngles = Vector3.zero;
        Rigidbody2D.rotation = 0;
        Rigidbody2D.simulated = false;
    }

    public void SwitchingUseAngleLimit()
    {
        IsUseConnectAngleLimit = !IsUseConnectAngleLimit;

        if( !IsUseConnectAngleLimit )
        {
            transform.localEulerAngles = Vector3.zero;
            m_Joint.useLimits = false;
        }
    }

    public void SetAngleLimit( Vector2 _axis )
    {
        if( !IsUseConnectAngleLimit ) return;

        m_Joint.useLimits = true;
        JointAngleLimits2D jointAngleLimits2D = m_Joint.limits;
        jointAngleLimits2D.min = m_LowerAngleBase;
        jointAngleLimits2D.max = m_UpperAngleBase;

        m_Joint.limits = jointAngleLimits2D;

        if( Mathf.Approximately( _axis.x, 0 ) && Mathf.Approximately( _axis.y, 0 ) ) return;

        float angle = Vector2.Angle( Vector2.right, _axis );
        Rigidbody2D.rotation = angle;
    }
}
