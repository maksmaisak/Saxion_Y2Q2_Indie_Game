using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

/// Ensures the "__preload" scene has been loaded.
/// Put on vital GameObjects such as the player to make it easy.
public class EnsurePreloadScene : MonoBehaviour
{
    public static bool isPreloadLoading { get; private set; }
    public static bool wasPreloadLoaded { get; private set; }

    void Awake()
    {
        if (wasPreloadLoaded || isPreloadLoading) return;
        if (GameObject.Find("__app"))
        {
            wasPreloadLoaded = true;
            return;
        }

        SceneManager.LoadScene(SceneNames.preload, LoadSceneMode.Additive);
        
        isPreloadLoading = true;
        this.DoNextFrame(() =>
        {
            wasPreloadLoaded = true;
            isPreloadLoading = false;
        });
    }
}