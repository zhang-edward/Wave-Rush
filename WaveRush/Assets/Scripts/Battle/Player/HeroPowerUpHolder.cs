﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroPowerUpHolder : MonoBehaviour
{
	private PlayerHero hero;
	[System.Serializable]
	public class HeroPowerUpDictionaryEntry 
	{
		public string name { 
			get {
				return powerUpPrefab.GetComponent<HeroPowerUp> ().data.powerUpName;
			}
		}
		public GameObject powerUpPrefab;
		public HeroPowerUpDictionaryEntry(GameObject prefab)
		{
			powerUpPrefab = prefab;
		}
	}

	//public List<HeroPowerUpDictionaryEntry> powerUpPrefabs { get; private set; }			// dictionary database of all available power ups
	public List<HeroPowerUp> powerUps;                  // list of powerups that are active on the player in the game
	public int numActivePowerUps;

	public delegate void OnPowerUpsChanged();
	public OnPowerUpsChanged OnPowerUpAdded;


	public void Init(HeroData heroData)
	{
		hero = GetComponent<PlayerHero>();
		HeroPowerUpListData powerUpListData = DataManager.GetPowerUpListData(hero.heroType);
		InitPowerUpList(heroData.level);
	}

	private void InitPowerUpList(int level)
	{
		HeroPowerUpListData powerUpListData = DataManager.GetPowerUpListData(hero.heroType);
		for (int i = 0; i < level; i ++)
		{
			HeroPowerUp powerUpPrefab = powerUpListData.GetPowerUpFromLevel(i);
			GameObject o = Instantiate(powerUpPrefab.gameObject);                         // instantiate the prefab
			HeroPowerUp powerUp = o.GetComponent<HeroPowerUp>();
			powerUps.Add(powerUp);
			o.transform.SetParent(transform);
			o.transform.localPosition = Vector3.zero;
			o.SetActive(false);
			AddPowerUp(powerUp.data.powerUpName);
		}
	}

	public HeroPowerUp GetPowerUp(string name)
	{
		foreach (HeroPowerUp powerUp in powerUps)
		{
			if (powerUp.data.powerUpName.Equals(name))
				return powerUp;
		}
		throw new UnityEngine.Assertions.AssertionException ("HeroPowerUpHolder.cs",
			"Cannot find HeroPowerUp with name" + "\"" + name + "\"");
	}

	public void AddPowerUp(string name)
	{
		HeroPowerUp selectedPowerUp = GetPowerUp(name).GetComponent<HeroPowerUp> ();
		print("Got power up:" + selectedPowerUp.gameObject);
		// test if this hero already has the selected power up
		if (selectedPowerUp.isActive)
		{
			print("Stacking...");
			selectedPowerUp.Stack ();
		}
		else
		{
			print("Power up not active. Activating...");
			selectedPowerUp.gameObject.SetActive(true);
			selectedPowerUp.Activate(hero);
			numActivePowerUps++;
		}
		// send event
		if (OnPowerUpAdded != null)
			OnPowerUpAdded();
	}
}

