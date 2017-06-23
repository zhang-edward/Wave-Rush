﻿using UnityEngine;
using UnityEngine.UI;

public class StageIcon : MonoBehaviour
{
	public delegate void OnClicked(GameObject obj);
	public OnClicked onClicked;

	public Text stageNameText;
	public Text stageLevelText;
	public Button highlightButton;      // the button that the user presses to expand the highlight menu
	public GameObject highlight;		// the shiny border around the window
	public GameObject highlightMenu;    // a menu with description and play button

	public int index;					// used for the placeholder in StageSelectView

	void Start()
	{
		highlightButton.onClick.AddListener(() => OnClick());
	}

	public void Init(StageData stage, int index)
	{
		stageNameText.text = stage.stageName;
		stageLevelText.text = stage.level.ToString();
		this.index = index;
	}

	private void OnClick()
	{
		if (onClicked != null)
			onClicked(this.gameObject);
	}

	public void ExpandHighlightMenu()
	{
		highlightMenu.SetActive(true);
	}

	public void CollapseHighlightMenu()
	{
		highlightMenu.SetActive(false);
	}
}