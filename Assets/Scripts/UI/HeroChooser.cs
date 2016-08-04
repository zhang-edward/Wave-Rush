﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeroChooser : MonoBehaviour
{
	public ScrollViewSnap heroIconsView;
	public HeroInfoPanel infoPanel;
	public Button playButton;

	void Awake()
	{
	}

	// Use this for initialization
	void Start ()
	{
		UpdateHeroInfoPanel ();
	}

	void OnEnable()
	{
		heroIconsView.OnEndDrag += UpdateHeroInfoPanel;
	}

	void OnDisabled()
	{
		heroIconsView.OnEndDrag -= UpdateHeroInfoPanel;
	}

	private void UpdateHeroInfoPanel()
	{
		string hero = heroIconsView.SelectedContent.GetComponent<HeroInfoIcon> ().heroName;
		infoPanel.selectedHeroName = hero;
		infoPanel.DisplayHeroInfo ();
		GameManager.instance.selectedHero = hero;
	}
}
