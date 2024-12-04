using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseCanvas : UICanvas
{
    public Sprite OnVolume;
    public Sprite OffVolume;

    [SerializeField] private Image buttonImage;
    void Start()
    {

        UpdateButtonImage();

    }
    public void RetryBtn()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        UIManager.Instance.CloseUI<LoseCanvas>(0.1f);

        SoundManager.Instance.PlayClickSound();


    }

    public void HomeBtn()
    {
        UIManager.Instance.CloseAll();
        Time.timeScale = 1;
        SceneManager.LoadScene("Home");
        UIManager.Instance.OpenUI<HomeCanvas>();

        SoundManager.Instance.PlayClickSound();

    }
    public void SoundBtn()
    {
        SoundManager.Instance.TurnOn = !SoundManager.Instance.TurnOn;
        UpdateButtonImage();

        SoundManager.Instance.PlayClickSound();
    }

    private void UpdateButtonImage()
    {
        if (SoundManager.Instance.TurnOn)
        {
            buttonImage.sprite = OnVolume;
        }
        else
        {
            buttonImage.sprite = OffVolume;
        }
    }
}
