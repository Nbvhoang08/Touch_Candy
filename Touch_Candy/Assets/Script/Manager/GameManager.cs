using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Danh sách các loại target có sẵn
    public List<TargetType> AvailableTargetTypes = new List<TargetType>();


    public List<GameObject> CandyBound = new List<GameObject>();
    public Transform SpawmPos1, SpawPos2;


    private void Start()
    {
        SpawnBound(SpawmPos1.position);
        SpawnBound(SpawPos2.position);
    }


    public void SpawnBound(Vector3 SpawnPos)
    {
        int randomIndex = Random.Range(0,4);
        Instantiate(CandyBound[randomIndex], SpawnPos, Quaternion.identity);
    }




    // Phương thức để thêm hoặc cập nhật loại target
    public void SetupTargetType(type typeName, Sprite sprite, int targetKPI)
    {
        // Kiểm tra xem target type đã tồn tại chưa
        TargetType existingType = AvailableTargetTypes.Find(t => t.TypeName == typeName);

        if (existingType == null)
        {
            // Tạo target type mới nếu chưa tồn tại
            existingType = new TargetType
            {
                TypeName = typeName,
                TargetSprite = sprite,
                TargetKPI = targetKPI,
                CurrentKPI = 0
            };
            AvailableTargetTypes.Add(existingType);
        }
        else
        {
            // Cập nhật target type đã tồn tại
            existingType.TargetSprite = sprite;
            existingType.TargetKPI = targetKPI;
        }
    }

    // Phương thức cập nhật KPI cho một loại target cụ thể
    public void UpdateTargetKPI(type typeName, int newKPI)
    {
        TargetType targetType = AvailableTargetTypes.Find(t => t.TypeName == typeName);
        if (targetType != null)
        {
            targetType.CurrentKPI = newKPI;
        }
    }
}
