using UnityEngine;

public class Enemy : MonoBehaviour
{    
    protected Vector3 relativePosition;   
    protected EnemyFormationManager formationManager;
       
    protected float currentEnemySpeed = 1f;
        
    public void SetFormationManager(EnemyFormationManager manager, Vector3 relative)
    {
        formationManager = manager;
        relativePosition = relative;
    }
       
    public void SetEnemySpeed(float speed)
    {
        currentEnemySpeed = speed;
    }
        
    void Update()
    {
        Move(); 
    }

    void LateUpdate()
    {
        transform.localPosition = relativePosition;
    }

    protected virtual void Move()
    {
       
    }
        
    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("BottomWall"))
        {
            if (GameManager.instance != null)
            {                
                GameManager.instance.EnemyReachedBottom(this);
            }
           
            Destroy(gameObject);
        }
    }
   
    void OnDestroy()
    {        
        if (formationManager != null)
        {
            formationManager.RemoveEnemy(this);
        }
    }
}