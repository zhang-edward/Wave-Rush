﻿using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Player/PowerUpListData", order = 2)]
public class HeroPowerUpListData : ScriptableObject
{
	public HeroType type;
	public HeroPowerUp[] powerUps;
}