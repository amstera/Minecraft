using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool IsGrounded;

    public GameObject Blood;
    public GameObject Explosion;
    public GameObject TNT;
    public EnemyType Type;
    public List<AudioClip> Sounds;
    public AudioSource EnemyAudioSource;

    private GameObject _player;
    private World _world;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _verticalMomentum;
    private Vector3 _previousPosition;
    private MeshRenderer[] _meshRenderers;
    private float _timeSinceLastSpoke;
    private float _explosionTimer;
    private float _lastDropTime;
    private Vector3 _localScale;
    private List<Color?> _meshColors = new List<Color?>();

    public bool CanFly;
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
        _world = GameObject.Find("World").GetComponent<World>();
        _rigidbody = GetComponent<Rigidbody>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        _localScale = transform.localScale;

        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            if (meshRenderer.material.HasProperty("_Color"))
            {
                _meshColors.Add(meshRenderer.material?.color);
            }
            else
            {
                _meshColors.Add(null);
            }
        }
    }

    void FixedUpdate()
    {
        if (IsDead)
        {
            return;
        }

        CalculateVelocity();

        if (!CanFly)
        {
            if (_jumpRequest)
            {
                Jump();
            }

            if (IsGrounded && System.Math.Abs(_previousPosition.magnitude - transform.position.magnitude) < 0.005f)
            {
                _jumpRequest = true;
            }

        }

        _previousPosition = transform.position;

        transform.Translate(_velocity, Space.World);

        if (!CanFly)
        {
            if (System.Math.Abs(_rigidbody.velocity.y) < 0.005f)
            {
                IsGrounded = true;
            }
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

        if ((Type == EnemyType.Zombie || Type == EnemyType.FlyingCreeper) && Time.time - _timeSinceLastSpoke > 5)
        {
            EnemyAudioSource.clip = Sounds[0];
            EnemyAudioSource.Play();
            _timeSinceLastSpoke = Time.time;
        }

        if (Type == EnemyType.Zombie)
        {
            if (Vector3.Distance(_player.transform.position, transform.position) < 1f)
            {
                _player.GetComponent<Player>().TakeDamage(Damage);
            }
        }
        else if (Type == EnemyType.Creeper)
        {
            if (Vector3.Distance(_player.transform.position, transform.position) < 3f)
            {
                if (!EnemyAudioSource.isPlaying)
                {
                    EnemyAudioSource.clip = Sounds[0];
                    EnemyAudioSource.Play();
                }
                transform.localScale += new Vector3(0.075f, 0.075f, 0.075f) * Time.deltaTime;
                _explosionTimer += Time.deltaTime;
                if (_explosionTimer >= 1.5f)
                {
                    Explode();
                }
            }
            else
            {
                _explosionTimer = 0;
                transform.localScale = _localScale;
            }
        }
        else if (Type == EnemyType.FlyingCreeper)
        {
            if (Time.time - _lastDropTime >= 10 && Vector3.Distance(transform.position, _player.transform.position) < 20)
            {
                Instantiate(TNT, transform.position, Quaternion.identity);
                _lastDropTime = Time.time;
            }
        }

        GetEnemyRotation();
    }

    public void TakeDamage(Vector3 dir, Vector3? hitPoint, int damage)
    {
        if (IsDead)
        {
            return;
        }

        EnemyAudioSource.clip = Sounds[1];
        EnemyAudioSource.Play();

        if (!CanFly)
        {
            _rigidbody.AddForce(dir * 8f, ForceMode.Impulse);
        }
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            meshRenderer.material.color = Color.red;
        }
        Invoke("ResetColor", 0.5f);
        Health -= damage;
        if (Health <= 0)
        {
            Die(dir, hitPoint);
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
        int i = 0;
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            if (_meshColors[i] != null)
            {
                meshRenderer.material.color = _meshColors[i].Value;
            }
            i++;
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

    private void Die(Vector3 dir, Vector3? hitPoint)
    {
        IsDead = true;
        EnemyAudioSource.clip = Sounds[2];
        EnemyAudioSource.Play();
        GetComponent<Animator>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = false;
        if (hitPoint != null)
        {
            GameObject blood = Instantiate(Blood, hitPoint.Value, Quaternion.identity);
            Destroy(blood, 2);
        }
        Transform trans = transform.Find("Root") ? transform.Find("Root") : transform;
        foreach (Transform child in trans)
        {
            BoxCollider boxCollider = child.gameObject.GetComponent<BoxCollider>();
            Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(dir * 10, ForceMode.Impulse);
            }
        }
        Destroy(gameObject, 5);
    }

    private void Explode()
    {
        EnemyAudioSource.clip = Sounds[3];
        EnemyAudioSource.Play();
        GameObject explosion = Instantiate(Explosion, transform.position, Quaternion.identity);
        Destroy(explosion, 3);
        StartCoroutine(Explode(transform.position));
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            int damage = Mathf.FloorToInt(Damage * (1f / distance));
            if (col.gameObject.GetComponent<Enemy>() != null)
            {
                col.gameObject.GetComponent<Enemy>().TakeDamage(transform.forward, null, damage);
            }
            else if (col.gameObject.GetComponent<Player>() != null)
            {
                col.gameObject.GetComponent<Player>().TakeDamage(damage);
            }
        }
    }

    private IEnumerator Explode(Vector3 position)
    {
        for (int x = -2; x < 2; x++)
        {
            for (int y = -2; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    Vector3 pos = new Vector3(position.x + x, position.y - 1f + y, position.z + z);
                    _world.GetChunkFromVector3(pos).EditVoxel(pos, Blocks.Empty);
                }
                yield return 0;
            }
        }
    }
}

public enum EnemyType
{
    None = 0,
    Zombie = 1,
    Creeper = 2,
    FlyingCreeper = 3
}
