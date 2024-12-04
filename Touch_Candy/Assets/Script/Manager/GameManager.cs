using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Danh sách các loại target có sẵn
    public List<TargetType> AvailableTargetTypes = new List<TargetType>();
    public List<GameObject> CandyBound = new List<GameObject>();
    public List<Cell> cell = new List<Cell>();  
    public Transform SpawmPos1, SpawPos2;
    public bool IsDestroy;
    public GameObject Harmer;
    public bool ReSpawn;
    public bool gameOver = false;  // Biến gameOver
    private bool winCanvasShown = false;  // Kiểm soát hiển thị WinCanvas
    private bool loseCanvasShown = false; // Kiểm soát hiển thị LoseCanvas
    private float gameOverTimer = 0f;     // Bộ đếm thời gian cho gameOver
    private bool isCheckingGameOver = false; // Cờ để kiểm soát việc đếm thời gian



    private void Start()
    {
        SpawnBound(SpawmPos1.position);
        SpawnBound(SpawPos2.position);
        IsDestroy = false;
    }

    private void Update()
    {
        CheckGameOver();
        if (HasWon && !winCanvasShown) // Nếu đã thắng và WinCanvas chưa hiển thị
        {
            //ShowWinCanvas();
            StartCoroutine(Win());
        }
        else if (gameOver) // Nếu gameOver xảy ra
        {
            if (!isCheckingGameOver) // Bắt đầu đếm thời gian chỉ một lần
            {
                isCheckingGameOver = true;
                gameOverTimer = 1f; // Đặt thời gian chờ là 1 giây
            }

            gameOverTimer -= Time.deltaTime;

            if (gameOverTimer <= 0f) // Khi thời gian chờ kết thúc
            {
                if (!HasWon && !loseCanvasShown) // Nếu chưa thắng, hiện LoseCanvas
                {
                    ShowLoseCanvas();
                }
                else if (HasWon && !winCanvasShown) // Nếu thắng, hiện lại WinCanvas
                {
                    //ShowWinCanvas();
                    StartCoroutine(Win());
                }
            }
        }
        else if (!gameOver && isCheckingGameOver) // Nếu trong vòng 1 giây, gameOver quay lại false
        {
            ResetGameOverCheck();
        }
    }
    public bool HasWon
    {
        get
        {
            return AvailableTargetTypes.All(target => target.isDone);
        }
    }
    IEnumerator Win()
    {
        yield return new WaitForSeconds(0.8f);
        ShowWinCanvas();
    }
    private void ShowWinCanvas()
    {
        winCanvasShown = true;
        UIManager.Instance.OpenUI<WinCanvas>();
        Time.timeScale = 0;
    }

    private void ShowLoseCanvas()
    {
        loseCanvasShown = true;
        UIManager.Instance.OpenUI<LoseCanvas>();
        Time.timeScale = 0;
    }

    private void ResetGameOverCheck()
    {
        isCheckingGameOver = false;
        gameOverTimer = 0f;
    }
    void CheckGameOver()
    {
        // Kiểm tra tất cả các Cell trong danh sách
        gameOver = true;  // Mặc định là true

        foreach (Cell c in cell)
        {
            if (!c.filled)  // Nếu có bất kỳ Cell nào không được filled
            {
                gameOver = false;  // Đặt gameOver là false
                break;  // Dừng kiểm tra nếu đã tìm thấy Cell không filled
            }
        }
    }
    public void OnDestroyBound()
    {
        if (ReSpawn) return;
        IsDestroy = true;
    }
    public void SpawnHarmer(Transform target)
    {
        Vector3 spawnPos = target.position;
        spawnPos.y -= 0.5f;
        spawnPos.x -= 0.5f;  
        Instantiate(Harmer, spawnPos, Quaternion.identity);
        SoundManager.Instance.PlayVFXSound(3);
        IsDestroy=false;
    } 
    public void RespawnCandy()
    {
        if (IsDestroy) return;
       ReSpawn = true;
        
    }
  




    public void SpawnBound(Vector3 SpawnPos)
    {
        int randomIndex = Random.Range(0,4);
        Instantiate(CandyBound[randomIndex], SpawnPos, Quaternion.identity);
    }



    // Phương thức cập nhật KPI cho một loại target cụ thể
    public void UpdateTargetKPI(type typeName, int amount)
    {
        TargetType targetType = AvailableTargetTypes.Find(t => t.TypeName == typeName);
        if (targetType != null)
        {
            targetType.CurrentKPI -= amount;
        }
    }
}
