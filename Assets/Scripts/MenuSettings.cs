using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSettings : MonoBehaviour
{
    public Toggle Music;
    public Toggle Invincibility;
    public Toggle NoMobs;
    public InputField Seed;
    public AudioSource MusicAudioSource;

    void Start()
    {
        Music.isOn = Menu.Instance.MusicOn;
        Invincibility.isOn = Menu.Instance.IsInvisibility;
        NoMobs.isOn = Menu.Instance.NoMobs;
        Seed.text = Menu.Instance.Seed.ToString();

        ChangeMenuMusic();
    }

    public void PlayGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    public void ChangeMusic(bool isOn)
    {
        Menu.Instance.MusicOn = isOn;
        ChangeMenuMusic();
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

    private void ChangeMenuMusic()
    {
        if (Menu.Instance.MusicOn)
        {
            MusicAudioSource.Play();
        }
        else
        {
            MusicAudioSource.Stop();
        }
    }
}
