using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walk,
        Sprint,
        Dead
    }
    public Camera cam;

    public float turnSpeed = 0.2f; // 마우스 회전 속도
    public float moveSpeed = 2.0f; // 캐릭터 이동 속도
    public float jumpForce = 7.0f; // 캐릭터 점프 힘

    private Vector2 mousePos = Vector2.zero;
    private Vector3 moveDir = Vector3.zero;

    public LayerMask groundMask;
    public Transform groundCheck;

    private Rigidbody rigid;
    [SerializeField] private bool isGrounded = false;

    [SerializeField] private bool isSprint = false;

    public State state = State.Idle;

    public float maxStamina = 100f;
    public float currStamina;
    [SerializeField] private bool recoveringStamina = false;


    private void Awake()
    {
        cam = Camera.main;
        rigid = GetComponent<Rigidbody>();
        currStamina = maxStamina;
    }

    private void Update()
    {
        StaminaControl();
        if(moveDir ==  Vector3.zero && isGrounded)
        {
            state = State.Idle;
        }
        else if(moveDir != Vector3.zero && !isSprint)
        {
            state = State.Walk;
        }
        else if (moveDir != Vector3.zero && isSprint)
        {
            state = State.Sprint;
        }
    }

    private void FixedUpdate()
    {
        Move();
        isGrounded = Physics.OverlapSphere(groundCheck.position, 0.1f, groundMask).Length > 0;
    }

    private void StaminaControl()
    {
        // 스태미나 회복 가능 상황
        if (currStamina < maxStamina && isGrounded && state != State.Sprint)
        {
            currStamina += 5f * Time.deltaTime;
        }
        // 스태미나 소모 상황
        else if (state == State.Sprint)
        {
            currStamina -= 10f * Time.deltaTime;
        }
    }

    private void Move()
    {
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        float xMoveDir = context.ReadValue<Vector2>().x;
        float zMoveDir = context.ReadValue<Vector2>().y; // context의 y가 실제 움직임에서는 z축과 연결(y는 점프)

        moveDir = new Vector3(xMoveDir, 0, zMoveDir);
    }

    public void MouseMove(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();

        float yRotateSize = mousePos.x * turnSpeed;
        float xRotateSize = -mousePos.y * turnSpeed;
        float xRotate = ClampAngle(cam.transform.localEulerAngles.x + xRotateSize);

        cam.transform.localEulerAngles = new Vector3(xRotate, 0, 0);
        transform.eulerAngles += new Vector3(0, yRotateSize, 0);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if(context.ReadValue<float>() > 0.1f)
        {
            if (currStamina <= 5f)
            {
                Debug.Log("스태미나 없음!");
                return;
            }

            isSprint = true;
            moveSpeed = 6.0f;
        }
        else
        {
            isSprint = false;
            moveSpeed = 2.0f;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            if (currStamina <= 20f)
            {
                Debug.Log("스태미나 없음!");
                return;
            }

            currStamina -= 20f;
            rigid.velocity = new Vector3(rigid.velocity.x, jumpForce, rigid.velocity.z);
        }
    }
    
    public void OnInteraction(InputAction.CallbackContext context)
    {
        Debug.Log("상호작용!");
    }

    public void OnItemUseMain(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.1f)
        {
            Debug.Log("메인기능 시작!");
        }
        else
        {
            Debug.Log("메인기능 끝!");
        }
    }

    public void OnItemUseSub(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.1f)
        {
            Debug.Log("서브기능 시작!");
        }
        else
        {
            Debug.Log("서브기능 끝!");
        }
    }

    public void OnSwitchingTool(InputAction.CallbackContext context)
    {
        if(context.ReadValue<float>() > 0)
        {
            Debug.Log("왼쪽 아이템으로 변경!");
        }
        else if (context.ReadValue<float>() < 0)
        {
            Debug.Log("오른쪽 아이템으로 변경!");
        }
    }

    private float ClampAngle(float angle)
    {
        if(angle > 180)
        {
            angle = Mathf.Clamp(angle, 300, 360);
        }
        else
        {
            angle = Mathf.Clamp(angle, -1, 60);
        }

        return angle;
    }
}
