using UnityEngine;

using CodeIcf.Extensions;

public class PlayerGrab : MonoBehaviour
{
    private const float GRAB_DISTANCE = 2.1f;
    private const int LAYER_OBJECT = 8;

    [SerializeField]
    private Transform m_GrabAnchor = null;
    [SerializeField]
    private SpriteRenderer m_GrabPointView = null;
    [SerializeField]
    private Sprite[] m_GrabPointViewSpriteList = null;
    [SerializeField]
    private DistanceJoint2D m_StartHinge = null;
    [SerializeField]
    private Wire[] m_WireList = null;
    [SerializeField]
    private WireTip m_WireTip = null;
    [SerializeField]
    private Rigidbody2D m_CandidateGrabRigidbory2D = null;
    [SerializeField]
    private Rigidbody2D m_GrabRigidbody2D = null;
    [SerializeField]
    private Vector2 m_ThrowDirection = Vector3.zero;
    [SerializeField]
    private float m_ThrowPower = 10;
    [SerializeField]
    private float m_LiftUpPower = 10;

    [SerializeField]
    private bool m_IsGrab = false;
    public bool IsGrab => m_IsGrab;

    private void Start()
    {
        m_GrabPointView.sprite = m_GrabPointViewSpriteList[ 0 ];
    }

    // Update is called once per frame
    void Update()
    {
        if( IsGrab )
        {
            m_GrabPointView.transform.position = m_WireTip.transform.position;

            return;
        }

        RaycastHit2D[] hitList = Physics2D.RaycastAll( transform.position, m_GrabAnchor.transform.position - transform.position, GRAB_DISTANCE, 1 << LAYER_OBJECT );

        if( hitList == null || hitList.Length <= 0 )
        {
            m_GrabPointView.color = Color.green;
            m_GrabPointView.transform.position = m_GrabAnchor.transform.position;
            m_CandidateGrabRigidbory2D = null;

            return;
        }

        System.Array.Sort( hitList, ( a, b ) =>
        {
            if( a.distance < b.distance ) return -1;
            if( a.distance > b.distance ) return 1;

            return 0;
        } );

        foreach( RaycastHit2D hit in hitList )
        {
            if( hit.distance > GRAB_DISTANCE ) continue;
            if( hit.rigidbody.gameObject.layer != LAYER_OBJECT ) continue;

            m_GrabPointView.color = Color.magenta;
            m_GrabPointView.transform.position = hit.point.ToXY( m_GrabPointView.transform.position.z );
            m_CandidateGrabRigidbory2D = hit.rigidbody;

            return;
        }

        m_GrabPointView.color = Color.green;
        m_GrabPointView.transform.position = m_GrabAnchor.transform.position;
        m_CandidateGrabRigidbory2D = null;
    }

    public void SetGabDirection( Vector2 _axis, bool _isPlayerFIxedGround )
    {
        float angle = Vector2.SignedAngle( Vector2.up, _axis ) + 90;

        if( IsGrab )
        {
            if( _isPlayerFIxedGround )
            {
                m_WireTip.Rigidbody2D.AddForce( _axis * m_LiftUpPower, ForceMode2D.Impulse );
                m_WireTip.SetAngleLimit( _axis );
            }

            if( ( !Mathf.Approximately( 0, _axis.x ) || !Mathf.Approximately( 0, _axis.y ) ) )
            {
                m_GrabPointView.enabled = true;
                m_GrabPointView.transform.eulerAngles = m_GrabPointView.transform.eulerAngles.SetZ( angle );
                m_ThrowDirection = _axis.normalized;
            }
            else
            {
                m_GrabPointView.enabled = false;
                m_ThrowDirection = Vector2.zero;
            }
        }
        else
        {
            if( ( !Mathf.Approximately( 0, _axis.x ) || !Mathf.Approximately( 0, _axis.y ) ) )
            {
                transform.rotation = Quaternion.Euler( 0, 0, angle );
            }
        }
    }

    public bool Grab()
    {
        if( IsGrab ) return false;
        if( m_CandidateGrabRigidbory2D == null ) return false;

        m_GrabRigidbody2D = m_CandidateGrabRigidbory2D;
        m_CandidateGrabRigidbory2D = null;

        m_WireTip.SetConnected( m_GrabRigidbody2D, m_GrabPointView.transform.position, transform.position );

        foreach( Wire wire in m_WireList )
        {
            wire.SetEnable( true );
        }

        m_StartHinge.enabled = true;

        m_IsGrab = true;

        m_GrabPointView.color = Color.cyan;
        m_GrabPointView.sprite = m_GrabPointViewSpriteList[ 1 ];
        m_GrabPointView.enabled = false;

        return true;
    }

    public void Release()
    {
        if( !IsGrab ) return;
        if( m_GrabRigidbody2D == null )
        {
            m_IsGrab = false;
            return;
        }

        m_StartHinge.enabled = false;

        m_WireTip.ReleaseConnected();

        foreach( Wire wire in m_WireList )
        {
            wire.SetEnable( false );
        }

        m_GrabRigidbody2D.AddForceAtPosition( m_ThrowDirection * m_ThrowPower, m_GrabPointView.transform.position, ForceMode2D.Impulse );

        m_GrabPointView.color = Color.green;
        m_GrabPointView.transform.position = m_GrabAnchor.transform.position;
        m_GrabRigidbody2D = null;
        m_CandidateGrabRigidbory2D = null;

        m_IsGrab = false;

        m_GrabPointView.sprite = m_GrabPointViewSpriteList[ 0 ];
        m_GrabPointView.transform.localEulerAngles = Vector3.zero;
        m_GrabPointView.enabled = true;
    }

    public void SwitchingUseAngleLimit()
    {
        m_WireTip.SwitchingUseAngleLimit();
    }
}
