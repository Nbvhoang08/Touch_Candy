using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy : MonoBehaviour
{
    // Start is called before the first frame update
    public type candyType;
    Vector3[] raycastDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
    [SerializeField] private List<Candy> adjacentCandies = new List<Candy>();
    [SerializeField] public bool CanCheck;
    public CandyBound bound;
    public bool isDespawn;
    // Update is called once per frame
    private void Start()
    {
       isDespawn = false;
    }
    void Update()
    {
        CheckAndDeactivateAdjacentCandies();
        if (isDespawn)
        {
            StartCoroutine(ShrinkAndDeactivate(this.GetComponent<Candy>()));
        }
        if (bound._done)
        {
            CanCheck = true;
        }
        Mathf.Clamp(transform.localScale.x, 0, 1);
        Mathf.Clamp(transform.localScale.y, 0, 1);
        /*if (transform.localScale.x <=0.01 || transform.localScale.y <= 0.01) 
        { 
            gameObject.SetActive(false);
        }*/
    }
    void CheckAndDeactivateAdjacentCandies()
    {

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        float rayDistance = 1.1f; // Điều chỉnh khoảng cách tùy theo kích thước của object

        // Tạo LayerMask để chỉ kiểm tra lớp "Candy"
        int candyLayer = LayerMask.NameToLayer("Candy");
        int layerMask = 1 << candyLayer;

        // Mảng để lưu kết quả raycast
        RaycastHit2D[] hits = new RaycastHit2D[10];

        foreach (Vector3 direction in directions)
        {
            Vector3 scaledDirection = direction;
            if (direction == Vector3.left || direction == Vector3.right)
            {
                scaledDirection *= transform.localScale.x;
            }
            else if (direction == Vector3.up || direction == Vector3.down)
            {
                scaledDirection *= transform.localScale.y;
            }

            int hitCount = Physics2D.RaycastNonAlloc(transform.position, scaledDirection, hits, rayDistance, layerMask);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider != null && hit.collider.gameObject != this.gameObject)
                {
                   
                    Candy hitCandy = hit.collider.GetComponent<Candy>();
                    if (hitCandy != null && hitCandy.candyType == this.candyType && hitCandy != this.GetComponent<Candy>() &&CanCheck && hitCandy.CanCheck && !adjacentCandies.Contains(hitCandy) )
                    {
                        adjacentCandies.Add(hitCandy);
                    }
                }
            }
        }

        if (adjacentCandies.Count > 0)
        {
            // Thêm chính object này vào danh sách
            // Thêm chính object này vào danh sách
            adjacentCandies.Add(this);
            foreach (Candy candy in adjacentCandies)
            {
                candy.isDespawn = true;
            }
        }
    }

    IEnumerator ShrinkAndDeactivate(Candy candy)
    {
        isDespawn = false;
        float duration = 0.5f; // Thời gian thu nhỏ
        Vector3 originalScale = candy.transform.localScale;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            candy.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        candy.transform.localScale = Vector3.zero;
        candy.gameObject.SetActive(false);
        candy.bound.HandleDisabledChild();
        Debug.Log(this.name);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Vector3 direction in raycastDirections)
        {
            Vector3 scaledDirection = direction;
            if (direction == Vector3.left || direction == Vector3.right)
            {
                scaledDirection *= transform.localScale.x;
            }
            else if (direction == Vector3.up || direction == Vector3.down)
            {
                scaledDirection *= transform.localScale.y;
            }

            Gizmos.DrawRay(transform.position, scaledDirection * 1.1f);
        }

        Gizmos.color = Color.red;
        foreach (Candy candy in adjacentCandies)
        {
            Gizmos.DrawWireCube(candy.transform.position, new Vector3(1, 1, 1));
        }
    }
}
