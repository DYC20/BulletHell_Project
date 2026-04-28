using UnityEngine;

public class FootPrint : MonoBehaviour
{
    public float lifeTime = 2.0f;

    private float mark;

    private Vector3 originalSize;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mark = Time.time;
        originalSize = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float ElapsedTime = Time.time - mark;
        if (ElapsedTime != 0)
        {
            float percentage = (lifeTime - ElapsedTime) / lifeTime;
            
            transform.localScale = new Vector3(originalSize.x * percentage, originalSize.y * percentage, originalSize.z * percentage);
            if (ElapsedTime > lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
