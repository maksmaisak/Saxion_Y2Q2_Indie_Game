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
        
        PlayerWeapon playerWeapon   = GetPlayerWeapon();
        playerWeapon.OnAmmoChanged += UpdateAmmo;
    }

    PlayerWeapon GetPlayerWeapon()
    {
        const string playerTag = "Player";
        
        var playerWeapon = GameObject
            .FindGameObjectsWithTag("Player")
            .Select(go => go.GetComponentInChildren<PlayerWeapon>())
            .FirstOrDefault(h => h != null);
        if (playerWeapon) return playerWeapon;

        const string PlayerGameObjectName = "Player";
        Debug.LogWarning($"Player not found by tag {playerTag}. Trying to find by gameObject name {PlayerGameObjectName}");
        return GameObject.Find(PlayerGameObjectName)?.GetComponentInChildren<PlayerWeapon>();
    }

    void UpdateAmmo(int newAmmo)
    {
        ammoText.SetText(newAmmo.ToString());
    }
}
