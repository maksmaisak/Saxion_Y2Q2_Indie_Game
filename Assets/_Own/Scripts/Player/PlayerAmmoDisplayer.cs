using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAmmoDisplayer : MonoBehaviour
{
    [SerializeField] TMP_Text ammoText;
    
    void Start()
    {
        Assert.IsNotNull(ammoText);
        
        PlayerAmmoManager playerWeapon   = GetPlayerWeapon();
        playerWeapon.OnAmmoChanged += UpdateAmmo;
    }

    PlayerAmmoManager GetPlayerWeapon()
    {
        const string playerTag = "Player";
        
        var playerWeapon = GameObject
            .FindGameObjectsWithTag("Player")
            .Select(go => go.GetComponentInChildren<PlayerAmmoManager>())
            .FirstOrDefault(h => h != null);
        if (playerWeapon) return playerWeapon;

        const string PlayerGameObjectName = "Player";
        Debug.LogWarning($"Player not found by tag {playerTag}. Trying to find by gameObject name {PlayerGameObjectName}");
        return GameObject.Find(PlayerGameObjectName)?.GetComponentInChildren<PlayerAmmoManager>();
    }

    void UpdateAmmo(int currentAmmo, int totalAmmoInMagazines)
    {
        ammoText.SetText($"{currentAmmo}/{totalAmmoInMagazines}");
    }
}
