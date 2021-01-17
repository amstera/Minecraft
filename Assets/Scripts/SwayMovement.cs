using UnityEngine;

public class SwayMovement : MonoBehaviour
{
    public float Amount;
    public float MaxAmount;
    public float SmoothAmount;

    private Vector3 _initialPosition;
    private Animator _animator;

    void Start()
    {
        _initialPosition = transform.localPosition;
        _animator = GetComponentInParent<Animator>();
        if (_animator != null)
        {
            _animator.enabled = false;
        }
    }

    void Update()
    {
        float movementX = Input.GetAxis("Mouse X") * Amount;
        float movementY = Input.GetAxis("Mouse Y") * Amount;
        movementX = Mathf.Clamp(movementX, -MaxAmount, MaxAmount);
        movementY = Mathf.Clamp(movementY, -MaxAmount, MaxAmount);

        Vector3 finalPos = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + _initialPosition, Time.deltaTime * SmoothAmount);

        if (_animator != null && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (!_animator.enabled)
            {
                _animator.enabled = true;
            }
            _animator.Play("Using", -1, 0f);
        }
    }
}