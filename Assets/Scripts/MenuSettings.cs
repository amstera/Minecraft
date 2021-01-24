using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSettings : MonoBehaviour
{
    public Toggle Music;
    public Toggle Invincibility;
    public Toggle NoMobs;
    public InputField Seed;

    void Start()
    {
        Music.isOn = Menu.Instance.MusicOn;
        Invincibility.isOn = Menu.Instance.IsInvisibility;
        NoMobs.isOn = Menu.Instance.NoMobs;
        Seed.text = Menu.Instance.Seed.ToString();
    }

    public void PlayGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    public void ChangeMusic(bool isOn)
    {
        Menu.Instance.MusicOn = isOn;
    }

    public void IsInvincible(bool isOn)
    {
        Menu.Instance.IsInvisibility = isOn;
    }

    public void IsNoMobs(bool isOn)
    {
        Menu.Instance.NoMobs = isOn;
    }

    public void ChangeSeed(string seed)
    {
        if (int.TryParse(seed, out int result))
        {
            Menu.Instance.Seed = result;
        }
    }

    public void LoadChannel()
    {
        Application.OpenURL("https://www.youtube.com/TylerGreen");
    }
}
