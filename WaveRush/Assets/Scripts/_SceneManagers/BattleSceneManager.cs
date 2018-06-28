﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages overall Battle Scene state
/// </summary>
public class BattleSceneManager : MonoBehaviour
{
	public struct PawnMetaData {
		public Pawn startingState;
		public int id;
	}

	public static BattleSceneManager instance;
	private GameManager gm;

	public Map map;
	public EnemyManager enemyManager;
	public Player player;
	public GUIManager gui;
	[Header("UI")]
public StageEndMenu losePanel;
	public DialogueView dialogueView;
	public GameObject stageCompleteOptions;
	public ModalSelectionView stageCompleteModal;
	public Button exitButton;
	public Button continueButton;
	public GameObject pauseButton;

	public int moneyEarned { get; private set; } 			// money earned in this session
	public int soulsEarned { get; private set; }            // souls earned in this session
	public bool leaveOrContinueOptionOpen { get; private set; }

	//private int[] pawnIds;
	//private Pawn[] startingPawnStates;
	private PawnMetaData[] pawnMetaData;

	public delegate void BattleSceneEvent();
	public BattleSceneEvent OnStageCompleted;

	void Awake()
	{
		// Make this a singleton
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this.gameObject);

		// Do BattleSceneManager-specific initialization which has no external dependencies
		//acquiredPawns = new List<Pawn>();
		gm = GameManager.instance;
		enemyManager.OnStageCompleted += () => { StartCoroutine(StageCompleteRoutine()); };
		player.OnPlayerDied += () => { UpdateData(false); };
		//gm.OnSceneLoaded += Init;
	}

	void Start() {
		Init();
	}

	// Init main game environment
	private void Init() {
		StartCoroutine(InitRoutine());
	}

	private IEnumerator InitRoutine()
	{
		// Get data from GameManager
		Pawn[] pawns = gm.selectedPawns;
		StageData stage = gm.GetStage(gm.selectedSeriesIndex, gm.selectedStageIndex);

		pawnMetaData = new PawnMetaData[pawns.Length];		
		for (int i = 0; i < pawns.Length; i ++) {
			pawnMetaData[i].id = pawns[i].Id;
			pawnMetaData[i].startingState = new Pawn(pawns[i]);
		}

		// Initialize components
		map.chosenMap = stage.mapType;
		map.GenerateMap();
		player.SetParty(pawns);
		player.SetHero(0);
		gui.partyView.Init(pawns);

		// Do dialogue before starting the game
		if (stage.dialogueSets.Length > 0)
			dialogueView.Init(stage.dialogueSets);
		player.GetComponent<PlayerInput>().enabled = false;
		gui.enemyWaveText.gameObject.SetActive(false);
		while (dialogueView.dialoguePlaying)
			yield return null;

		SoundManager.instance.PlayMusicLoop(map.data.musicLoop, map.data.musicIntro);
		player.GetComponent<PlayerInput>().enabled = true;
		gui.enemyWaveText.gameObject.SetActive(true);
		enemyManager.Init(stage);
		gui.DisplayIntroMessage();

		//gm.OnSceneLoaded -= Init;   // Remove the listener because it is only run once per scene
	}

	private IEnumerator StageCompleteRoutine()
	{
		enemyManager.paused = true;
		// Delay before showing end dialogue
		yield return new WaitForSecondsRealtime(1.0f);
		stageCompleteOptions.SetActive(true);
		leaveOrContinueOptionOpen = true;
		// Modal View Selection
		int selection = -1;
		stageCompleteModal.OnOptionSelected += (s) => {
			selection = s;
		};
		while (leaveOrContinueOptionOpen)
		{
			pauseButton.gameObject.SetActive(false);
			// "Leave" option selected
			if (selection == 1)
			{
				player.transform.parent.gameObject.SetActive(false);
				stageCompleteOptions.GetComponent<UIAnimatorControl>().AnimateOut();
				UpdateData(true);
				yield break;
			}
			// "Continue" option selected
			if (selection == 0)
			{
				leaveOrContinueOptionOpen = false;
			}
			yield return null;
		}
		pauseButton.gameObject.SetActive(true);
		enemyManager.paused = false;
		stageCompleteOptions.GetComponent<UIAnimatorControl>().AnimateOut();
		//print("No sacrifice detected; continue");
	}

	private void UpdateData(bool completedStage)
	{
		// Collect all money and souls
		List<GameObject> moneyPickups = ObjectPooler.GetObjectPooler(Enemy.POOL_MONEY).GetAllActiveObjects();
		List<GameObject> soulPickups = ObjectPooler.GetObjectPooler(BossEnemy.POOL_SOULS).GetAllActiveObjects();
		foreach (GameObject o in moneyPickups) {
			AddMoney(o.GetComponent<MoneyPickup>().value);
		}
		foreach (GameObject o in soulPickups) {
			AddSouls(1);
		}
		// Money report data
		ScoreReport.ScoreReportData scoreData = new ScoreReport.ScoreReportData(
			money: 				gm.save.money,
			moneyEarned: 		moneyEarned,
			souls: 				gm.save.souls,
			soulsEarned:		soulsEarned
		);

		HeroExpMenu.HeroExpMenuData[] expData = new HeroExpMenu.HeroExpMenuData[pawnMetaData.Length];
		// Add or subtract experience from pawns
		if (completedStage) {
			int stagesCompleted = enemyManager.waveNumber / enemyManager.stageData.goalWave;
			int gainedExperience = (int)(Formulas.ExperienceFormula(enemyManager.level) * 0.3f * stagesCompleted * enemyManager.stageData.maxPartySize / pawnMetaData.Length);
			for (int i = 0; i < pawnMetaData.Length; i ++) {
				gm.save.AddExperience(pawnMetaData[i].id, gainedExperience);
			}
		}
		else {
			for (int i = 0; i < pawnMetaData.Length; i ++) {
				int lostExperience = (int)(Formulas.ExperienceFormula(gm.save.GetPawn(pawnMetaData[i].id).level) * 0.2f);
				gm.save.LoseExperience(pawnMetaData[i].id, lostExperience);
			}
		}
		for (int i = 0; i < pawnMetaData.Length; i ++) {
			expData[i] = new HeroExpMenu.HeroExpMenuData(
				startState: pawnMetaData[i].startingState,
				endState: 	gm.save.GetPawn(pawnMetaData[i].id)
			);
		}

		gui.gameOverUI.SetActive(true);
		losePanel.Init(scoreData, expData, gm.GetStage(gm.selectedSeriesIndex, gm.selectedStageIndex).stageName);
	
		if (enemyManager.isStageComplete) {
			if (OnStageCompleted != null)
				OnStageCompleted();
			if (IsPlayerOnLatestStage()) {
				gm.UnlockNextStage();
			}
		}

		int enemiesDefeated = enemyManager.enemiesKilled;
		int wavesSurvived = enemyManager.waveNumber;
		int maxCombo = player.hero.maxCombo;

		gm.save.AddMoney(moneyEarned);
		gm.save.AddSouls(soulsEarned);
		gm.UpdateScores(enemiesDefeated, wavesSurvived, maxCombo);
	}

	private bool IsPlayerOnLatestStage()
	{
		return (gm.selectedStageIndex  == gm.save.LatestStageIndex &&
		        gm.selectedSeriesIndex == gm.save.LatestSeriesIndex);
	}

	//public void AddPawn(Pawn pawn)
	//{
	//	acquiredPawns.Add(pawn);
	//}

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

	// DEBUG
	public void DebugCompleteStage() {
		StartCoroutine(StageCompleteRoutine());
	}
}
