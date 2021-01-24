using UnityEngine;
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

    public void LoadChannel()
    {
        Application.OpenURL("https://www.youtube.com/TylerGreen");
    }
}
