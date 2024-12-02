using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeCanvas : UICanvas
{
    public Sprite OnVolume;
    public Sprite OffVolume;

    [SerializeField] private Image buttonImage;

    void Start()
    {

        UpdateButtonImage();

    }
    public void playBtn()
    {
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<ChooseLevelCanvas>();
        //SoundManager.Instance.PlayVFXSound(2);

    }

    public void SoundBtn()
    {
        SoundManager.Instance.TurnOn = !SoundManager.Instance.TurnOn;
        UpdateButtonImage();
        //SoundManager.Instance.PlayVFXSound(2);
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
