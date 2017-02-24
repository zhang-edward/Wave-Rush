﻿using UnityEngine;
using System.Collections;

public abstract class EnemyStatus : MonoBehaviour
{
	[Header("Base EnemyStatus Variables")]
	public string statusName;
	public bool permanent;
	public float duration;

	protected Enemy enemy;
	protected float timer;

	public virtual void Init(Enemy enemy)
	{
		this.enemy = enemy;
		timer = duration;
		if (gameObject.activeInHierarchy)
			StartCoroutine ("Effect");
	}

	void Update()
	{
		timer -= Time.deltaTime;
	}

	protected abstract IEnumerator Effect ();

	public virtual void Stack()
	{}
}
