using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelCanvas : Singleton<LevelCanvas>
{    
    [SerializeField] GameObject _activeInThirdPersonOnly;
    public GameObject activeInThirdPersonOnly => _activeInThirdPersonOnly;
    
    [SerializeField] GameObject _activeInSniperZoomOnly;
    public GameObject activeInSniperZoomOnly => _activeInSniperZoomOnly;

    [SerializeField] DamageOverlay _damageOverlay;
    public DamageOverlay damageOverlay => _damageOverlay;

    public static LevelCanvas Get()
    {
        Assert.IsTrue(exists, "Level canvas not found! Put in the canvas prefab if it's not there.");
        return instance;
    }
}
