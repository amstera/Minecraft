using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool IsGrounded;

    private GameObject _player;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _verticalMomentum;
    private Vector3 _previousPosition;

    public float Speed = 2f;
    public float JumpForce = 5f;
    public float Gravity = -9.81f;

    private bool _jumpRequest;

    private void Start()
    {
        _player = GameObject.Find("Player");
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (_jumpRequest)
        {
            Jump();
        }

        if (IsGrounded && System.Math.Abs(_previousPosition.magnitude - transform.position.magnitude) < 0.005f)
        {
            _jumpRequest = true;
        }

        _previousPosition = transform.position;

        transform.Translate(_velocity, Space.World);

        if (System.Math.Abs(_rigidbody.velocity.y) < 0.005f)
        {
            IsGrounded = true;
        }
    }

    private void Update()
    {
        GetEnemyRotation();
        if (Vector3.Distance(transform.position, _player.transform.position) > 100)
        {
            Destroy(gameObject);
        }
    }

    void Jump()
    {
        _verticalMomentum = JumpForce;
        IsGrounded = false;
        _jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        if (_verticalMomentum > 0)
        {
            _verticalMomentum = Mathf.Clamp(_verticalMomentum + (Time.fixedDeltaTime * Gravity), 0, JumpForce);
        }

        _velocity = transform.forward * Time.fixedDeltaTime * Speed;
        _velocity += Vector3.up * _verticalMomentum * Time.fixedDeltaTime;
    }

    private void GetEnemyRotation()
    {
        Vector3 targetDirection = _player.transform.position - transform.position;
        float step = Speed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}