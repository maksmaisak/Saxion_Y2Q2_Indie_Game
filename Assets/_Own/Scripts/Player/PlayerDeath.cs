using System;
using Cinemachine;
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
		new PlayerDied().PostEvent();
		this.Delay(levelRestartDelay, LevelManager.instance.RestartCurrentLevel);
	}
}
