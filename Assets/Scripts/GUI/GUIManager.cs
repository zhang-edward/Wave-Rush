﻿using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {

	// hud
	[Header("GUI")]
	public GameObject gameUI;
	public EnemyWaveText enemyWaveText;

	// game over panel
	[Header("Game Over Panel")]
	public GameObject gameOverUI;
	// score report in game over panel
	public ScoreReport scorePanel;

	[Header("Data")]
	public EnemyManager enemyManager;
	public Player player;

	void Awake()
	{
	}

	void OnEnable()
	{
		player.OnPlayerDied += GameOverUI;
		enemyManager.OnEnemyWaveSpawned += ShowEnemyWaveText;
		enemyManager.OnBossIncoming += ShowBossIncomingText;
	}

	void OnDisabled()
	{
		player.OnPlayerDied -= GameOverUI;
		enemyManager.OnEnemyWaveSpawned -= ShowEnemyWaveText;
		enemyManager.OnBossIncoming -= ShowBossIncomingText;

	}

	private void GameOverUI()
	{
		Invoke ("InitGameOverUI", 1.0f);
	}

	private void InitGameOverUI()
	{
		gameUI.SetActive (false);
		gameOverUI.GetComponent<Animator> ().SetTrigger ("In");
		gameOverUI.SetActive (true);
		Invoke("ReportScore", 0.5f);	
	}

	private void ReportScore()
	{
		scorePanel.ReportScore (enemyManager.enemiesKilled, enemyManager.waveNumber - 1);
	}

	private void ShowEnemyWaveText(int waveNumber)
	{
		if (waveNumber > 1)
			enemyWaveText.DisplayWaveComplete ();
		enemyWaveText.DisplayWaveNumberAfterDelay (waveNumber);
	}

	private void ShowBossIncomingText()
	{
		enemyWaveText.DisplayBossIncoming ();
	}
} 
