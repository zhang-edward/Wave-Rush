﻿using UnityEngine;
using System.Collections;

public class KnightRushPowerUp : HeroPowerUp
{
	private const float MULTIPLIER = 1.2f;

	public KnightHero knight;
	private float totalSpeedMultiplier;		// the amount of speed that this powerup adds to the rush effect

	public override void Activate(PlayerHero hero)
	{
		base.Activate (hero);
		this.knight = (KnightHero)hero;
		knight.rushMoveSpeed *= MULTIPLIER;
		totalSpeedMultiplier = MULTIPLIER;
	}

	public override void Deactivate ()
	{
		base.Deactivate ();
		knight.rushMoveSpeed /= totalSpeedMultiplier;
	}

	public override void Stack ()
	{
		base.Stack ();
		knight.rushMoveSpeed *= MULTIPLIER;
		totalSpeedMultiplier *= MULTIPLIER;
	}
}

