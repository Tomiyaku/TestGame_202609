using UnityEngine;

using CodeIcf.Input;
using CodeIcf.Input.GamePad;
using CodeIcf.Extensions;

public class Player : MonoBehaviour
{
    private const string ANIMA_NAME_IDLE = "Idle";
    private const string ANIMA_NAME_WALK = "Walk";
    private const string ANIMA_NAME_RUN = "Run";
    private const string ANIMA_NAME_JUMP = "Jump";
    private const string ANIMA_NAME_RISING = "Jump";
    private const string ANIMA_NAME_FALL = "Fall";
    private const string ANIMA_NAME_LANDING = "Jump";
    private const string ANIMA_NAME_GRABIDLE = "Idle";
    private const string ANIMA_NAME_GRABPUSH = "Run";
    private const string ANIMA_NAME_GRABPULL = "Run";
    private const string ANIMA_NAME_GRABJUMP = "Jump";
    private const string ANIMA_NAME_LIFTUPIDLE = "Idle";
    private const string ANIMA_NAME_LIFTUPRUN = "Run";
    private const string ANIMA_NAME_LIFTUPJUMP = "Jump";
    private const string ANIMA_NAME_BURNING = "Burned";

    [SerializeField]
    private PlayerLeg m_Leg;
    [SerializeField]
    private Transform m_ModelParantTramsform = null;
    [SerializeField]
    private Animator m_ModelAnimator = null;
    [SerializeField]
    private PlayerGrab m_Grab = null;
    [SerializeField]
    private PlayerFixedGround m_FixedGround = null;
    private Rigidbody2D Rigidbody2D { get; set; } = null;
    private CapsuleCollider2D CapsuleCollider2D { get; set; } = null;
    private string m_CurrentAnimationName = ANIMA_NAME_IDLE;
    private float m_JumpStartTime = -1;
    [SerializeField]
    private float m_MaxMotorSpeed = 750;
    [SerializeField, Range( 0f, 1f )]
    private float m_ThresholdRunAnime = 0.7f;
    [SerializeField]
    private float m_JumpPower = 100f;
    [SerializeField]
    private float m_MaxAirMoveSpeed = 6f;
    [SerializeField]
    private float m_AddAirMovePoewer = 6f;
    [SerializeField]
    private Vector2 m_IdleColliderOffset = Vector2.zero;
    [SerializeField]
    private Vector2 m_WalkColliderOffset = Vector2.zero;
    [SerializeField]
    private Vector2 m_RunColliderOffset = Vector2.zero;
    [field: SerializeField]
    public bool IsKeepPositionH { get; private set; } = false;

    private void Awake()
    {
        Application.targetFrameRate = 30;
        Rigidbody2D = GetComponent<Rigidbody2D>();
        CapsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Grab.Release();
        NSInputManager.CreateInstance();

        CapsuleCollider2D.offset = m_IdleColliderOffset;
    }

    // Update is called once per frame
    void Update()
    {
        NSGamePadDevice gamePadDevice = NSInputManager.Instance.GetInputDeviceToLeadConnected();
        NSGamePadInputData inputData = gamePadDevice != null ? gamePadDevice.InputData : new NSGamePadInputData();

        float hValue = Mathf.Abs( inputData.LStickAxisH );
        Vector2 bodyColliderOffset = hValue < m_ThresholdRunAnime ? m_WalkColliderOffset : m_RunColliderOffset;

        m_Grab.SetGabDirection( inputData.LStickAxis, m_FixedGround.IsFixed );

        IsKeepPositionH = inputData.IsHold( GamepadKeyId.L );

        if( m_Leg.IsGround )
        {
            m_JumpStartTime = -1;

            if( m_FixedGround.IsFixed )
            {
                PlayAnimation(ANIMA_NAME_IDLE);

                if ( inputData.IsUp( GamepadKeyId.L ) )
                {
                    m_FixedGround.Unpin();
                    Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                }
            }
            else
            {
                if( !IsKeepPositionH && hValue >= 0.1f )
                {
                    m_Leg.SetMotorSpeed( m_MaxMotorSpeed * inputData.LStickAxisH );
                    PlayAnimation( hValue < m_ThresholdRunAnime ? ANIMA_NAME_WALK : ANIMA_NAME_RUN, 0.25f );

                    m_ModelParantTramsform.localEulerAngles = m_ModelParantTramsform.localEulerAngles.SetY( inputData.LStickAxisH < 0 ? -90f : 90f );
                    CapsuleCollider2D.offset = inputData.LStickAxisH < 0 ? -bodyColliderOffset : bodyColliderOffset;
                }
                else
                {
                    m_Leg.SetMotorSpeed( 0 );
                    PlayAnimation( ANIMA_NAME_IDLE );
                    CapsuleCollider2D.offset = m_IdleColliderOffset;
                }

                if( inputData.IsDown( GamepadKeyId.L ) || inputData.IsHold( GamepadKeyId.L ) )
                {
                    if( m_FixedGround.CheckFiexdGround() )
                    {
                        m_Leg.SetMotorSpeed( 0 );
                        PlayAnimation( ANIMA_NAME_IDLE );
                        CapsuleCollider2D.offset = m_IdleColliderOffset;
                        Rigidbody2D.bodyType = RigidbodyType2D.Static;
                    }
                }

                if( inputData.IsDown( GamepadKeyId.B ) )
                {
                    Rigidbody2D.AddForce( new Vector2( 0, m_JumpPower ) );
                    m_JumpStartTime = Time.unscaledTime;
                }
            }
        }
        else
        {
            if( Rigidbody2D.velocity.y > 0 ) PlayAnimation( ANIMA_NAME_RISING );
            else if( Rigidbody2D.velocity.y < 0 ) PlayAnimation( ANIMA_NAME_FALL );

            if( !IsKeepPositionH && hValue >= 0.1f )
            {
                m_Leg.SetMotorSpeed( m_MaxMotorSpeed * inputData.LStickAxisH );

                if( inputData.LStickAxisH > 0 && Rigidbody2D.velocity.x < m_MaxAirMoveSpeed ) Rigidbody2D.velocity = Rigidbody2D.velocity.AddX( m_AddAirMovePoewer * inputData.LStickAxisH );
                if( inputData.LStickAxisH < 0 && Rigidbody2D.velocity.x > -m_MaxAirMoveSpeed ) Rigidbody2D.velocity = Rigidbody2D.velocity.AddX( m_AddAirMovePoewer * inputData.LStickAxisH );

                CapsuleCollider2D.offset = inputData.LStickAxisH < 0 ? -bodyColliderOffset : bodyColliderOffset;
            }
            else
            {
                m_Leg.SetMotorSpeed( 0 );
                CapsuleCollider2D.offset = m_IdleColliderOffset;
            }
        }

        if( inputData.IsDown( GamepadKeyId.Y ) )
        {
            if( m_Grab.IsGrab ) m_Grab.Release();
            else m_Grab.Grab();
        }

        if( inputData.IsDown( GamepadKeyId.R ) ) 
        {
            m_Grab.SwitchingUseAngleLimit();
        }
    }

    private void PlayAnimation( string _nextAnimaName, float _transitionDuration = 0.1f )
    {
        if( !m_CurrentAnimationName.Equals( _nextAnimaName ) )
        {
            m_ModelAnimator.CrossFade( _nextAnimaName, _transitionDuration );
        }

        m_CurrentAnimationName = _nextAnimaName;
    }
}

