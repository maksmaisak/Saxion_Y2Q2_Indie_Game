using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class PlayerDeath : MonoBehaviour
{
	[SerializeField] float levelRestartDelay = 5f;
	
	void Start()
	{
		var health = GetComponent<Health>();
		Assert.IsNotNull(health);
		health.OnDeath += OnDeath;
	}

	private void OnDeath(Health health)
	{
		this.Delay(levelRestartDelay, Restart);
	}

	private void Restart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
