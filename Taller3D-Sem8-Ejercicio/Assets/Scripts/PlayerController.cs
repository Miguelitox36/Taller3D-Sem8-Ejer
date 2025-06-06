using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float baseMoveSpeed = 15f; 
    private float currentMoveSpeed; 
    private float limitX, limitZ;

    private void Start()
    {        
        UpdateMoveSpeed(GameManager.instance.level);        
        UpdateLimits(GameManager.instance.GetMapUnitsSize(), GameManager.instance.GetTileUnitSize());       
        float mapHalfWidth = GameManager.instance.GetMapUnitsSize() * GameManager.instance.GetTileUnitSize();
        transform.position = new Vector3(0, 0.6f, -mapHalfWidth + (2f * GameManager.instance.GetTileUnitSize()));
        Debug.Log($"Player spawned. Limits: X=±{limitX}, Z=±{limitZ}");
    }

    private void Update()
    {
        Move();
    }
    
    public void UpdateMoveSpeed(int level)
    {        
        currentMoveSpeed = baseMoveSpeed + (level - 1) * 0.5f; 
    }
    
    public void UpdateLimits(int mapUnitsSize, float tileUnitSize)
    {        
        float mapHalfWidth = mapUnitsSize * tileUnitSize;
        limitX = mapHalfWidth - tileUnitSize; 
        limitZ = mapHalfWidth - tileUnitSize; 
        Debug.Log($"Player limits updated. X=±{limitX}, Z=±{limitZ}, mapHalfWidth={mapHalfWidth}");
    }
    
    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

       
        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;
        
        Vector3 newPos = transform.position + moveDir * currentMoveSpeed * Time.deltaTime;
               
        newPos.x = Mathf.Clamp(newPos.x, -limitX, limitX);
        newPos.z = Mathf.Clamp(newPos.z, -limitZ, limitZ);

        transform.position = newPos;
    }
}