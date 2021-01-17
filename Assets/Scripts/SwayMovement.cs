using UnityEngine;

public class SwayMovement : MonoBehaviour
{
    public float Amount;
    public float MaxAmount;
    public float SmoothAmount;

    private Vector3 _initialPosition;

    void Start()
    {
        _initialPosition = transform.localPosition;
    }

    void Update()
    {
        float movementX = Input.GetAxis("Mouse X") * Amount;
        float movementY = Input.GetAxis("Mouse Y") * Amount;
        movementX = Mathf.Clamp(movementX, -MaxAmount, MaxAmount);
        movementY = Mathf.Clamp(movementY, -MaxAmount, MaxAmount);

        Vector3 finalPos = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + _initialPosition, Time.deltaTime * SmoothAmount);
    }
}
