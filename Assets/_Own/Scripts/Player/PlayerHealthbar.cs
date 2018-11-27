using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour
{
	[SerializeField] Image fillImage;
	[SerializeField] string playerTag = "Player";
	
	void Start()
	{
		Assert.IsNotNull(fillImage);
		
		Health playerHealth = GetPlayerHealth();
		Assert.IsNotNull(playerHealth, "Player health not found! Is the player in the scene?");

		UpdateBar(playerHealth);
		playerHealth.OnHealthChanged += (sender, oldValue, newValue) => UpdateBar(sender);
	}
	
	Health GetPlayerHealth()
	{
		var health = GameObject
			.FindGameObjectsWithTag("Player")
			.Select(go => go.GetComponentInChildren<Health>())
			.FirstOrDefault(h => h != null);
		if (health) return health;

		const string PlayerGameObjectName = "Player"; 
		Debug.LogWarning($"Player not found by tag {playerTag}. Trying to find by gameObject name {PlayerGameObjectName}");
		return GameObject.Find(PlayerGameObjectName)?.GetComponentInChildren<Health>();
	}
	
	private void UpdateBar(Health sender)
	{
		fillImage.fillAmount = (float)sender.health / sender.maxHealth;
	}
}
