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

    public float turnSpeed = 0.2f; // ���콺 ȸ�� �ӵ�
    public float moveSpeed = 2.0f; // ĳ���� �̵� �ӵ�
    public float jumpForce = 7.0f; // ĳ���� ���� ��

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
        // ���¹̳� ȸ�� ���� ��Ȳ
        if (currStamina < maxStamina && isGrounded && state != State.Sprint)
        {
            currStamina += 5f * Time.deltaTime;
        }
        // ���¹̳� �Ҹ� ��Ȳ
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
        float zMoveDir = context.ReadValue<Vector2>().y; // context�� y�� ���� �����ӿ����� z��� ����(y�� ����)

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
                Debug.Log("���¹̳� ����!");
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
                Debug.Log("���¹̳� ����!");
                return;
            }

            currStamina -= 20f;
            rigid.velocity = new Vector3(rigid.velocity.x, jumpForce, rigid.velocity.z);
        }
    }
    
    public void OnInteraction(InputAction.CallbackContext context)
    {
        Debug.Log("��ȣ�ۿ�!");
    }

    public void OnItemUseMain(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.1f)
        {
            Debug.Log("���α�� ����!");
        }
        else
        {
            Debug.Log("���α�� ��!");
        }
    }

    public void OnItemUseSub(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.1f)
        {
            Debug.Log("������ ����!");
        }
        else
        {
            Debug.Log("������ ��!");
        }
    }

    public void OnSwitchingTool(InputAction.CallbackContext context)
    {
        if(context.ReadValue<float>() > 0)
        {
            Debug.Log("���� ���������� ����!");
        }
        else if (context.ReadValue<float>() < 0)
        {
            Debug.Log("������ ���������� ����!");
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
