﻿using UnityEngine;
using System.Collections;

public class WalkVicinityState : MoveState
{
	private enum State {
		Walk,
		Wait
	}
	private State state = State.Walk;
	private Vector3 target;
	private float waitTimer;    // how long this entity should wait, once it has reached its destination
	private Map map;

	public float waitTime = 1.0f;
	public float walkRadius = 1.0f;

	public override void Init (Enemy e, Transform player)
	{
		base.Init (e, player);
		map = e.map;
		anim = e.anim;
		state = State.Wait;
	}

	public override void Reset()
	{
		ToWalkState ();
	}

	// Simple FSM
	public override void UpdateState()
	{
		switch (state)
		{
		case State.Walk:
			WalkState ();
			break;
		case State.Wait:
			WaitState ();
			break;
		}
	}

	private void ToWalkState()
	{
		bool targetWithinMap = false;
		while (!targetWithinMap)
		{
			target = (Vector2)(player.position) + new Vector2(
					Random.Range(-walkRadius, walkRadius),
					Random.Range(-walkRadius, walkRadius));     // add a random offset;
			targetWithinMap = map.WithinOpenCells(target);
		}
		state = State.Walk;
	}

	private void WalkState()
	{
		if (Vector3.Distance (enemy.transform.position, target) > 0.1f)
		{
			// move
			if (!anim.player.IsPlayingAnimation(moveState))
				anim.Play(moveState);
			body.Move ((target - enemy.transform.position).normalized);
			// print("moving");
		}
		else
		{
			ToWaitState ();
		}
	}

	private void ToWaitState()
	{
		body.Move (Vector2.zero);
		anim.player.ResetToDefault();

		state = State.Wait;
	}

	private void WaitState()
	{
		// Debug.Log ("WaitState");
		waitTimer-= Time.deltaTime;
		if (waitTimer <= 0)
		{
			waitTimer = waitTime;
			ToWalkState ();
		}
	}
}

