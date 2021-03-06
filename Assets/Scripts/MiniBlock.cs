using UnityEngine;

public class MiniBlock : MonoBehaviour
{
    public Blocks Block;
    public bool IsRotating = true;

    void Start()
    {
        Destroy(gameObject, 8);
    }

    void Update()
    {
        if (IsRotating)
        {
            transform.Rotate(Vector3.up, 100 * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            Destroy(gameObject);
            Inventory.Instance.Add(Block);
        }
    }
}
