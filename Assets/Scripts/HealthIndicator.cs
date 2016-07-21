﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UI
{
	public class HealthIndicator : MonoBehaviour
	{
		public bool empty { get; private set; }

		public Image image;
		public Sprite emptySprite;
		public Sprite fullSprite;

		public void SetEmpty()
		{
			empty = true;
			image.sprite = emptySprite;
		}

		public void SetFull()
		{
			empty = false;
			image.sprite = fullSprite;
		}
	}
}