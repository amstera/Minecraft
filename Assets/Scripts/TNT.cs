using System.Collections;
using UnityEngine;

public class TNT : MonoBehaviour
{
    public Material Tnt;
    public Material White;
    public GameObject Explosion;
    public AudioSource ExplodeAudioSource;

    public int Damage = 20;

    private MeshRenderer _meshRenderer;
    private World _world;
    private float _startTime;
    private bool _isExploded;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _world = FindObjectOfType<World>();
        _startTime = Time.time;
        Invoke("ChangeToWhite", 0.1f);
    }

    void Update()
    {
        if (_isExploded)
        {
            return;
        }
        if (Time.time - _startTime >= 3.5f)
        {
            Explode();
        }
    }

    private void ChangeToWhite()
    {
        _meshRenderer.material = White;
        Invoke("ChangeToTnt", 0.1f);
    }

    private void ChangeToTnt()
    {
        _meshRenderer.material = Tnt;
        Invoke("ChangeToWhite", 0.1f);
    }

    private void Explode()
    {
        _isExploded = true;

        transform.localScale = Vector3.zero;
        ExplodeAudioSource.Play();
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

        Destroy(gameObject);
    }
}
