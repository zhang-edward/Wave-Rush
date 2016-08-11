﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AbilityIcon : MonoBehaviour
{
	public Image image { get; private set; }
	public RectTransform cooldownMask;

	void Awake()
	{
		image = GetComponent<Image> ();
	}

	public void SetCooldown(float percent)
	{
		if (percent < 0)
			return;
		cooldownMask.sizeDelta = new Vector2 (16, percent * 16);
	}
}

