﻿using UnityEngine;
using Projectiles;

public class MageFirestorm : HeroPowerUp
{
	public GameObject firestormPrefab;
	public IndicatorEffect indicator;

	private MageHero mage;
	private RuntimeObjectPooler originalProjectilePool;
	private RuntimeObjectPooler projectilePool;
	private float activateChance = 0.15f;
	private bool activated = false;

	public override void Activate(PlayerHero hero)
	{
		base.Activate(hero);
		mage = (MageHero)hero;
		originalProjectilePool = mage.projectilePool;
		projectilePool = (RuntimeObjectPooler)firestormPrefab.GetComponent<Projectile>().GetObjectPooler();
		mage.OnMageShotFireball += TryActivateAbility;
	}

	public void TryActivateAbility(GameObject o)
	{
		if (activated)
			DeactivateFirestorm();
		else if (Random.value < activateChance)
			ActivateFirestorm();
	}

	public override void Stack()
	{
		base.Stack();
		activateChance += 0.05f;
	}

	public void ActivateFirestorm()
	{
		mage.projectilePool = projectilePool;
		activated = true;
		indicator.gameObject.SetActive(true);
		print("Activated");
	}

	public void DeactivateFirestorm()
	{
		mage.projectilePool = originalProjectilePool;
		activated = false;
		indicator.AnimateOut();
		print("Deactivated");
	}
}