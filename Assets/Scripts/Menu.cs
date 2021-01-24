using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ChangeMusic(bool isOn)
    {
        MusicOn = isOn;
    }

    public void IsInvincible(bool isOn)
    {
        IsInvisibility = isOn;
    }

    public void IsNoMobs(bool isOn)
    {
        NoMobs = isOn;
    }

    public void ChangeSeed(string seed)
    {
        if (int.TryParse(seed, out int result))
        {
            Seed = result;
        }
    }
}
