using UnityEngine;

public class Cannon : MonoBehaviour
{
    //! Should probably be handled groupwise
    float _reload = 0;
    public float ReloadLeft => _reload;

    bool isReloading => ReloadLeft > 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (isReloading)
        {
            _reload -= Time.deltaTime;
        }
    }

    public void Fire()
    {
        print(isReloading ? "Im Reloading!" : "Fire!");
        if (!isReloading)
        {
            _reload = 5;
        }
    }
}
