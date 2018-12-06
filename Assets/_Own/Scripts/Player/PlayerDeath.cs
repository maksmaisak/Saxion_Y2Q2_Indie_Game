using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Health))]
public class PlayerDeath : MyBehaviour, IEventReceiver<OnLevelCompleted>
{
	[SerializeField] bool restartAutomatically;
	[SerializeField] float autoRestartDelay = 12f;
	
	private Health health;
	
	void Start()
	{
		health = GetComponent<Health>();
		Assert.IsNotNull(health);
		health.OnDeath += OnDeath;
	}

	private void OnDeath(Health sender)
	{
		new OnPlayerDied().PostEvent();
		
		if (restartAutomatically)
		{
			this.Delay(autoRestartDelay, LevelManager.instance.RestartCurrentLevel);
		}
	}

	public void On(OnLevelCompleted message)
	{
		if (health) health.SetCanBeReduced(false);
	}
}
