﻿using UnityEngine;
public class DataManager : MonoBehaviour
{
	public static DataManager instance;

	public GameObject[] heroPrefabs;		// hero behavior data
	public HeroData[] heroData;		// power up info for every hero
	public StatData statData;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this.gameObject);
		DontDestroyOnLoad(this);
	}

	public static GameObject GetPlayerHero(HeroType heroType) {
		foreach (GameObject o in instance.heroPrefabs) {
			if (o.GetComponent<PlayerHero>().heroType == heroType) {
				return o;
			}
		}
		throw new UnityEngine.Assertions.AssertionException
							 (instance.GetType() + ".cs",
							  "Could not find playerHero for hero with name " + heroType.ToString() + "!");
	}

	public static HeroData GetHeroData(HeroType heroType) {
		foreach (HeroData data in instance.heroData) {
			if (data.heroType == heroType)
				return data;
		}
		throw new UnityEngine.Assertions.AssertionException
							 (instance.GetType() + ".cs",
							  "Could not find data for hero with name " + heroType.ToString() + "!");
	} 

	public static HeroPowerUpListData GetPowerUpListData(HeroType heroType)
	{
		foreach (HeroData data in instance.heroData)
		{
			if (data.heroType == heroType)
				return data.powerUpData;
		}
		throw new UnityEngine.Assertions.AssertionException
		                     (instance.GetType() + ".cs",
							  "Could not find data for hero with name " + heroType.ToString() + "!");
	}

	/*public static HeroDescriptionData GetDescriptionData(HeroType type)
	{
		foreach (HeroDescriptionData data in instance.heroDescriptions)
		{
			if (data.heroName == type)
				return data;
		}
		throw new UnityEngine.Assertions.AssertionException
							 (instance.GetType() + ".cs",
							  "Could not find data for hero with name " + type.ToString() + "!");
	}*/
}
