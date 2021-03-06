﻿using UnityEngine;
using System.Collections;

public class UIAnimatorControl : MonoBehaviour
{
	private Animator anim;
	public bool animateOnStart = true;
	public UIAnimatorControl[] childAnimators;

	void Awake() {
		anim = GetComponent<Animator>();
		if (!animateOnStart)
			anim.Play(anim.GetCurrentAnimatorStateInfo(-1).fullPathHash, -1, 1);
	}

	public void OnEnable() {
		foreach (UIAnimatorControl anim in childAnimators) {
			anim.gameObject.SetActive(true);
		}
	}

	public void AnimateOut()
	{
		StartCoroutine(AnimateOutRoutine());
		foreach(UIAnimatorControl anim in childAnimators) {
			anim.AnimateOut();
		}
	}

	private IEnumerator AnimateOutRoutine()
	{
		anim.CrossFade("Out", 0);
		yield return new WaitForEndOfFrame();
		while (anim.GetCurrentAnimatorStateInfo(0).IsName("Out"))
			yield return null;
		gameObject.SetActive(false);
	}
}