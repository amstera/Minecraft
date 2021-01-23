using UnityEngine;

public class SwayMovement : MonoBehaviour
{
    public float Amount;
    public float MaxAmount;
    public float SmoothAmount;
    public string AnimationName = "Using";
    public Blocks Type;
    public AudioClip AudioClip;

    private Vector3 _initialPosition;
    private Animator[] _animators;
    private Player _player;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
    }

    void OnEnable()
    {
        _initialPosition = transform.localPosition;
        _animators = GetComponentsInParent<Animator>();
        foreach (Animator animator in _animators)
        {
            animator.enabled = false;
        }
    }

    void Update()
    {
        if (_player.IsDead)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (Type == Blocks.Gun)
            {
                Transform muzzleFlash = transform.Find("MuzzleFlash");
                AudioSource.PlayClipAtPoint(AudioClip, transform.position);
                muzzleFlash.gameObject.SetActive(true);
                Invoke("DisableMuzzleFlash", 0.2f);
            }
            else if (Type == Blocks.Stopwatch)
            {
                AudioSource.PlayClipAtPoint(AudioClip, transform.position);
            }
        }

        float movementX = Input.GetAxis("Mouse X") * Amount;
        float movementY = Input.GetAxis("Mouse Y") * Amount;
        movementX = Mathf.Clamp(movementX, -MaxAmount, MaxAmount);
        movementY = Mathf.Clamp(movementY, -MaxAmount, MaxAmount);

        Vector3 finalPos = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + _initialPosition, Time.deltaTime * SmoothAmount);

        if (_animators.Length > 0 && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (!_animators[0].enabled)
            {
                _animators[0].enabled = true;
            }
            _animators[0].Play(AnimationName, -1, 0f);
        }
    }

    private void DisableMuzzleFlash()
    {
        Transform muzzleFlash = transform.Find("MuzzleFlash");
        muzzleFlash.gameObject.SetActive(false);
    }
}
