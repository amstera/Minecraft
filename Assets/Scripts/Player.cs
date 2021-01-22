using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool IsGrounded;
    public bool IsSprinting;
    public bool IsDead;

    private World _world;
    private Transform _cam;
    public Transform HighlightBlock;
    public Transform PlaceBlock;
    public GameObject Hearts;
    public GameObject DeathScreen;

    public Texture FullHeart;
    public Texture HalfHeart;
    public Texture EmptyHeart;

    public List<AudioClip> BlockSounds;
    public AudioClip DeathSound;
    public AudioSource BlockAudioSource;
    public AudioSource PainAudioSource;

    public float WalkSpeed = 3f;
    public float SprintSpeed = 6f;
    public float JumpForce = 5f;
    public float Gravity = -9.81f;

    public float PlayerWidth = 0.15f;
    public float CheckIncrement = 0.075f;
    public float Reach = 8;

    public int Health = 20;

    private float _horizontal;
    private float _vertical;
    private Vector3 _velocity;
    private float _verticalMomentum;
    private bool _jumpRequest;
    private float _lastDamageTime;
    private float _lastHealthRegenerationTime;

    void Start()
    {
        _world = GameObject.Find("World").GetComponent<World>();
        _cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
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

        transform.Translate(_velocity, Space.World);
        DrawHealth();
    }

    void Update()
    {
        if (IsDead)
        {
            return;
        }

        GetPlayerInputs();
        PlaceCursorBlock();

        if (Health < 20 && Health > 0 && Time.time - _lastHealthRegenerationTime >= 4)
        {
            Health += 1;
            _lastHealthRegenerationTime = Time.time;
            DrawHealth();
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        if (Time.time - _lastDamageTime > 1)
        {
            PainAudioSource.Play();
            _jumpRequest = true;
            Health -= damage;
            Health = Mathf.Clamp(Health, 0, 20);
            if (Health <= 0)
            {
                Die();
            }

            _lastDamageTime = Time.time;
            _lastHealthRegenerationTime = Time.time;
            DrawHealth();
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
        // Affect vertical momentum with gravity.
        if (_verticalMomentum > Gravity)
        {
            _verticalMomentum += Time.fixedDeltaTime * Gravity;
        }

        float speedMultiplier = IsSprinting ? SprintSpeed : WalkSpeed;
        _velocity = ((transform.forward * _vertical) + (transform.right * _horizontal)) * Time.fixedDeltaTime * speedMultiplier;

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

    private void GetPlayerInputs()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            IsSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            IsSprinting = false;
        }

        if (IsGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _jumpRequest = true;
        }

        //Destroy block
        if (Input.GetMouseButtonDown(0))
        {
            Blocks selectedBlock = Inventory.Instance.GetSelectedBlock();
            if (selectedBlock == Blocks.Sword)
            {
                SwingSword();
            }
            else if (selectedBlock == Blocks.Gun)
            {
                ShootGun();
            }

            if (HighlightBlock.gameObject.activeSelf)
            {
                Chunk chunk = _world.GetChunkFromVector3(HighlightBlock.position);
                Blocks block = chunk.GetBlockFromGlobalVector3(HighlightBlock.position);
                if (block != Blocks.Empty && block != Blocks.Bedrock)
                {
                    PlayBlockSound(block);
                    chunk.EditVoxel(HighlightBlock.position, Blocks.Empty);
                    _world.GenerateBlock(block, HighlightBlock.position);
                }
            }
        }

        //Create block
        if (Input.GetMouseButtonDown(1))
        {
            Blocks selectedBlock = Inventory.Instance.GetSelectedBlock();
            if (selectedBlock > Blocks.Empty && selectedBlock != Blocks.Diamond && HighlightBlock.gameObject.activeSelf)
            {
                PlayBlockSound(selectedBlock);
                _world.GetChunkFromVector3(PlaceBlock.position).EditVoxel(PlaceBlock.position, selectedBlock);
                Inventory.Instance.Remove(selectedBlock);
            }
            else if (selectedBlock == Blocks.Sword)
            {
                SwingSword();
            }
            else if (selectedBlock == Blocks.Gun)
            {
                ShootGun();
            }
        }
    }

    private void PlaceCursorBlock()
    {
        float step = CheckIncrement;
        Vector3 lastPos = new Vector3();

        while (step < Reach)
        {
            Vector3 pos = _cam.position + (_cam.forward * step);
            if (_world.CheckForVoxel(pos))
            {
                HighlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                PlaceBlock.position = lastPos;

                HighlightBlock.gameObject.SetActive(true);
                PlaceBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += CheckIncrement;
        }

        HighlightBlock.gameObject.SetActive(false);
        PlaceBlock.gameObject.SetActive(false);
    }

    private void DrawHealth()
    {
        int i = 1;
        foreach (Transform child in Hearts.transform)
        {
            if (Health >= (i * 2))
            {
                child.GetComponent<RawImage>().texture = FullHeart;
            }
            else if (Health <= (i - 1) * 2)
            {
                child.GetComponent<RawImage>().texture = EmptyHeart;
            }
            else
            {
                child.GetComponent<RawImage>().texture = HalfHeart;
            }

            i++;
        }
    }

    private void SwingSword()
    {
        int layerMask = LayerMask.GetMask("Enemy");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 4, layerMask))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_cam.transform.forward, hit.point, 7, 8);
            }
        }
    }

    private void ShootGun()
    {
        int layerMask = LayerMask.GetMask("Enemy");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_cam.transform.forward, hit.point, 15, 10);
            }
        }
    }

    private void PlayBlockSound(Blocks block)
    {
        BlockAudioSource.clip = BlockSounds[(int)block - 2];
        BlockAudioSource.Play();
    }

    private void Die()
    {
        IsDead = true;
        PainAudioSource.clip = DeathSound;
        PainAudioSource.Play();
        Inventory.Instance.EmptyInventory();

        _cam.Rotate(new Vector3(0, 90, 0));
        DeathScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (
            _world.CheckForVoxel(transform.position.x - PlayerWidth, transform.position.y + downSpeed, transform.position.z - PlayerWidth) ||
            _world.CheckForVoxel(transform.position.x + PlayerWidth, transform.position.y + downSpeed, transform.position.z - PlayerWidth) ||
            _world.CheckForVoxel(transform.position.x + PlayerWidth, transform.position.y + downSpeed, transform.position.z + PlayerWidth) ||
            _world.CheckForVoxel(transform.position.x - PlayerWidth, transform.position.y + downSpeed, transform.position.z + PlayerWidth)
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
            _world.CheckForVoxel(transform.position.x - PlayerWidth, transform.position.y + 2f + upSpeed, transform.position.z - PlayerWidth) ||
            _world.CheckForVoxel(transform.position.x + PlayerWidth, transform.position.y + 2f + upSpeed, transform.position.z - PlayerWidth) ||
            _world.CheckForVoxel(transform.position.x + PlayerWidth, transform.position.y + 2f + upSpeed, transform.position.z + PlayerWidth) ||
            _world.CheckForVoxel(transform.position.x - PlayerWidth, transform.position.y + 2f + upSpeed, transform.position.z + PlayerWidth)
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
                _world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + PlayerWidth) ||
                _world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + PlayerWidth)
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
                _world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - PlayerWidth) ||
                _world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - PlayerWidth)
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
                _world.CheckForVoxel(transform.position.x - PlayerWidth, transform.position.y, transform.position.z) ||
                _world.CheckForVoxel(transform.position.x - PlayerWidth, transform.position.y + 1f, transform.position.z)
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
                _world.CheckForVoxel(transform.position.x + PlayerWidth, transform.position.y, transform.position.z) ||
                _world.CheckForVoxel(transform.position.x + PlayerWidth, transform.position.y + 1f, transform.position.z)
                )
            {
                return true;
            }

            return false;
        }

    }
}