using UnityEngine;

public class PlayerFixedGround : MonoBehaviour
{
    private const int LAYER_GROUND = 6;
    private const int LAYER_OBJECT = 8;
    private const int LAYERMASK_FIXEDCHECK = 1 << LAYER_GROUND | 1 << LAYER_OBJECT;

    [ SerializeField]
    private LineRenderer m_FrontFixedLine = null;
    [SerializeField]
    private LineRenderer m_BackFixedLine = null;

    [field: SerializeField]
    public bool IsFixed { get; private set; } = false;
    [SerializeField]
    private float m_FixedCheckDistance = 1.2f;


    private void Start()
    {
        Unpin();
    }

    public bool CheckFiexdGround()
    {
        bool isFrontHit = IsFixedGround( m_FrontFixedLine.transform.position, m_FrontFixedLine.transform.localPosition.normalized, out Vector2 ftontHitPoint );
        bool isBackHit = IsFixedGround( m_BackFixedLine.transform.position, m_BackFixedLine.transform.localPosition.normalized, out Vector2 backHitPoint );

        if( !isFrontHit || !isBackHit ) return false;

        m_FrontFixedLine.useWorldSpace = true;
        m_FrontFixedLine.SetPosition( 0, transform.position );
        m_FrontFixedLine.SetPosition( 1, ftontHitPoint );
        m_FrontFixedLine.enabled = true;

        m_BackFixedLine.useWorldSpace = true;
        m_BackFixedLine.SetPosition( 0, transform.position );
        m_BackFixedLine.SetPosition( 1, backHitPoint );
        m_BackFixedLine.enabled = true;

        IsFixed = true;

        return true;
    }

    public void Unpin()
    {
        m_FrontFixedLine.enabled = false;
        m_BackFixedLine.enabled = false;
        IsFixed = false;
    }

    private bool IsFixedGround( Vector2 _pos, Vector2 _direction, out Vector2 hitPoint )
    {
        hitPoint = Vector2.zero;
        RaycastHit2D[] hitList =  Physics2D.RaycastAll( _pos, _direction, m_FixedCheckDistance, LAYERMASK_FIXEDCHECK );

        if( hitList == null || hitList.Length <= 0 ) return false;

        System.Array.Sort( hitList, ( a, b ) =>
        {
            if( a.distance < b.distance ) return -1;
            if( a.distance > b.distance ) return 1;

            return 0;
        } );

        if( hitList[ 0 ].rigidbody.gameObject.layer != LAYER_GROUND ) return false;

        hitPoint = hitList[ 0 ].point;

        return true;
    }
}
