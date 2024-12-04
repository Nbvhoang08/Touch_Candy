using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : Singleton<CoinManager>
{
    private const string CoinKey = "PlayerCoins"; // Khóa lưu số coin trong PlayerPrefs
    private int _coinCount; // Biến lưu số coin hiện tại trong bộ nhớ

    void Start()
    {
        // Lấy số coin từ PlayerPrefs hoặc gán mặc định là 100
        _coinCount = PlayerPrefs.GetInt(CoinKey, 100);
    }

    /// <summary>
    /// Lấy số coin hiện tại
    /// </summary>
    /// <returns>Số coin hiện tại</returns>
    public int GetCoinCount()
    {
        return _coinCount;
    }
    private void Update()
    {
        _coinCount = PlayerPrefs.GetInt(CoinKey, 100);
    }

    /// <summary>
    /// Cộng thêm số coin
    /// </summary>
    /// <param name="amount">Số coin cần thêm</param>
    public void AddCoins(int amount)
    {
        _coinCount += amount;
        SaveCoins();
    }

    /// <summary>
    /// Trừ số coin
    /// </summary>
    /// <param name="amount">Số coin cần trừ</param>
    /// <returns>Số coin thực tế đã trừ</returns>
    public int SubtractCoins(int amount)
    {
        int coinsSubtracted;

        if (_coinCount >= amount) // Nếu đủ coin để trừ
        {
            _coinCount -= amount;
            coinsSubtracted = amount;
        }
        else // Nếu không đủ coin, trừ hết về 0
        {
            coinsSubtracted = _coinCount; // Trừ hết số coin hiện tại
            _coinCount = 0;
        }

        SaveCoins(); // Lưu số coin mới vào PlayerPrefs
        return coinsSubtracted; // Trả về số coin thực tế đã trừ
    }

    /// <summary>
    /// Lưu số coin hiện tại vào PlayerPrefs
    /// </summary>
    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinKey, _coinCount);
        PlayerPrefs.Save(); // Lưu thay đổi
    }
}
