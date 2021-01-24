using UnityEngine;

public class Menu : MonoBehaviour
{
    public static Menu Instance;

    public bool MusicOn;
    public bool IsInvisibility;
    public bool NoMobs;
    public int Seed;

    void Awake()
    {
        DontDestroyOnLoad(this);
        if (Instance == null)
        {
            MusicOn = true;
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
