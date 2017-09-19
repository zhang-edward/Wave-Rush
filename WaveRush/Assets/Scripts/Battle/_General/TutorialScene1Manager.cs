﻿using UnityEngine;
using System.Collections;

public class TutorialScene1Manager : MonoBehaviour
{
	private const float TASK_DELAY_INTERVAL = 1.0f;

	public static TutorialScene1Manager instance;
	private GameManager gm;
	[Header("Game")]
	public Map map;
	public EnemyManager enemyManager;
	public Player player;
	public SimpleAnimationPlayer knightCharacter;
	[Header("UI")]
	public GUIManager gui;
	public DialogueView dialogueView;
	public AbilityIconBar abilitiesBar;
	public TutorialTaskView tutorialTaskView;
	public Animator controlPointer;
	[Header("Data")]
	public DialogueSet[] dialogueSteps;
	public GameObject trainingDummyPrefab;
	public GameObject attackingDummyPrefab;
	public AudioClip taskCompleteSound;

	public MapType mapType;

	private KnightHero knight;
	private PlayerHero.InputAction storedOnSwipe, storedOnTap;

	private int knightRushCount;
	private int knightShieldCount;
	private int parryCount;
	private bool playerActivatedSpecial;

	void Awake()
	{
		// Make this a singleton
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this.gameObject);

		gm = GameManager.instance;
		gm.OnSceneLoaded += Init;
	}

	// Init main game environment
	private void Init()
	{
		StartCoroutine(TutorialScene());
	}

	private void Restart()
	{
		
	}

	private IEnumerator TutorialScene()
	{
		// Get data from GameManager
		Pawn pawn = gm.selectedPawn;
		SoundManager sound = SoundManager.instance;
		CameraControl cam = CameraControl.instance;

		// Initialize components
		map.chosenMap = mapType;
		map.GenerateMap();
		player.Init(pawn);
		knight = (KnightHero)player.hero;
		enemyManager.level = 1;

		gm.OnSceneLoaded -= Init;   // Remove the listener because it is only run once per scene


		// Step -2: Swipe
		DisableTap();
		DisableSpecialAbility();
		controlPointer.gameObject.SetActive(true);
		controlPointer.CrossFade("Swipe", 0f);
		tutorialTaskView.Init("Swipe to use your Rush Ability", false);

		knight.OnKnightRush += IncrementRushCount;
		while (knightRushCount < 1)
			yield return null;
		tutorialTaskView.SetCompleted(true);
		sound.PlayUISound(taskCompleteSound);
		yield return new WaitForSeconds(TASK_DELAY_INTERVAL);
		controlPointer.gameObject.SetActive(false);
		knightRushCount = 0;

		// Step -1: Learn swipe controls
		tutorialTaskView.Init("Use your Rush Ability 3 times (0/3)", false);
		abilitiesBar.abilityIcons[0].FlashHighlight(Color.white);

		while (knightRushCount < 3)
		{
			tutorialTaskView.SetText(string.Format("Use your Rush Ability 3 times ({0}/3)", knightRushCount));
			yield return null;
		}
		tutorialTaskView.Init("Use your Rush Ability 3 times (3/3)", true);
		sound.PlayUISound(taskCompleteSound);
		yield return new WaitForSeconds(TASK_DELAY_INTERVAL);
		knightRushCount = 0;
		abilitiesBar.abilityIcons[0].StopFlashHighlight();
		knight.OnKnightRush -= IncrementRushCount;

		// Step 0: Learn tap controls
		PlayKnightCharDialogue(0);
		yield return new WaitUntil(() => !dialogueView.dialoguePlaying);
		cam.ResetFocus();

		tutorialTaskView.Init("Use your Shield Ability 2 times (0/2)", false);
		controlPointer.gameObject.SetActive(true);
		controlPointer.CrossFade("Tap", 0f);
		abilitiesBar.abilityIcons[1].FlashHighlight(Color.white);
		EnableTap();

		knight.OnKnightShield += IncrementShieldCount;
		while (knightShieldCount < 2)
		{
			tutorialTaskView.SetText(string.Format("Use your Shield Ability 2 times ({0}/2)", knightShieldCount));
			yield return null;
		}
		tutorialTaskView.Init("Use your Shield Ability 2 times (2/2)", true);
		sound.PlayUISound(taskCompleteSound);
		yield return new WaitForSeconds(TASK_DELAY_INTERVAL);
		abilitiesBar.abilityIcons[1].StopFlashHighlight();
		controlPointer.gameObject.SetActive(false);
		knightShieldCount = 0;
		knight.OnKnightShield -= IncrementShieldCount;

		// Step 1: Attacking a dummy
		PlayKnightCharDialogue(1);
		yield return new WaitUntil(() => !dialogueView.dialoguePlaying);
		cam.ResetFocus();

		tutorialTaskView.Init("Destroy the dummy", false);
		GameObject trainingDummy = enemyManager.SpawnEnemy(trainingDummyPrefab, map.CenterPosition);
		while (trainingDummy.gameObject.activeInHierarchy)
			yield return null;
		tutorialTaskView.SetCompleted(true);
		sound.PlayUISound(taskCompleteSound);
		yield return new WaitForSeconds(TASK_DELAY_INTERVAL);

		// Step 2: Learn the parry
		PlayKnightCharDialogue(2);
		yield return new WaitUntil(() => !dialogueView.dialoguePlaying);
		cam.ResetFocus();

		tutorialTaskView.Init("Parry 3 times (0/3)", false);
		Enemy attackingDummy = enemyManager.SpawnEnemy(attackingDummyPrefab, map.CenterPosition + (Vector3.right * 2f)).GetComponentInChildren<Enemy>();
		attackingDummy.invincible = true;
		knight.onParry += IncrementParryCount;
		while (parryCount < 3)
		{
			tutorialTaskView.SetText(string.Format("Parry 3 times ({0}/3)", parryCount));
			yield return null;
		}
		tutorialTaskView.Init("Parry 3 times (3/3)", true);
		sound.PlayUISound(taskCompleteSound);
		yield return new WaitForSeconds(TASK_DELAY_INTERVAL);
		attackingDummy.invincible = false;
		attackingDummy.GetComponentInChildren<Enemy>().Damage(999);
		parryCount = 0;
		knight.onParry -= IncrementParryCount;

		// Step 3: Finish tutorial
		PlayKnightCharDialogue(3);
		yield return new WaitUntil(() => !dialogueView.dialoguePlaying);

		enemyManager.SpawnEndPortal();
	}

	private void PlayKnightCharDialogue(int step)
	{
		CameraControl.instance.SetFocus(knightCharacter.transform);
		knightCharacter.Play();
		dialogueView.Init(dialogueSteps[step]);
	}

	private void DisableSwipe()
	{
		storedOnSwipe = player.hero.onSwipe;
		player.hero.onSwipe = null;
	}

	private void EnableSwipe()
	{
		player.hero.onSwipe = storedOnSwipe;
	}

	private void DisableTap()
	{
		storedOnTap = player.hero.onTap;
		player.hero.onTap = null;
	}

	private void EnableTap()
	{
		player.hero.onTap = storedOnTap;
	}

	private void DisableSpecialAbility()
	{
		abilitiesBar.specialAbilityIcon.gameObject.SetActive(false);
	}

	private void EnableSpecialAbility()
	{
		abilitiesBar.specialAbilityIcon.gameObject.SetActive(true);
	}

	private void IncrementRushCount()
	{
		knightRushCount++;
	}

	private void IncrementShieldCount()
	{
		knightShieldCount++;
	}

	private void IncrementParryCount()
	{
		parryCount++;
	}
}