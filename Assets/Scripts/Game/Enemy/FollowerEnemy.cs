﻿using UnityEngine;
using System.Collections;

public class FollowerEnemy : Enemy {

	public int damage = 1;

	protected override IEnumerator MoveState()
	{
		moveState = new FollowState (this);
		while (true)
		{
			moveState.UpdateState ();
			yield return null;
		}
	}

	protected override void ResetVars ()
	{
		body.gameObject.layer = DEFAULT_LAYER;
		body.moveSpeed = DEFAULT_SPEED;
	}

	void OnTriggerStay2D(Collider2D col)
	{
		if (col.CompareTag("Player"))
		{
			Player player = col.GetComponentInChildren<Player>();
			if (health > 0 && !hitDisabled)
				player.Damage (damage);
		}
	}
}
