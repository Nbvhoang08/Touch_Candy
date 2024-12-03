using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
public class CandyBound : MonoBehaviour
{
    // Start is called before the first frame update
    public TypeCandy typeCandy;
    [SerializeField] List<GameObject> CandyPool = new List<GameObject>();
    public bool CanMove;
    private Vector3 startPosition;
    private bool isDragging = false;
    public bool _done = false;
    private Collider2D cellCollider;
    public List<GameObject> CandyChild = new List<GameObject>();
    private bool isScaling = false;
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
        InitCandy();
        startPosition = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (_gameManager == null)
        {
            _gameManager = FindAnyObjectByType<GameManager>();
        }
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Đảm bảo object không thay đổi trục Z
            transform.position = mousePosition;
        }
       

        if (CandyChild.All(child => !child.activeSelf))
        {
            StartCoroutine(DestroyAfterDelay());
        }



    }
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject); // Destroy object chính sau 0.5 giây
     
    }

    /// <summary>
    /// Handle Movement
    /// </summary>
    void OnMouseDown()
    {
      
        if(!_done)
        isDragging = true;
        
    }
    void OnMouseUp()
    {
        isDragging = false;
        Vector3 snappedPosition = SnapToGrid(transform.position);
        if (IsValidCell(snappedPosition) && !_done)
        {
            transform.position = snappedPosition;
            foreach(GameObject candy in CandyChild)
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
        if (activeChildren.Count == 2)
        {
            StartCoroutine(ScaleAndRepositionMultiple(null, activeChildren));
            return;
        }

        // Find the disabled child
        GameObject disabledChild = CandyChild.FirstOrDefault(child => !child.activeSelf);
        if (disabledChild == null) return;

        // Find the best child to scale
        GameObject childToScale = FindBestChildToScale(disabledChild);
        if (childToScale != null)
        {
            StartCoroutine(ScaleAndRepositionChild(disabledChild, childToScale));
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
        float duration = 0.5f;
        float elapsedTime = 0;

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
    // Special case handler
    private List<GameObject> HandleSpecialCase()
    {
        // Get active children
        List<GameObject> activeChildren = CandyChild.Where(child => child.activeSelf).ToList();

        // Check if exactly 2 objects remain and their scales are equal
        if (activeChildren.Count == 2)
        {
            Vector3 scale1 = activeChildren[0].transform.localScale;
            Vector3 scale2 = activeChildren[1].transform.localScale;

            if (Mathf.Approximately(scale1.x, scale2.x) && Mathf.Approximately(scale1.y, scale2.y))
            {
                return activeChildren; // Return both objects
            }
        }
        return null;
    }

    // Coroutine to scale and reposition multiple objects
    private IEnumerator ScaleAndRepositionMultiple(GameObject disabledChild, List<GameObject> objectsToScale)
    {
        isScaling = true;

        GameObject child1 = objectsToScale[0];
        GameObject child2 = objectsToScale[1];

        // Calculate combined scale and position
        Vector3 combinedScale = child1.transform.localScale;
        Vector3 targetScale = combinedScale;

        Vector3 targetPosition1 = child1.transform.localPosition;
        Vector3 targetPosition2 = child2.transform.localPosition;
        


        if (child1.transform.localPosition.x == child2.transform.localPosition.x)
        {
            targetPosition1.x = 0f;
            targetPosition2.x = 0f;
            targetScale.x = combinedScale.x*2;
            Mathf.Clamp(targetScale.x,0,1);
        }
        else{
            targetPosition1.y = 0f;
            targetPosition2.y = 0f;
            targetScale.y = combinedScale.y *2;
            Mathf.Clamp(targetScale.y, 0, 1);
        }

        // Animation parameters
        float duration = 0.5f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Interpolate scale for both objects
            child1.transform.localScale = Vector3.Lerp(child1.transform.localScale, targetScale, elapsedTime / duration);
            child2.transform.localScale = Vector3.Lerp(child2.transform.localScale, targetScale, elapsedTime / duration);

            // Interpolate position
            child1.transform.localPosition = Vector3.Lerp(child1.transform.localPosition, targetPosition1, elapsedTime / duration);
            child2.transform.localPosition = Vector3.Lerp(child2.transform.localPosition, targetPosition2, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final values
        child1.transform.localScale = targetScale;
        child2.transform.localScale = targetScale;
        child1.transform.localPosition = targetPosition1;
        child2.transform.localPosition = targetPosition2;
       
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
        float duration = 0.5f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Interpolate position and scale
            child.transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / duration);
            child.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

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

        // Ensure we have enough objects in the CandyPool
        if (CandyPool.Count < 4)
        {
            Debug.LogError("Not enough objects in CandyPool to spawn children!");
            return;
        }

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
        int RandomIndex = Random.Range(0,CandyPool.Count);
        GameObject child = Instantiate(CandyPool[RandomIndex], transform);
        child.transform.localPosition = new Vector3(0,0,2); 
        child.transform.localScale = Vector3.one;
        child.GetComponent<Candy>().bound = this.GetComponent<CandyBound>();
        CandyChild.Add(child);
    }

    private void SpawnTwoChildren()
    {
        // Randomly choose between horizontal and vertical layout
        bool isHorizontal = Random.value > 0.5f;

        // Get unique random indices from CandyPool
        List<int> availableIndices = Enumerable.Range(0, CandyPool.Count).ToList();
        int index1 = availableIndices[Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(index1);
        int index2 = availableIndices[Random.Range(0, availableIndices.Count)];

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
        int index1 = availableIndices[Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(index1);
        int index2 = availableIndices[Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(index2);
        int index3 = availableIndices[Random.Range(0, availableIndices.Count)];

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
        int layoutChoice = Random.Range(0, 4);

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
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
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
