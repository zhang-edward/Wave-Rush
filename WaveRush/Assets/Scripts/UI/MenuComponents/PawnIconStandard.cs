﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PawnIconStandard : PawnIcon
{
	[Header("Card UI Elements")]
	public Text heroNameText;
	public Text heroLevelText;
	public Image heroPortrait;
	public Image heroPortraitBorder;
	public Image heroStars;

	public Button button;       // if it is interactable
	public delegate void Click(PawnIconStandard iconData);
	public Click onClick;

	public override void Init(Pawn pawnData)
	{
		base.Init(pawnData);
		// initialize display items
		heroNameText.text = pawnData.type.ToString();
		heroLevelText.text = "lv." + pawnData.level.ToString();

		// initialize button interactivity
		if (button != null)
			button.onClick.AddListener(() => OnClick());
	}

	private void OnClick()
	{
		if (onClick != null)
			onClick(this);
	}
}