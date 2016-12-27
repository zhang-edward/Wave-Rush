﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KnightHero : PlayerHero {

	[Header("Class-Specific")]
	public GameObject rushEffect;
	public GameObject areaAttackEffect;
	public Sprite hitEffect;
	public float areaAttackRange = 2.0f;
	private bool activatedSpecialAbility = false;

	public float rushMoveSpeed = 9f;
	public float rushDuration = 0.5f;
	private bool killBox = false;

	[Header("Audio")]
	public AudioClip rushSound;
	public AudioClip[] hitSounds;
	public AudioClip areaAttackSound;
	public AudioClip powerUpSound;
	public AudioClip powerDownSound;

	private List<Enemy> hitEnemies = new List<Enemy>();

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere (transform.position, 1f);
	}

	public override void Init (EntityPhysics body, Animator anim, Player player)
	{
		abilityCooldowns = new float[2];
		base.Init (body, anim, player);
		heroName = PlayerHero.HERO_TYPES ["KNIGHT"];
	}

	// Dash attack
	public override void HandleSwipe()
	{
		// if cooldown has not finished
		if (abilityCooldowns[0] > 0)
		{
			if (abilityCooldowns [0]< 0.3f)
			{
				inputAction = HandleSwipe;
				QueueAction (abilityCooldowns [0]);
			}
			return;
		}
		ResetCooldown (0);
		// Sound
		SoundManager.instance.RandomizeSFX (rushSound);
		// Animation
		anim.SetBool ("Attacking", true);
		// Effects
		PlayRushEffect ();
		// Player properties
		player.input.isInputEnabled = false;
		killBox = true;
		body.moveSpeed = rushMoveSpeed;
		body.Move(player.dir.normalized);
		Debug.DrawRay (transform.position, player.dir, Color.red, 0.5f);
		player.isInvincible = true;
		// reset ability
		Invoke ("ResetRushAbility", rushDuration);
	}

	// Area attack
	public override void HandleTapRelease ()
	{
		if (abilityCooldowns [1] > 0)
			return;
		ResetCooldown (1);
		// Sound
		SoundManager.instance.RandomizeSFX (areaAttackSound);
		// Animation
		anim.SetTrigger ("AreaAttack");
		// Effects
		PlayAreaAttackEffect ();
		// Properties
		player.input.isInputEnabled = false;
		player.isInvincible = true;
		body.Move (Vector2.zero);
		Collider2D[] cols = Physics2D.OverlapCircleAll (transform.position, areaAttackRange);
		foreach (Collider2D col in cols)
		{
			if (col.CompareTag("Enemy"))
			{
				Enemy e = col.gameObject.GetComponentInChildren<Enemy> ();
				DamageEnemy (e);
			}
		}
		// Reset Ability
		Invoke ("ResetAreaAttackAbility", 0.5f);
	}

	private void ResetRushAbility()
	{
		hitEnemies.Clear ();
		rushEffect.GetComponent<TempObject> ().Deactivate ();
		killBox = false;
		body.moveSpeed = player.DEFAULT_SPEED;

		// if special ability is activated, stay invincible
		if (!activatedSpecialAbility)
			player.isInvincible = false;
		
		player.input.isInputEnabled = true;
		anim.SetBool ("Attacking", false);
	}

	private void ResetAreaAttackAbility()
	{
		hitEnemies.Clear ();
		anim.SetBool ("AreaAttack", false);
		player.input.isInputEnabled = true;

		// if special ability is activated, stay invincible
		if (!activatedSpecialAbility)
			player.isInvincible = false;
	}

	private void ResetSpecialAbility()
	{
		// Sound
		SoundManager.instance.PlayImportantSound(powerDownSound);

		player.isInvincible = false;
		activatedSpecialAbility = false;
		specialAbilityCharge = 0;
		cooldownTime [0] = cooldownTimeNormal [0];
		cooldownTime [1] = cooldownTimeNormal [1];
		rushMoveSpeed = 10f;
		rushDuration = 0.5f;

		CameraControl.instance.StartFlashColor (Color.white);
		CameraControl.instance.SetOverlayColor (Color.clear, 0);
	}

	public override void SpecialAbility ()
	{
		if (specialAbilityCharge < specialAbilityChargeCapacity || activatedSpecialAbility)
			return;
		// Sound
		SoundManager.instance.PlayImportantSound(powerUpSound);
		// Properties
		activatedSpecialAbility = true;
		cooldownTime [0] = 0.5f;
		cooldownTime [1] = 2f;
		rushMoveSpeed = 15;
		rushDuration = 0.4f;
		CameraControl.instance.StartShake (0.3f, 0.05f);
		CameraControl.instance.StartFlashColor (Color.white);
		CameraControl.instance.SetOverlayColor (Color.red, 0.3f);
		Invoke ("ResetSpecialAbility", 10f);
	}

	private void PlayRushEffect()
	{
		TempObject effect = rushEffect.GetComponent<TempObject> ();
		SimpleAnimationPlayer animPlayer = rushEffect.GetComponent<SimpleAnimationPlayer> ();

		Vector2 dir = player.dir;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		TempObjectInfo info = new TempObjectInfo ();
		info.isSelfDeactivating = false;
		info.targetColor = new Color (1, 1, 1, 0.5f);
		info.fadeOutTime = 0.1f;
		effect.Init (Quaternion.Euler (new Vector3 (0, 0, angle)), transform.position, animPlayer.anim.frames [0], info);
		animPlayer.Play ();
	}

	private void PlayAreaAttackEffect()
	{
		TempObject effect = areaAttackEffect.GetComponent<TempObject> ();
		SimpleAnimationPlayer animPlayer = areaAttackEffect.GetComponent<SimpleAnimationPlayer> ();

		TempObjectInfo info = new TempObjectInfo ();
		info.isSelfDeactivating = true;
		info.lifeTime = animPlayer.anim.TimeLength;
		info.targetColor = new Color (1, 1, 1, 1f);
		effect.Init (
			Quaternion.identity,
			transform.position,
			animPlayer.anim.frames[0],
			info
		);
		animPlayer.Play ();
	}

	void OnTriggerStay2D(Collider2D col)
	{
		if (killBox)
		{
			if (col.CompareTag("Enemy"))
			{
				Enemy e = col.gameObject.GetComponentInChildren<Enemy> ();
				DamageEnemy (e);
			}
		}
	}

	private void DamageEnemy(Enemy e)
	{
		if (!e.invincible && e.health > 0 && !hitEnemies.Contains(e))
		{
			//string status = "Poison";
			/*if (Random.value < 0.5f)
				status = "Freeze";
			else
				status = "Burn";*/
			//e.AddStatus (Instantiate (StatusEffectContainer.instance.GetStatus (status)));
			e.Damage (damage);
			hitEnemies.Add (e);
			/*Instantiate (hitEffect, 
						Vector3.Lerp (transform.position, e.transform.position, 0.5f), 
						Quaternion.identity);*/
			player.effectPool.GetPooledObject().GetComponent<TempObject>().Init(
				Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360f))),
				Vector3.Lerp (transform.position, e.transform.position, 0.5f), 
				hitEffect,
				true,
				0);

			SoundManager.instance.RandomizeSFX (hitSounds[Random.Range(0, hitSounds.Length)]);
			player.TriggerOnEnemyDamagedEvent(damage);
		}
	}
}
