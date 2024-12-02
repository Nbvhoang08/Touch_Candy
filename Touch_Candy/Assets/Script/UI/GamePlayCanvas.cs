using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayCanvas : UICanvas
{
    // Start is called before the first frame update
    [SerializeField] private Text _levelText;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Transform _targetContainerPanel;
    [SerializeField] private GameObject _targetPrefab;

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
 

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name);

    }


    public void PauseBtn()
    {
        UIManager.Instance.OpenUI<PauseCanvas>();
        Time.timeScale = 0;
    }

    private void Update()
    {
        UpdateLevelText();
        if (_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager>();
        }
        UpdateTargetKPIProgress();

        // Kiểm tra điều kiện hoàn thành
        //if (CheckTargetCompletion())
        //{
        //    // Xử lý khi tất cả target đều hoàn thành
        //    // Ví dụ: Chuyển màn, hiển thị thông báo chiến thắng,...
        //    HandleLevelCompletion();
        //}

    }

    public void UpdateTargetKPIProgress()
    {
        // Kiểm tra GameManager
        if (_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager>();
            if (_gameManager == null)
            {
                Debug.LogWarning("GameManager không tồn tại!");
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
                // Cập nhật text KPI
                kpiText.text = $"{currentTargetType.CurrentKPI}/{currentTargetType.TargetKPI}";
            }

            // Tìm thanh progress (giả sử có Image làm progress bar)
            Image progressBar = targetUIElement.transform.Find("ProgressBar")?.GetComponent<Image>();
            if (progressBar != null)
            {
                // Tính toán và cập nhật tiến độ
                float progressPercentage = (float)currentTargetType.CurrentKPI / currentTargetType.TargetKPI;
                progressBar.fillAmount = Mathf.Clamp01(progressPercentage);
            }
        }
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
                kpiText.text = targetType.CurrentKPI.ToString();
            }
        }
    }

    // Phương thức cập nhật KPI động
    public void UpdateTargetKPI()
    {
        SpawnTargetUI();
    }




}
[System.Serializable]
public class TargetType
{
    public type TypeName;         // Tên loại target
    public Sprite TargetSprite;     // Hình ảnh đại diện
    public int CurrentKPI;          // Số KPI hiện tại
    public int TargetKPI;           // Mục tiêu KPI
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
