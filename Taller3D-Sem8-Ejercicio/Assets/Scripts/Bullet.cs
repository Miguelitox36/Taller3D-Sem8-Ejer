using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public GameObject hitEffect;

    private Rigidbody rb;
    private TrailRenderer trail;

    void Start()
    {       
        Destroy(gameObject, lifetime);
               
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
               
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = transform.forward * speed;
        }
                
        if (trail != null)
        {
            trail.time = 0.5f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0.05f;
        }
    }

    void Update()
    {       
        if (rb == null || rb.velocity.magnitude < 0.1f)
        {           
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {       
        if (other.CompareTag("Enemy"))
        {           
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && GameManager.instance != null)
            {                
                GameManager.instance.EnemyKilled(enemy);
            }
                        
            if (other.gameObject != null)
            {
                Destroy(other.gameObject);
            }
            
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        
        else if (other.CompareTag("Wall"))
        {            
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
                       
            Destroy(gameObject);
        }
    }
}