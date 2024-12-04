using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Net;
using Unity.Mathematics;
public class CandyBound : MonoBehaviour
{
    // Start is called before the first frame update
    public TypeCandy typeCandy;
    [SerializeField] List<GameObject> CandyPool = new List<GameObject>();
    public bool CanMove;
    private Vector3 startPosition;
    private bool isDragging = false;
    public bool _done = false;
    public List<GameObject> CandyChild = new List<GameObject>();
    [SerializeField] private bool isScaling = false;
    private GameManager _gameManager;
    private void Awake()
    {
        if(_gameManager == null)
        {
            _gameManager = FindAnyObjectByType<GameManager>();
        }
        
    }
    void Start()
    {
        startPosition = transform.position;
        ResetCollider();
        InitCandy();

    }
    void ResetCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false; // Tắt collider
            collider.enabled = true;  // Bật lại collider
        }
        else
        {
            Debug.LogWarning("Collider not found on the object!" + this.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameManager == null)
        {
            _gameManager = FindAnyObjectByType<GameManager>();
        }

        // Xử lý kéo thả bằng Input
        if (Input.GetMouseButtonDown(0))
        {
            // Kiểm tra xem chuột có click vào object này không
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
               
                if (_done)
                {
                    if (_gameManager.IsDestroy)
                    {
                        _gameManager.SpawnHarmer(transform);
                        StartCoroutine(DestroyAfterDelay());
                    }
                    if (_gameManager.ReSpawn)
                    {
                        _gameManager.ReSpawn = false;
                        InitCandy();
                    }
                }
                else
                {
                    isDragging = true;
                    SoundManager.Instance.PlayVFXSound(0);
                }
            }
        }

        // Kéo object
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Đảm bảo object không thay đổi trục Z
            transform.position = mousePosition;
        }

        // Thả chuột
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Vector3 snappedPosition = SnapToGrid(transform.position);

            if (!_done)
            {
                if (IsValidCell(snappedPosition))
                {
                    transform.position = snappedPosition;
                    foreach (GameObject candy in CandyChild)
                    {
                        candy.GetComponent<Candy>().CanCheck = true;
                    }
                    _gameManager.SpawnBound(startPosition);
                    _done = true;
                }
                else
                {
                    StartCoroutine(MoveToStartPosition());
                }
            }
        }

        // Các logic còn lại giữ nguyên
        if (CandyChild.All(child => !child.activeSelf))
        {
            StartCoroutine(DestroyAfterDelay());
        }

        // Xử lý logic scale và child
        if (!isScaling && CandyChild.Count(child => child.activeSelf) == 2)
        {
            bool allScalesLessThanOne = CandyChild
                .Where(child => child.activeSelf)
                .Any(child => child.transform.localScale.x < 1 && child.transform.localScale.y < 1);
            if (allScalesLessThanOne)
            {
                HandleDisabledChild();
            }
        }
        else if (!isScaling && CandyChild.Count(child => child.activeSelf) == 1)
        {
            float totalScaleX = CandyChild.Where(child => child.activeSelf)
                               .Sum(child => child.transform.localScale.x);
            float totalScaleY = CandyChild.Where(child => child.activeSelf)
                                        .Sum(child => child.transform.localScale.y);
            if (totalScaleX < 1 || totalScaleY < 1)
            {
                HandleDisabledChild();
            }
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject); // Destroy object chính sau 0.5 giây

    }

    Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / 2) * 2;
        float y = Mathf.Round(position.y / 2) * 2;
        return new Vector3(x, y, position.z);
    }

    bool IsValidCell(Vector3 position)
    {
        // Lưu lớp ban đầu của object
        int originalLayer = gameObject.layer;

        // Đặt lớp của object sang lớp "Ignore Raycast"
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // Tạo LayerMask để bỏ qua lớp "Ignore Raycast" và "Candy"
        int layerMask = ~(LayerMask.GetMask("Ignore Raycast", "Candy"));

        // Kiểm tra va chạm tại vị trí
        Collider2D hitCollider = Physics2D.OverlapPoint(position, layerMask);

        if (hitCollider != null)
        {
            if (hitCollider.CompareTag("Cell"))
            {
                Collider2D[] colliders = Physics2D.OverlapPointAll(position, layerMask);
                foreach (Collider2D collider in colliders)
                {
                    if (collider != hitCollider)
                    {
                        // Khôi phục lớp ban đầu của object trước khi trả về
                        gameObject.layer = originalLayer;
                        return false; // Có object khác ngoài cell tại vị trí này
                    }
                }
                // Khôi phục lớp ban đầu của object trước khi trả về
                gameObject.layer = originalLayer;
                return true; // Chỉ có cell tại vị trí này
            }
        }

        // Khôi phục lớp ban đầu của object trước khi trả về
        gameObject.layer = originalLayer;
        return false; // Không có cell tại vị trí này
       
    }

    System.Collections.IEnumerator MoveToStartPosition()
    {
        float elapsedTime = 0;
        float duration = 0.5f; // Thời gian di chuyển về vị trí ban đầu
        Vector3 initialPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, startPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;
    }


    /// Handle Scale child Object 



    public void HandleDisabledChild()
    {
        if (typeCandy == TypeCandy.One) return;
        if (isScaling) return; // Prevent multiple scaling operations

        // Get the active children
        List<GameObject> activeChildren = CandyChild.Where(child => child.activeSelf).ToList();

        // Handle when only one object remains
        if (activeChildren.Count == 1)
        {
            StartCoroutine(MoveAndScaleToCenter(activeChildren[0]));
         
            return;
        }
        // Handle when only two objects remain
        else if (activeChildren.Count == 2)
        {
            StartCoroutine(ScaleAndRepositionMultiple(activeChildren));
          
            return;
        }
        else
        {
            // Find the disabled child
            GameObject disabledChild = CandyChild.FirstOrDefault(child => !child.activeSelf);
            if (disabledChild == null)
            {
              
                HandleDisabledChild();
                return;
            } 
               

            // Find the best child to scale
            GameObject childToScale = FindBestChildToScale(disabledChild);
            if (childToScale != null)
            {
                StartCoroutine(ScaleAndRepositionChild(disabledChild, childToScale));
              
            }
        } 
    }

    private GameObject FindBestChildToScale(GameObject disabledChild)
    {
        // Get active children
        List<GameObject> activeChildren = CandyChild.Where(child => child.activeSelf).ToList();
       
        // First, find adjacent children
        var adjacentChildren = activeChildren.Where(child =>
            Mathf.Abs(child.transform.localPosition.x - disabledChild.transform.localPosition.x) < 0.5f ||
            Mathf.Abs(child.transform.localPosition.y - disabledChild.transform.localPosition.y) < 0.5f
        ).ToList();

        if (adjacentChildren.Count > 0)
        {
            // If multiple adjacent children, choose the one with smallest scale
            return adjacentChildren.OrderBy(child =>
                Mathf.Min(child.transform.localScale.x, child.transform.localScale.y)
            ).First();
        }

        // If no adjacent children, choose the smallest scaled active child
        if (activeChildren.Count > 0)
        {
            return activeChildren.OrderBy(child =>
                Mathf.Min(child.transform.localScale.x, child.transform.localScale.y)
            ).First();
        }

        // If all else fails, return null
        return null;
    }

    private IEnumerator ScaleAndRepositionChild(GameObject disabledChild, GameObject childToScale)
    {
        isScaling = true;

        // Calculate target scale
        Vector3 originalScale = childToScale.transform.localScale;
        Vector3 targetScale = originalScale;

    
        // Calculate target position (midpoint between the two children)
        Vector3 originalPosition = childToScale.transform.localPosition;
        Vector3 disabledPosition = disabledChild.transform.localPosition;
        Vector3 targetPosition = (originalPosition + disabledPosition) / 2f;
        if(originalPosition.x == disabledPosition.x)
        {
            targetScale.y += disabledChild.transform.localScale.y + childToScale.transform.localScale.y;
        }
        else
        {
            targetScale.x += disabledChild.transform.localScale.x + childToScale.transform.localScale.x;
        }

        // Animation parameters
        float duration = 0.4f;
        float elapsedTime = 0;
        Mathf.Clamp(childToScale.transform.localScale.y, 0, 1);
        Mathf.Clamp(childToScale.transform.localScale.x, 0, 1);
       
        while (elapsedTime < duration)
        {
            // Interpolate scale
            childToScale.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);

            // Interpolate position
            childToScale.transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are set
        childToScale.transform.localScale = targetScale;
        childToScale.transform.localPosition = targetPosition;

        isScaling = false;
    }

  

    // Coroutine to scale and reposition multiple objects
    private IEnumerator ScaleAndRepositionMultiple(List<GameObject> objectsToScale)
    {
        if (objectsToScale == null || objectsToScale.Count < 2)
        {
           
            yield break;
        }

        isScaling = true;


        // Lấy hai đối tượng
        GameObject child1 = objectsToScale[0];
        GameObject child2 = objectsToScale[1];

        // Lấy scale ban đầu của cả hai
        Vector3 scale1 = child1.transform.localScale;
        Vector3 scale2 = child2.transform.localScale;

        // Phân loại đối tượng lớn hơn và nhỏ hơn
        GameObject smallerObject = scale1.magnitude < scale2.magnitude ? child1 : child2;
        GameObject largerObject = smallerObject == child1 ? child2 : child1;

        // Lấy vị trí ban đầu
        Vector3 originalPositionSmaller = smallerObject.transform.localPosition;
        Vector3 originalPositionLarger = largerObject.transform.localPosition;

        // Tính toán target scale và position
        Vector3 targetScaleSmaller = smallerObject.transform.localScale;
        Vector3 targetScaleLarger = largerObject.transform.localScale;
        Vector3 targetPositionSmaller = originalPositionSmaller;
        Vector3 targetPositionLarger = originalPositionLarger;

        if (Mathf.Approximately(scale1.magnitude, scale2.magnitude))
        {
            // Trường hợp kích thước bằng nhau
            targetScaleSmaller *= 2f;
           

            // Tính điểm giữa và di chuyển cả hai đối tượng
            if(originalPositionLarger.x == originalPositionSmaller.x)
            {
                targetPositionSmaller.x = 0;
                targetPositionLarger.x = 0;
                targetScaleSmaller.x *= 1f;
                targetScaleSmaller.y = 0.5f;
                targetScaleLarger = targetScaleSmaller;
                Mathf.Clamp(targetScaleSmaller.x, 0, 1);
               
            }
            else
            {
                targetPositionSmaller.y = 0;
                targetPositionLarger.y = 0;
                targetScaleSmaller.y *= 1f;
                targetScaleSmaller.x = 0.5f;
                Mathf.Clamp(targetScaleSmaller.y, 0, 1);
                targetScaleLarger = targetScaleSmaller;
               
            }
            
        }
        else
        {
            // Trường hợp kích thước khác nhau
            targetScaleSmaller = targetScaleLarger;
            if (targetPositionLarger.x == 0) 
            {
                targetPositionSmaller.x = 0;
             
                 
            }else if(targetPositionLarger.y == 0)
            {
                targetPositionSmaller.y = 0;
  
            }
            // Di chuyển nhỏ hơn đến vị trí lớn hơn
        }

        // Animation parameters
        float duration = 0.4f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Interpolate scale
            smallerObject.transform.localScale = Vector3.Lerp(smallerObject.transform.localScale, targetScaleSmaller, elapsedTime / duration);

            if (Mathf.Approximately(scale1.magnitude, scale2.magnitude))
            {
                // Nếu bằng nhau, scale cả hai
                largerObject.transform.localScale = Vector3.Lerp(largerObject.transform.localScale, targetScaleLarger, elapsedTime / duration);
            }

            // Interpolate position
            smallerObject.transform.localPosition = Vector3.Lerp(smallerObject.transform.localPosition, targetPositionSmaller, elapsedTime / duration);
            largerObject.transform.localPosition = Vector3.Lerp(largerObject.transform.localPosition, targetPositionLarger, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Gán giá trị cuối cùng
        smallerObject.transform.localScale = targetScaleSmaller;
        smallerObject.transform.localPosition = targetPositionSmaller;


        if (Mathf.Approximately(scale1.magnitude, scale2.magnitude))
        {
            largerObject.transform.localScale = targetScaleLarger;
            largerObject.transform.localPosition = targetPositionLarger;
            
        }
       


        isScaling = false;
    }



    private IEnumerator MoveAndScaleToCenter(GameObject child)
    {
        isScaling = true;

        // Target position and scale
        Vector3 targetPosition = Vector3.zero; // Center of the parent (CandyBound)
        Vector3 targetScale = Vector3.one; // Scale to 1x1

        // Current position and scale
        Vector3 originalPosition = child.transform.localPosition;
        Vector3 originalScale = child.transform.localScale;

        // Animation parameters
        float duration = 0.4f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Interpolate position and scale
            child.transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / duration);
            child.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Mathf.Clamp(child.transform.localScale.y, 0, 1);
        Mathf.Clamp(child.transform.localScale.x, 0, 1);
        // Ensure final values are set
        child.transform.localPosition = targetPosition;
        child.transform.localScale = targetScale;

        isScaling = false;
    }






    /// <summary>
    //  Handle Spawn Candy
    /// </summary>
    public void InitCandy()
    {
        // Clear any existing children first
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        CandyChild = new List<GameObject>();    
        // Ensure we have enough objects in the CandyPool
        if (CandyPool.Count < 4)
        {
            return;
        }
        //CandyChild = new List<GameObject>();
        switch (typeCandy)
        {
            case TypeCandy.One:
                SpawnSingleChild();
                break;
            case TypeCandy.Two:
                SpawnTwoChildren();
                break;
            case TypeCandy.Three:
                SpawnThreeChildren();
                break;
            case TypeCandy.Four:
                SpawnFourChildren();
                break;
        }
    }

    private void SpawnSingleChild()
    {
        int RandomIndex = UnityEngine.Random.Range(0,CandyPool.Count);
        GameObject child = Instantiate(CandyPool[RandomIndex], transform);
        child.transform.localScale = Vector3.one;
        child.GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        child.transform.localPosition = new Vector3(0, 0, 2);
        CandyChild.Add(child);
    }

    private void SpawnTwoChildren()
    {
        // Randomly choose between horizontal and vertical layout
        bool isHorizontal = UnityEngine.Random.value > 0.5f;
       
        // Get unique random indices from CandyPool
        List<int> availableIndices = Enumerable.Range(0, CandyPool.Count).ToList();
        int index1 = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(index1);
        int index2 = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];

        GameObject child1 = Instantiate(CandyPool[index1], transform);
        GameObject child2 = Instantiate(CandyPool[index2], transform);
        CandyChild.Add(child1);
        CandyChild.Add(child2);
        child1.GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        child2.GetComponent<Candy>().bound = this.GetComponent<CandyBound>();

        if (isHorizontal)
        {
            // Horizontal layout
            child1.transform.localPosition = new Vector3(0, 0.25f, 2);
            child2.transform.localPosition = new Vector3(0, -0.25f, 2);
            child1.transform.localScale = new Vector3(1f, 0.5f, 1f);
            child2.transform.localScale = new Vector3(1f, 0.5f, 1f);
            
        }
        else
        {
            // Vertical layout
            child1.transform.localPosition = new Vector3(0.25f, 0, 2);
            child2.transform.localPosition = new Vector3(-0.25f,0, 2);
            child1.transform.localScale = new Vector3(0.5f, 1f, 1f);
            child2.transform.localScale = new Vector3(0.5f, 1f, 1f);
           
        }

       
    }

    private void SpawnThreeChildren()
    {
       
        // Get unique random indices from CandyPool
        List<int> availableIndices = Enumerable.Range(0, CandyPool.Count).ToList();
        // Randomly select 3 unique objects
        int index1 = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(index1);
        int index2 = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(index2);
        int index3 = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];

        GameObject child1 = Instantiate(CandyPool[index1], transform);
        GameObject child2 = Instantiate(CandyPool[index2], transform);
        GameObject child3 = Instantiate(CandyPool[index3], transform);
        CandyChild.Add(child1);
        CandyChild.Add(child2);
        CandyChild.Add(child3);
        child1.GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        child2.GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        child3.GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        // Randomize layout to create different square-like arrangements
        int layoutChoice = UnityEngine.Random.Range(0, 4);

        switch (layoutChoice)
        {
            case 0:
                // Layout 1: Two small objects at bottom, one larger at top
                child1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child2.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child3.transform.localScale = new Vector3(1f, 0.5f, 1f);

                child1.transform.localPosition = new Vector3(-0.25f, -0.25f, 2);
                child2.transform.localPosition = new Vector3(0.25f, -0.25f, 2);
                child3.transform.localPosition = new Vector3(0, 0.25f, 2);
                break;

            case 1:
                // Layout 2: Two small objects at top, one larger at bottom
                child1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child2.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child3.transform.localScale = new Vector3(1f, 0.5f, 1f);
                child1.transform.localPosition = new Vector3(-0.25f, 0.25f, 2);
                child2.transform.localPosition = new Vector3(0.25f, 0.25f, 2);
                child3.transform.localPosition = new Vector3(0, -0.25f, 2);
                break;

            case 2:
                // Layout 3: Two small objects on left, one larger on right
                child1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child2.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child3.transform.localScale = new Vector3(0.5f, 1f, 1f);

                child1.transform.localPosition = new Vector3(-0.25f, -0.25f, 2);
                child2.transform.localPosition = new Vector3(-0.25f, 0.25f, 2);
                child3.transform.localPosition = new Vector3(0.25f, 0, 2);
                break;

            case 3:
                // Layout 4: Two small objects on right, one larger on left
                child1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child2.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                child3.transform.localScale = new Vector3(0.5f, 1f, 1f);

                child1.transform.localPosition = new Vector3(0.25f, -0.25f, 2);
                child2.transform.localPosition = new Vector3(0.25f, 0.25f, 2);
                child3.transform.localPosition = new Vector3(-0.25f, 0, 2);
                break;
        }
    }

    private void SpawnFourChildren()
    {
       
        // Get unique random indices from CandyPool
        List<int> availableIndices = Enumerable.Range(0, CandyPool.Count).ToList();

        // Randomly select 4 unique objects
        List<GameObject> selectedChildren = new List<GameObject>();
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
            selectedChildren.Add(Instantiate(CandyPool[randomIndex], transform));
            availableIndices.Remove(randomIndex);
        }

        // Set scale and position at 4 corners
        selectedChildren[0].transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        selectedChildren[1].transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        selectedChildren[2].transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        selectedChildren[3].transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        selectedChildren[0].transform.localPosition = new Vector3(-0.25f, 0.25f, 2);
        selectedChildren[1].transform.localPosition = new Vector3(0.25f, 0.25f, 2);
        selectedChildren[2].transform.localPosition = new Vector3(-0.25f, -0.25f, 2);
        selectedChildren[3].transform.localPosition = new Vector3(0.25f, -0.25f, 2);
        CandyChild.Add(selectedChildren[0]);
        CandyChild.Add(selectedChildren[1]);
        CandyChild.Add(selectedChildren[2]);
        CandyChild.Add(selectedChildren[3]);
        selectedChildren[0].GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        selectedChildren[1].GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        selectedChildren[2].GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        selectedChildren[3].GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
    }



}
 




public enum TypeCandy
{

    One,
    Two,
    Three,
    Four
}
