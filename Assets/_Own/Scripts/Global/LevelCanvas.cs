using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCanvas : Singleton<LevelCanvas>
{
    [SerializeField] GameObject _activeInThirdPersonOnly;
    public GameObject activeInThirdPersonOnly => _activeInThirdPersonOnly;
    
    [SerializeField] GameObject _activeInSniperZoomOnly;
    public GameObject activeInSniperZoomOnly => _activeInSniperZoomOnly;
}
