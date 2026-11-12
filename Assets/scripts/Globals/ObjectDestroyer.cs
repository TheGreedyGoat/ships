using UnityEngine;

public static class ObjectDestroyer
{
    /// <summary>
    /// Destroys the passed Unity Object, automatically handling if the app is playing.
    /// </summary>
    /// <param name="obj"></param>
    public static void Destroy(Object obj)
    {
        if (Application.isPlaying)
        {
            Object.Destroy(obj);
        }
        else
        {
            Object.DestroyImmediate(obj);
        }
    }
}
