﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour {

	private const float DIFFICULTY_CURVE = 5f;

	public Player player;
	public int enemiesKilled { get; private set; }
	public Map map;

	public ObjectPooler enemyHealthBarPool;

	private List<Enemy> enemies = new List<Enemy>();
	public List<Enemy> Enemies { get {return enemies;} }

	public EnemyData[] enemyData;
	public EnemyData data { get; private set; }
	public MapType chosenMap;

	public EnemyHealthBar bossHealthBar;
	private BossSpawn bossSpawn;
	public int shopWave = 3;
	public int bossWave = 5;
	public float bossSpawnDelay = 3f;
	// public int onBossDifficultyScaleBack = 3;	// subtract this from difficultyCurve on a boss wave

	public GameObject heartPickup;
	public GameObject moneyPickup;

	public int waveNumber { get; private set; }
	private int difficultyCurve = 0;	// number to determine the number of enemies to spawn
	public ShopNPC shopNPC;

	public List<BossEnemy> bosses;

	public delegate void EnemyWaveSpawned (int waveNumber);
	public event EnemyWaveSpawned OnEnemyWaveSpawned;
	public delegate void EnemyWaveCompleted ();
	public event EnemyWaveCompleted OnEnemyWaveCompleted;
	public delegate void BossIncoming();
	public event BossIncoming OnQueueBossMessage;

	void OnEnable()
	{
		player.OnPlayerInitialized += Init;
	}

	void OnDisbled()
	{
		player.OnPlayerInitialized -= Init;
	}

	private void Init()
	{
		data = GetEnemyData ();
		bossSpawn = map.bossSpawn.GetComponent<BossSpawn> ();
		StartCoroutine (StartSpawningEnemies ());
	}

	private EnemyData GetEnemyData()
	{
		foreach (EnemyData data in enemyData)
		{
			if (data.mapType == chosenMap)
				return data;
		}
		throw new UnityEngine.Assertions.AssertionException ("EnemyManager.cs:", "EnemyManagerInfo not found");
	}

	private IEnumerator StartSpawningEnemies()
	{
		int prevCount = 0;
		int count = 0;

		while (true)
		{
			// all enemies dead
			count = NumAliveEnemies();
			if (count != prevCount)
			{
				prevCount = count;
				//print ("Number of enemies still alive: " + count);
			}
			if (NumAliveEnemies() <= 0)
			{
				if (waveNumber >= 1)
				{
					OnEnemyWaveCompleted ();
				}
				waveNumber++;
				difficultyCurve++;
				// spawn shop every 3 waves after wave 5
				if (waveNumber % shopWave == 0 && waveNumber > 1)
				{
					// spawn shop npc before the boss
					shopNPC.Appear ();
					// wait for shopNPC to disappear
					while (shopNPC.gameObject.activeInHierarchy)
						yield return null;
				}
				// every 'bossWave' waves, spawn a boss
				if (waveNumber % bossWave == 0)
				{					
					StartBossIncoming ();
				}
				// if it is the wave after a boss wave (just defeated boss), spawn heart pickup
				if (waveNumber % bossWave == 1 && waveNumber != 1)
				{
					Instantiate (heartPickup, map.CenterPosition, Quaternion.identity);
				}

				StartNextWave ();
			}
			yield return null;
		}
	}

	private void StartNextWave()
	{
		// event call
		if (OnEnemyWaveSpawned != null)
			OnEnemyWaveSpawned (waveNumber);
		// Number of enemies spawning curve (used desmos.com for the graph)
		int spawningPointsAvailable = DifficultyCurveEquation();
		List<GameObject> prefabPool = GetPrefabPool();

		for (int i = 0; i < spawningPointsAvailable; i++)
		{
			Vector3 randOpenCell = map.OpenCells [Random.Range (0, map.OpenCells.Count)];
			SpawnEnemy (prefabPool [Random.Range (0, prefabPool.Count)], randOpenCell);
		}

		Debug.Log ("Number of enemies in this wave: " + spawningPointsAvailable);
	}

    private int DifficultyCurveEquation()
    {
		float t = -difficultyCurve + 18;	// 18 = max slope part of curve (difficulty increases most on this wave)
		float answer = 20 / (1 + Mathf.Pow(1.2f, t)) + 5;
		return Mathf.RoundToInt(answer);
    }

	/// <summary>
	/// Gets the list of potential enemies to spawn
	/// </summary>
	/// <returns>Prefab pool</returns>
	private List<GameObject> GetPrefabPool()
	{
		List<GameObject> prefabPool = new List<GameObject>();
		if (waveNumber <= 5)
		{
			prefabPool = data.enemyPrefabs1;
		}
		else if (5 < waveNumber && waveNumber <= 10)
		{
			prefabPool.AddRange(data.enemyPrefabs1);
			prefabPool.AddRange(data.enemyPrefabs2);
		}
		else
		{
			prefabPool = data.enemyPrefabs2;
		}
		return prefabPool;
	}

	private void StartBossIncoming()
	{
		OnQueueBossMessage ();
		Invoke ("SpawnBoss", bossSpawnDelay);
	}

	public GameObject SpawnEnemy(GameObject prefab, Vector3 pos)
	{
		GameObject o = Instantiate (prefab);
		o.transform.SetParent (transform);
		if (Random.value < 0.5f)
			o.transform.position = new Vector3 (Random.Range (0, 10), Map.size + 4);
		else
			o.transform.position = new Vector3 (Random.Range (0, 10), -4);

		Enemy e = o.GetComponentInChildren<Enemy> ();
		e.player = player.transform;
		e.moneyPickupPrefab = moneyPickup;
		e.Init (pos, map);
		e.OnEnemyDied += IncrementEnemiesKilled;
		e.OnEnemyObjectDisabled += RemoveEnemyFromEnemiesList;
		enemies.Add (e);

		EnemyHealthBar healthBar = enemyHealthBarPool.GetPooledObject ().GetComponent<EnemyHealthBar>();
		healthBar.Init (e);
		healthBar.GetComponent<UIFollow> ().Init(o.transform);
		healthBar.player = player;
		return o;
	}

	public GameObject SpawnEnemyForcePosition(GameObject prefab, Vector3 pos)
	{
		//print (pos);
		GameObject o = Instantiate (prefab);
		o.transform.SetParent (transform);
		o.transform.position = pos;

		Enemy e = o.GetComponentInChildren<Enemy> ();
		e.spawnMethod = Enemy.SpawnMethod.None;		// no spawn animation
		e.Init (pos, map);
		e.moneyPickupPrefab = moneyPickup;
		e.player = player.transform;
		enemies.Add (e);
		e.OnEnemyDied += IncrementEnemiesKilled;
		e.OnEnemyObjectDisabled += RemoveEnemyFromEnemiesList;

		EnemyHealthBar healthBar = enemyHealthBarPool.GetPooledObject ().GetComponent<EnemyHealthBar>();
		healthBar.Init (e);
		healthBar.GetComponent<UIFollow> ().Init(o.transform);
		healthBar.player = player;
		return o;
	}

	public void SpawnBoss()
	{
		bossSpawn.PlayAnimation ();
		GameObject o = Instantiate (data.bossPrefabs [Random.Range (0, data.bossPrefabs.Count)]);
		o.transform.SetParent (transform);

		Enemy e = o.GetComponentInChildren<Enemy> ();
		e.player = player.transform;
		e.Init (bossSpawn.transform.position, map);
		e.moneyPickupPrefab = moneyPickup;
		e.OnEnemyDied += IncrementEnemiesKilled;
		e.OnEnemyObjectDisabled += RemoveEnemyFromEnemiesList;

		bossHealthBar.Init (e);
		bossHealthBar.abilityIconBar.GetComponent<UIFollow> ().Init(o.transform, e.healthBarOffset);
		enemies.Add (e);
		bosses.Add ((BossEnemy)e);
	}

	private int NumAliveEnemies()
	{
		int count = 0;
		for (int i = enemies.Count - 1; i >= 0; i --)
		{
			Enemy e = enemies [i];
			// count alive enemies
			if (e.health > 0)
				count++;
		}
		//print (count);
		return count;
	}

	private void RemoveEnemyFromEnemiesList(Enemy e)
	{
		//Debug.Log ("Removed " + e);
		enemies.Remove (e);
	}

	private void IncrementEnemiesKilled()
	{
		enemiesKilled++;	
	}
}