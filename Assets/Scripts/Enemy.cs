using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool IsGrounded;

    public GameObject Blood;

    private GameObject _player;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _verticalMomentum;
    private Vector3 _previousPosition;
    private MeshRenderer[] _meshRenderers;

    public float Speed = 2f;
    public float JumpForce = 5f;
    public float Gravity = -9.81f;
    public bool IsDead;

    public int Health = 20;
    public int Damage = 3;

    private bool _jumpRequest;

    void Start()
    {
        _player = GameObject.Find("Player");
        _rigidbody = GetComponent<Rigidbody>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void FixedUpdate()
    {
        if (IsDead)
        {
            return;
        }

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

    void Update()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) >= 100)
        {
            Destroy(gameObject);
        }

        if (IsDead)
        {
            return;
        }

        GetEnemyRotation();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsDead)
        {
            return;
        }

        if (collision.collider.name == "Player")
        {
            _player.GetComponent<Player>().TakeDamage(Damage);
        }
    }

    public void TakeDamage(Vector3 dir, int damage)
    {
        if (IsDead)
        {
            return;
        }

        _rigidbody.AddForce(dir * 8f, ForceMode.Impulse);
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            meshRenderer.material.color = Color.red;
        }
        Invoke("ResetColor", 0.5f);
        Health -= damage;
        if (Health <= 0)
        {
            Die(dir);
            IsDead = true;
            Destroy(gameObject, 5);
        }
    }

    private void Jump()
    {
        _verticalMomentum = JumpForce;
        IsGrounded = false;
        _jumpRequest = false;
    }

    private void ResetColor()
    {
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            meshRenderer.material.color = Color.white;
        }
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

    private void Die(Vector3 dir)
    {
        GetComponent<Animator>().enabled = false;
        GameObject blood = Instantiate(Blood, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z - 0.5f), Quaternion.identity);
        Destroy(blood, 2);
        foreach (Transform child in transform)
        {
            BoxCollider boxCollider = child.gameObject.GetComponent<BoxCollider>();
            Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(dir * 10, ForceMode.Impulse);
            }
        }
    }
}