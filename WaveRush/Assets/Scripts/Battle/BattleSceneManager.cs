﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages overall Battle Scene state
/// </summary>
public class BattleSceneManager : MonoBehaviour
{
	public static BattleSceneManager instance;
	GameManager gm;

	public Map map;
	public EnemyManager enemyManager;
	public Player player;
	public GUIManager gui;
	public DialogueView dialogueView;

	public List<Pawn> acquiredPawns { get; private set; }   // pawns acquired this session
	public int moneyEarned { get; private set; } 			// money earned in this session
	public int soulsEarned { get; private set; }            // souls earned in this session

	public delegate void BattleSceneEvent();
	public BattleSceneEvent OnStageCompleted;

	void Awake()
	{
		// Make this a singleton
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this.gameObject);

		acquiredPawns = new List<Pawn>();
		gm = GameManager.instance;
		gm.OnSceneLoaded += Init;
		player.OnPlayerDied += UpdateData;
	}

	// Init main game environment
	private void Init()
	{
		StartCoroutine(InitRoutine());
	}

	private IEnumerator InitRoutine()
	{
		// Get data from GameManager
		Pawn pawn = gm.selectedPawn;
		StageData stage = gm.GetStage(gm.selectedSeriesIndex, gm.selectedStageIndex);

		// Initialize components
		print("Map:" + map);
		map.chosenMap = stage.mapType;
		map.GenerateMap();
		player.Init(pawn);

		// Do dialogue before starting the game
		if (stage.dialogueSets.Length > 0)
			dialogueView.Init(stage.dialogueSets);
		while (dialogueView.dialoguePlaying)
			yield return null;
		
		enemyManager.Init(stage);
		gui.DisplayIntroMessage();

		gm.OnSceneLoaded -= Init;   // Remove the listener because it is only run once per scene
	}

	private void UpdateData()
	{
		ScoreReport.ScoreReportData data = new ScoreReport.ScoreReportData(
			enemiesDefeated: 	enemyManager.enemiesKilled,
			wavesSurvived: 		Mathf.Max(enemyManager.waveNumber - 1, 0),
			maxCombo: 			player.hero.maxCombo,
			money: 				gm.wallet.money,
			moneyEarned: 		moneyEarned,
			souls: 				gm.wallet.souls,
			soulsEarned:		soulsEarned
		);
		gui.GameOverUI(data);

		if (enemyManager.IsStageComplete())
		{
			if (OnStageCompleted != null)
				OnStageCompleted();
			if (IsPlayerOnLatestStage())
			{
				gm.UnlockNextStage();
			}
		}

		gm.saveGame.RemovePawn(gm.selectedPawn.id);
		foreach(Pawn pawn in acquiredPawns)
		{
			gm.saveGame.AddPawn(pawn);
		}

		int enemiesDefeated = enemyManager.enemiesKilled;
		int wavesSurvived = enemyManager.waveNumber;
		int maxCombo = player.hero.maxCombo;

		gm.wallet.AddMoney(moneyEarned);
		gm.wallet.AddSouls(soulsEarned);
		gm.UpdateScores(enemiesDefeated, wavesSurvived, maxCombo);
	}

	private bool IsPlayerOnLatestStage()
	{
		return (gm.selectedStageIndex == gm.saveGame.latestUnlockedStageIndex &&
				gm.selectedSeriesIndex == gm.saveGame.latestUnlockedSeriesIndex);
	}

	public void AddPawn(Pawn pawn)
	{
		acquiredPawns.Add(pawn);
	}

	public void AddMoney(int amt)
	{
		moneyEarned += amt;
		gui.UpdateMoney(moneyEarned);
	}

	public void AddSouls(int amt)
	{
		soulsEarned += amt;
		gui.UpdateSouls(soulsEarned);
	}
}
