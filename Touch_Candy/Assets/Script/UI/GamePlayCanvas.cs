using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayCanvas : UICanvas
{
    // Start is called before the first frame update
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _cointText;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Transform _targetContainerPanel;
    [SerializeField] private GameObject _targetPrefab;
    [SerializeField] private Animator _animHarmer;
    [SerializeField] private Animator _amimRenew; 
    private void Awake()
    {
        if (_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager>();
        }
    }
    private void Start()
    {
        SpawnTargetUI();
    }
    private void Update()
    {
        UpdateLevelText();
        if (_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager>();
            SpawnTargetUI();
            Debug.Log("Spawn Lai");

        }
        UpdateTargetKPIProgress();
        _animHarmer.SetBool("choosen", _gameManager.IsDestroy);
        _amimRenew.SetBool("choosen", _gameManager.ReSpawn);
        if(_cointText != null)
        {
            _cointText.text = CoinManager.Instance.GetCoinCount().ToString();
        }
       

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name);

    }

    public void HarmerBtn()
    {
        if(CoinManager.Instance.GetCoinCount() >=500)
        {
            _gameManager.OnDestroyBound();
            CoinManager.Instance.SubtractCoins(500);
        }
      
        
    }

    public void RevertBtn()
    {
        if (CoinManager.Instance.GetCoinCount() >= 100)
        {
            _gameManager.RespawnCandy();
            CoinManager.Instance.SubtractCoins(100);

        }
  
       
    }


    public void PauseBtn()
    {
        UIManager.Instance.OpenUI<PauseCanvas>();
        SoundManager.Instance.PlayClickSound();
        Time.timeScale = 0;
    }



  
    public bool CheckTargetCompletion()
    {
        bool allTargetsCompleted = true;

        foreach (TargetType targetType in _gameManager.AvailableTargetTypes)
        {
            if (targetType.CurrentKPI < targetType.TargetKPI)
            {
                allTargetsCompleted = false;
                break;
            }
        }

        return allTargetsCompleted;
    }


    private void UpdateLevelText()
    {
        if (_levelText != null)
        {
            _levelText.text = "Level " +  SceneManager.GetActiveScene().name;
        }
    }

    public void SpawnTargetUI()
    {
        // Xóa các target UI hiện tại
        if (_targetContainerPanel != null)
        {
            foreach (Transform child in _targetContainerPanel)
            {
                Destroy(child.gameObject);
            }
        }

        // Kiểm tra GameManager
        if (_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager>();
           
            
            
        }

        // Lấy danh sách target types từ GameManager
        foreach (TargetType targetType in _gameManager.AvailableTargetTypes)
        {
            // Tạo phần tử UI cho mỗi loại target
            GameObject targetUIElement = Instantiate(_targetPrefab, _targetContainerPanel);

            // Tìm và set hình ảnh
            Image flagImage = targetUIElement.GetComponentInChildren<Image>();
            if (flagImage != null)
            {
                flagImage.sprite = targetType.TargetSprite;
            }

            // Tìm và set text KPI
            Text kpiText = targetUIElement.GetComponentInChildren<Text>();
            if (kpiText != null)
            {
                if (targetType.CurrentKPI > 0)
                {
                    kpiText.text = targetType.CurrentKPI.ToString();
                }
                else
                {
                    kpiText.text = "Done!";
                }
               
            }
        }
    }
    public void UpdateTargetKPIProgress()
    {
        // Kiểm tra GameManager
        if (_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager>();
            if (_gameManager == null)
            {
                
                return;
               
            }
        }

        // Kiểm tra xem có target UI hiện tại không
        if (_targetContainerPanel == null)
        {
            Debug.LogWarning("Target Container Panel chưa được thiết lập!");
            return;
        }

        // Lặp qua các phần tử target UI hiện có
        for (int i = 0; i < _targetContainerPanel.childCount; i++)
        {
            // Kiểm tra nếu vượt quá số lượng target types
            if (i >= _gameManager.AvailableTargetTypes.Count)
            {
                break;
            }

            // Lấy phần tử UI và target type tương ứng
            GameObject targetUIElement = _targetContainerPanel.GetChild(i).gameObject;
            TargetType currentTargetType = _gameManager.AvailableTargetTypes[i];

            // Tìm text KPI
            Text kpiText = targetUIElement.GetComponentInChildren<Text>();
            if (kpiText != null)
            {


                if (currentTargetType.CurrentKPI > 0)
                {
                    // Cập nhật text KPI
                    kpiText.text = $"{currentTargetType.CurrentKPI}/{currentTargetType.TargetKPI}";
                }
                else
                {
                    kpiText.text = $"DONE!";
                }




            }


        }
    }






}
[System.Serializable]
public class TargetType
{
    public type TypeName;         // Tên loại target
    public Sprite TargetSprite;     // Hình ảnh đại diện
    public int CurrentKPI;          // Số KPI hiện tại
    public int TargetKPI;           // Mục tiêu KPI
    public bool isDone => CurrentKPI <= 0 ;
    void Start()
    {
        CurrentKPI = TargetKPI;
    }
}
public enum type
{
    blue,
    green,
    red,
    yellow,
    brown,
    violet,
    pink

}
