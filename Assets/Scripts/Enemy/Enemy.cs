﻿using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour, IDamageable {

	protected string DEFAULT_STATE = "MoveState";
	protected int DEFAULT_LAYER;
	protected float DEFAULT_SPEED;

	public SpriteRenderer sr;
	public Transform player;
	public EntityPhysics body;
	public Animator anim;

	public bool hitDisabled = false;

	public float playerDetectionRange = 2f;

	public Sprite deathSprite;
	public Sprite[] deathProps;
	public GameObject deathPropPrefab;

	public int health;

	void Awake()
	{
		DEFAULT_LAYER = body.gameObject.layer;
	}

	void Start()
	{
		DEFAULT_SPEED = body.moveSpeed;
	}

	public void Init(Vector3 spawnLocation)
	{
		StartCoroutine (SpawnState(spawnLocation));
	}

	public virtual void Damage(int amt)
	{
		// Stop all states
		StopAllCoroutines ();
		body.AddRandomImpulse ();
		health -= amt;

		if (health > 0)
			StartCoroutine (HitDisableState ());
		else
		{
			ResetVars ();
/*			anim.enabled = false;
			sr.sprite = deathSprite;
			sr.color = new Color (1, 1, 1, 0.8f);
			sr.sortingLayerName = "TerrainObjects";
			transform.parent.gameObject.layer = LayerMask.NameToLayer ("NoCollideSelf");
			transform.parent.rotation = Quaternion.Euler (new Vector3 (0, 0, Random.Range (0, 90))); */
			SpawnDeathProps ();
			transform.parent.gameObject.SetActive (false);
			//StartCoroutine (FadeAway ());
		}
	}

	public virtual void Heal (int amt)
	{
		health += amt;
	}

	private IEnumerator HitDisableState()
	{
		hitDisabled = true;
		sr.color = Color.red;
		//Debug.Log ("Stopped all Coroutines");
		yield return new WaitForSeconds (0.2f);
		sr.color = Color.white;
		hitDisabled = false;

		UnityEngine.Assertions.Assert.IsTrue(anim.HasState(0, Animator.StringToHash("default")));
		anim.CrossFade ("default", 0f);

		ResetVars ();
		StartCoroutine (DEFAULT_STATE);
		yield return null;
	}

	protected bool PlayerInRange()
	{
		Collider2D[] cols = Physics2D.OverlapCircleAll (transform.position, playerDetectionRange);
		foreach (Collider2D col in cols)
		{
			if (col.CompareTag ("Player"))
			{
				return true;
			}
		}
		return false;
	}

	protected Player GetPlayerInRange()
	{
		Collider2D[] cols = Physics2D.OverlapCircleAll (transform.position, playerDetectionRange);
		foreach (Collider2D col in cols)
		{
			if (col.CompareTag ("Player"))
			{
				return col.GetComponentInChildren<Player>();
			}
		}
		return null;
	}

	// implement object pooling
	private void SpawnDeathProps()
	{
		foreach (Sprite sprite in deathProps)
		{
			GameObject o = Instantiate (deathPropPrefab, transform.position, Quaternion.identity) as GameObject;
			//o.transform.SetParent (this.transform);
			o.transform.rotation = Quaternion.Euler (new Vector3 (0, 0, Random.Range (0, 90)));
			o.GetComponent<SpriteRenderer> ().sprite = sprite;
			o.GetComponent<Rigidbody2D> ().AddTorque (Random.Range (-50f, 50f));
			o.GetComponent<Rigidbody2D> ().AddForce (new Vector2(
				Random.value - 0.5f,
				Random.value - 0.5f),
				ForceMode2D.Impulse);
		}
	}

	protected abstract void ResetVars();
	protected abstract IEnumerator MoveState();
	protected virtual IEnumerator SpawnState(Vector3 target)
	{
		body.gameObject.layer = LayerMask.NameToLayer ("EnemySpawnLayer");
		anim.SetBool ("Moving", true);
		while (Vector3.Distance(transform.position, target) > 0.1f)
		{
			body.Move ((target - transform.position).normalized);
			yield return null;
		}
		body.gameObject.layer = DEFAULT_LAYER;
		StartCoroutine (DEFAULT_STATE);
		//Debug.Log ("Done");
	}

/*	private IEnumerator FadeAway()
	{
		yield return new WaitForSeconds (10.0f);
		while (sr.color.a > 0)
		{
			sr.color = new Color (1, 1, 1, sr.color.a - 0.01f);
			yield return null;
		}
		sr.color = new Color (1, 1, 1, 0);
	}*/
}
