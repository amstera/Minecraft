using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool IsGrounded;

    private GameObject _player;
    private World _world;

    public float Speed = 2f;
    public float JumpForce = 5f;
    public float Gravity = -9.81f;

    public float Width = 0.5f;
    public float CheckIncrement = 0.075f;
    public float Reach = 8;

    private float _horizontal;
    private float _vertical;
    private Vector3 _velocity;
    private float _verticalMomentum;
    private bool _jumpRequest;

    private void Start()
    {
        _player = GameObject.Find("Player");
        _world = GameObject.Find("World").GetComponent<World>();
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (_jumpRequest)
        {
            Jump();
        }

        transform.Translate(_velocity, Space.World);
    }

    private void Update()
    {
        GetEnemyMovement();
    }

    void Jump()
    {
        _verticalMomentum = JumpForce;
        IsGrounded = false;
        _jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity.
        if (_verticalMomentum > Gravity)
        {
            _verticalMomentum += Time.fixedDeltaTime * Gravity;
        }

        _velocity = ((transform.forward * _vertical) + (transform.right * _horizontal)) * Time.fixedDeltaTime * Speed;

        // Apply vertical momentum (falling/jumping).
        _velocity += Vector3.up * _verticalMomentum * Time.fixedDeltaTime;

        if ((_velocity.z > 0 && Front) || (_velocity.z < 0 && Back))
        {
            _velocity.z = 0;
        }
        if ((_velocity.x > 0 && Right) || (_velocity.x < 0 && Left))
        {
            _velocity.x = 0;
        }

        if (_velocity.y < 0)
        {
            _velocity.y = CheckDownSpeed(_velocity.y);
        }
        else if (_velocity.y > 0)
        {
            _velocity.y = CheckUpSpeed(_velocity.y);
        }
    }

    private void GetEnemyMovement()
    {
        Vector3.RotateTowards(transform.position, _player.transform.position, 10, 100);
        Vector3 pos = Vector3.MoveTowards(transform.position, _player.transform.position, 100);
        _horizontal = pos.normalized.x;
        _vertical = pos.normalized.z;

        if (IsGrounded && _velocity.x <= 0 && _velocity.z <= 0)
        {
            _jumpRequest = true;
        }
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (
            _world.CheckForVoxel(transform.position.x - Width, transform.position.y + downSpeed, transform.position.z - Width) ||
            _world.CheckForVoxel(transform.position.x + Width, transform.position.y + downSpeed, transform.position.z - Width) ||
            _world.CheckForVoxel(transform.position.x + Width, transform.position.y + downSpeed, transform.position.z + Width) ||
            _world.CheckForVoxel(transform.position.x - Width, transform.position.y + downSpeed, transform.position.z + Width)
           )
        {
            IsGrounded = true;
            return 0;

        }

        IsGrounded = false;
        return downSpeed;
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (
            _world.CheckForVoxel(transform.position.x - Width, transform.position.y + 2f + upSpeed, transform.position.z - Width) ||
            _world.CheckForVoxel(transform.position.x + Width, transform.position.y + 2f + upSpeed, transform.position.z - Width) ||
            _world.CheckForVoxel(transform.position.x + Width, transform.position.y + 2f + upSpeed, transform.position.z + Width) ||
            _world.CheckForVoxel(transform.position.x - Width, transform.position.y + 2f + upSpeed, transform.position.z + Width)
           )
        {

            return 0;

        }

        return upSpeed;

    }

    public bool Front
    {

        get
        {
            if (
                _world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + Width) ||
                _world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + Width)
                )
            {
                return true;
            }

            return false;
        }

    }
    public bool Back
    {

        get
        {
            if (
                _world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - Width) ||
                _world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - Width)
                )
            {
                return true;
            }

            return false;
        }

    }
    public bool Left
    {

        get
        {
            if (
                _world.CheckForVoxel(transform.position.x - Width, transform.position.y, transform.position.z) ||
                _world.CheckForVoxel(transform.position.x - Width, transform.position.y + 1f, transform.position.z)
                )
            {
                return true;
            }

            return false;
        }

    }
    public bool Right
    {

        get
        {
            if (
                _world.CheckForVoxel(transform.position.x + Width, transform.position.y, transform.position.z) ||
                _world.CheckForVoxel(transform.position.x + Width, transform.position.y + 1f, transform.position.z)
                )
            {
                return true;
            }

            return false;
        }

    }

}