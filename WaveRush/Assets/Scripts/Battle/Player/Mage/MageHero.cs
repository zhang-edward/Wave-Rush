using UnityEngine;
using Projectiles;
using System.Collections;

public class MageHero : PlayerHero {

	[Header("Class-Specific")]
	public ObjectPooler effectPool;
	public RuntimeObjectPooler projectilePool;
	[Space]
	public Sprite hitEffect;
	public Map map;
	public GameObject projectilePrefab;
	private bool activatedSpecialAbility;
	public MageFire mageFirePrefab;

	[Header("Audio")]
	public AudioClip shootSound;
	public AudioClip teleportOutSound;
	public AudioClip teleportInSound;
	public AudioClip powerUpSound;
	public AudioClip powerDownSound;

	public delegate void MageAbilityActivated();
	public event MageAbilityActivated OnMageTeleportIn;
	public event MageAbilityActivated OnMageTeleportOut;
	public event MageAbilityActivated OnMageSpecialAbility;

	public delegate void MageCreatedObject (GameObject o);
	public event MageCreatedObject OnMageShotFireball;

	//private float chargeTime;
	//private bool sprayingFire;
	//private float tapHoldTime;
	//private const float minTapHoldTime = 0.2f;

	public override void Init(EntityPhysics body, Player player, Pawn heroData)
	{
		cooldownTimers = new float[2];
		map = GameObject.Find ("Map").GetComponent<Map>();
		projectilePool = (RuntimeObjectPooler)projectilePrefab.GetComponent<Projectile>().GetObjectPooler();
		base.Init (body, player, heroData);

		onSwipe = ShootFireball;
		onTap = StartTeleport;
	}

	public override void SpecialAbility ()
	{
		if (specialAbilityCharge < specialAbilityChargeCapacity || activatedSpecialAbility)
			return;
		// Sound
		SoundManager.instance.PlayImportantSound(powerUpSound);
		activatedSpecialAbility = true;

		CameraControl.instance.StartFlashColor (Color.white);
		CameraControl.instance.SetOverlayColor (new Color(1, 0.2f, 0), 0.2f);
		CameraControl.instance.StartShake (0.3f, 0.05f);

		if (OnMageSpecialAbility != null)
			OnMageSpecialAbility();

		Invoke ("ResetSpecialAbility", 10.0f);
	}
		
	private void ResetSpecialAbility()
	{
		// Sound
		SoundManager.instance.PlayImportantSound(powerDownSound);

		activatedSpecialAbility = false;
		specialAbilityCharge = 0;

		CameraControl.instance.StartFlashColor (Color.white);
		CameraControl.instance.SetOverlayColor (Color.clear, 0f);
	}

	private void ShootFireball()
	{
		if (!IsCooledDown (0, true, HandleSwipe))
			return;
		ResetCooldownTimer (0);
		// Sound
		SoundManager.instance.RandomizeSFX (shootSound);
		// Animation
		anim.Play ("Shoot");

		// actual projectile stuff
		GameObject fireballObj = projectilePool.GetPooledObject ();
		Vector2 dir = player.dir.normalized;
		Projectile fireball = fireballObj.GetComponent<Projectile> ();
		fireball.GetComponentInChildren<AreaDamageAction>().damage = damage;
 		fireball.Init (transform.position, dir);
		

		// recoil
		body.Move (dir);	// set the sprites flipX to the correct direction
		body.Rb2d.velocity = dir * -4f;

		// event
		if (OnMageShotFireball != null)
			OnMageShotFireball (fireballObj);

		// Reset the ability
		Invoke ("ResetShootFireball", 0.5f);
	}

	public void ResetShootFireball()
	{
		body.moveSpeed = player.DEFAULT_SPEED;
	}

	public void StartTeleport()
	{
		if (!IsCooledDown (1))
			return;
		if (map.WithinOpenCells(player.transform.position + (Vector3)player.dir))
			StartCoroutine (Teleport ());
	}

	private IEnumerator Teleport()
	{
		ResetCooldownTimer (1);
		// Sound
		SoundManager.instance.RandomizeSFX (teleportOutSound);
		// Animation
		anim.Play ("TeleOut");
		// Set properties
		player.isInvincible = true;
		player.input.isInputEnabled = false;
		// event trigger
		if (OnMageTeleportOut != null)
			OnMageTeleportOut();
		// Wait for end of animation
		while (anim.player.isPlaying)
			yield return null;
		// Animation
		anim.Play("TeleIn");
		// Sound
		SoundManager.instance.RandomizeSFX (teleportInSound);
		// (animation triggers automatically)
		// Set position
		player.transform.parent.position = (Vector3)player.dir + player.transform.parent.position;
		// do area attack
		AreaAttack ();
		if (activatedSpecialAbility)
			CreateFire ();
		// event trigger
		if (OnMageTeleportIn != null)
			OnMageTeleportIn ();
		// Wait for end of animation
		while (anim.player.isPlaying)
			yield return null;
		// reset properties
		player.isInvincible = false;
		player.input.isInputEnabled = true;
	}

	private void AreaAttack()
	{
		Collider2D[] cols = Physics2D.OverlapCircleAll (transform.position, 1.5f);
		foreach (Collider2D col in cols)
		{
			if (col.CompareTag("Enemy"))
			{
				Enemy e = col.gameObject.GetComponentInChildren<Enemy> ();
				DamageEnemy (e);
			}
		}
	}

	private void CreateFire()
	{
		Instantiate (mageFirePrefab, transform.position, Quaternion.identity);
	}

	// damage an enemy and spawn an effect
	private void DamageEnemy(Enemy e)
	{
		if (!e.invincible && e.health > 0)
		{
			e.Damage (damage);
			player.effectPool.GetPooledObject ().GetComponent<TempObject> ().Init (
				Quaternion.Euler (new Vector3 (0, 0, Random.Range (0, 360f))),
				e.transform.position, 
				hitEffect,
				true,
				0,
				0.2f,
				1.0f);

			player.TriggerOnEnemyDamagedEvent(damage);
			player.TriggerOnEnemyLastHitEvent (e);
		}
	}
}
