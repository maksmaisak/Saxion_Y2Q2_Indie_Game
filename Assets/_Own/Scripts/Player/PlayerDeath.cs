using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class PlayerDeath : MyBehaviour, IEventReceiver<OnLevelCompleted>
{
	[SerializeField] float levelRestartDelay = 5f;
	
	private Health health;
	
	void Start()
	{
		health = GetComponent<Health>();
		Assert.IsNotNull(health);
		health.OnDeath += OnDeath;
	}

	private void OnDeath(Health health)
	{
		new OnPlayerDied().PostEvent();
		this.Delay(levelRestartDelay, LevelManager.instance.RestartCurrentLevel);
	}

	public void On(OnLevelCompleted message)
	{
		if (health) health.SetCanBeReduced(false);
	}
}
