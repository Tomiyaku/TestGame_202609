using System.Collections.Generic;
using CodeIcf.Extensions;
using CodeIcf.Input;
using CodeIcf.Input.GamePad;
using UnityEngine;

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
    [SerializeField, Range(0f, 1f)]
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
        InputDeviceManager.CreateInstance();

        CapsuleCollider2D.offset = m_IdleColliderOffset;
    }

    // Update is called once per frame
    void Update()
    {
        List<int> gamepadIdList = InputDeviceManager.Instance.GetGamepadDeviceIdList();

        if (gamepadIdList == null || gamepadIdList.Count <= 0) return;

        GamePadDevice gamePadDevice = InputDeviceManager.Instance.GetGamePadDevice(gamepadIdList[0]);

        if (gamePadDevice == null) return;

        GamePadInputData inputData = gamePadDevice.InputData;

        float hValue = Mathf.Abs(inputData.LStickAxisH);
        Vector2 bodyColliderOffset = hValue < m_ThresholdRunAnime ? m_WalkColliderOffset : m_RunColliderOffset;

        m_Grab.SetGabDirection(inputData.RStickAxis, m_FixedGround.IsFixed);

        IsKeepPositionH = inputData.IsHold(GamepadKeyId.L);

        if (m_Leg.IsGround) Standing(inputData, hValue, bodyColliderOffset);
        else Floting(inputData, hValue, bodyColliderOffset);

        if (inputData.IsDown(GamepadKeyId.RT))
        {
            if (m_Grab.IsGrab) m_Grab.Release();
            else m_Grab.Grab();
        }

        // if (inputData.IsDown(GamepadKeyId.R))
        // {
        //     m_Grab.SwitchingUseAngleLimit();
        // }
    }

    private void Standing(GamePadInputData inputData, float hValue, Vector2 bodyColliderOffset)
    {
        m_JumpStartTime = -1;

        if (m_FixedGround.IsFixed)
        {
            PlayAnimation(ANIMA_NAME_IDLE);

            if (inputData.IsUp(GamepadKeyId.L))
            {
                m_FixedGround.Unpin();
                Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        else
        {
            if (!IsKeepPositionH && !Mathf.Approximately(hValue, 0))
            {
                float rstickH = Mathf.Abs(inputData.RStickAxisH);
                float targetStickH = Mathf.Approximately(rstickH, 0) ? inputData.LStickAxisH : inputData.RStickAxisH; // 自キャラの向きを決めるためのスティック並行入力値

                m_ModelParantTramsform.localEulerAngles = m_ModelParantTramsform.localEulerAngles.SetY(targetStickH < 0 ? -90f : 90f);

                float moveRatio = SetMoveSpeed(inputData.LStickAxisH, inputData.RStickAxisH);
                PlayAnimation(moveRatio < m_ThresholdRunAnime ? ANIMA_NAME_WALK : ANIMA_NAME_RUN, 0.25f);

                CapsuleCollider2D.offset = inputData.LStickAxisH < 0 ? -bodyColliderOffset : bodyColliderOffset;
            }
            else
            {
                m_Leg.SetMotorSpeed(0);
                PlayAnimation(ANIMA_NAME_IDLE);
                CapsuleCollider2D.offset = m_IdleColliderOffset;
            }

            if (inputData.IsDown(GamepadKeyId.L) || inputData.IsHold(GamepadKeyId.L))
            {
                if (m_FixedGround.CheckFiexdGround())
                {
                    m_Leg.SetMotorSpeed(0);
                    PlayAnimation(ANIMA_NAME_IDLE);
                    CapsuleCollider2D.offset = m_IdleColliderOffset;
                    Rigidbody2D.bodyType = RigidbodyType2D.Static;
                }
            }

            if (inputData.IsDown(GamepadKeyId.A))
            {
                Rigidbody2D.AddForce(new Vector2(0, m_JumpPower));
                m_JumpStartTime = Time.unscaledTime;
            }
        }
    }

    private void Floting(GamePadInputData inputData, float hValue, Vector2 bodyColliderOffset)
    {
        if (Rigidbody2D.linearVelocity.y > 0) PlayAnimation(ANIMA_NAME_RISING);
        else if (Rigidbody2D.linearVelocity.y < 0) PlayAnimation(ANIMA_NAME_FALL);

        if (!IsKeepPositionH && hValue >= 0.1f)
        {
            SetMoveSpeed(inputData.LStickAxisH, inputData.RStickAxisH);

            if (inputData.LStickAxisH > 0 && Rigidbody2D.linearVelocity.x < m_MaxAirMoveSpeed) Rigidbody2D.linearVelocity = Rigidbody2D.linearVelocity.AddX(m_AddAirMovePoewer * inputData.LStickAxisH);
            if (inputData.LStickAxisH < 0 && Rigidbody2D.linearVelocity.x > -m_MaxAirMoveSpeed) Rigidbody2D.linearVelocity = Rigidbody2D.linearVelocity.AddX(m_AddAirMovePoewer * inputData.LStickAxisH);

            CapsuleCollider2D.offset = inputData.LStickAxisH < 0 ? -bodyColliderOffset : bodyColliderOffset;
        }
        else
        {
            m_Leg.SetMotorSpeed(0);
            CapsuleCollider2D.offset = m_IdleColliderOffset;
        }
    }

    private float SetMoveSpeed(float _lStickH, float _rStickH)
    {
        float maxSpeed = m_MaxMotorSpeed;
        float moveRatio = Mathf.Abs(_lStickH);

        if (!Mathf.Approximately(_rStickH, 0))
        {
            if ((_lStickH > 0 && _rStickH < 0) || (_lStickH < 0 && _rStickH > 0))
            {
                maxSpeed = m_MaxMotorSpeed * 0.25f;
                moveRatio *= 0.25f;
            }
        }

        m_Leg.SetMotorSpeed(maxSpeed * _lStickH);

        return moveRatio;
    }

    private void PlayAnimation(string _nextAnimaName, float _transitionDuration = 0.1f)
    {
        if (!m_CurrentAnimationName.Equals(_nextAnimaName))
        {
            m_ModelAnimator.CrossFade(_nextAnimaName, _transitionDuration);
        }

        m_CurrentAnimationName = _nextAnimaName;
    }
}

