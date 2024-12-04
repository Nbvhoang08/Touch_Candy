using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    // Biến để lưu trạng thái
    public bool filled = false;

    // Bán kính kiểm tra (tùy chỉnh theo kích thước của đối tượng)
    [SerializeField] private float detectionRadius = 0.5f;


    void Start()
    {
        UpdateFilledStatus(); // Cập nhật trạng thái khi bắt đầu
    }

    void Update()
    {
        UpdateFilledStatus(); // Cập nhật trạng thái mỗi frame
    }

    void UpdateFilledStatus()
    {
        // Kiểm tra xem có đối tượng nào trong bán kính không
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        filled = false; // Mặc định không có đối tượng

        foreach (Collider2D collider in colliders)
        {
            // Kiểm tra xem collider có chứa component CandyBound không
            if (collider.GetComponent<CandyBound>() != null)
            {
                filled = true;
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ bán kính kiểm tra để dễ dàng quan sát trong Editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
