using UnityEngine;
using System.Collections;

public class HeroIconsView : ScrollViewSnap {

	public GameObject leftArrow, rightArrow;

	protected override void InitContent ()
	{
		base.InitContent ();
		UpdateArrowVisibility();

		SaveGame.HeroSaveData[] unlockedHeroes = GameManager.instance.saveGame.heroData;
		for (int i = 0; i < content.Length; i ++)
		{
			HeroIcon heroIcon = content [i].GetComponent<HeroIcon> ();
			heroIcon.Init(unlockedHeroes[i].unlocked);
		}
	}

	public override void EndDrag()
	{
		base.EndDrag();
		UpdateArrowVisibility();
	}

	private void UpdateArrowVisibility()
	{
		leftArrow.gameObject.SetActive(selectedContentIndex > 0);
		rightArrow.gameObject.SetActive(selectedContentIndex < content.Length - 1);

		if (leftArrow.gameObject.activeInHierarchy)
			leftArrow.GetComponent<SimpleAnimationPlayerImage>().Play();
		if (rightArrow.gameObject.activeInHierarchy)
			rightArrow.GetComponent<SimpleAnimationPlayerImage>().Play();
	}
}
