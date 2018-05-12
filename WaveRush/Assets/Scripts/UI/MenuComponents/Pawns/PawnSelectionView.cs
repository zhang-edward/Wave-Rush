﻿using UnityEngine;
using System.Collections.Generic;

public class PawnSelectionView : MonoBehaviour {

	public GameObject pawnIconPrefab;
	public GameObject selectedContentPlaceholder;
	public Transform contentFolder;
	public Transform selectedContentFolder;
	public List<PawnIcon> pawnIcons { get; private set; }

	private Pawn[] pawns;
	private SaveModifier.PawnStateUpdate pawnStateUpdateEvent;
	private bool initialized;	// Variable to deal with the event assignment for pawnStateUpdateEvent

	void Awake() {
	}

	public void Init(Pawn[] pawns, SaveModifier.PawnStateUpdate pawnStateUpdateEvent) {
		this.pawns = pawns;
		this.pawnStateUpdateEvent = pawnStateUpdateEvent;

		pawnStateUpdateEvent += UpdatePawn;
		initialized = true;

		pawnIcons = new List<PawnIcon>();
		Refresh();
	}

	void OnEnable() {
		if (initialized) {
			pawnStateUpdateEvent += UpdatePawn;
		}
	}

	void OnDisable() {
		if (initialized) {
			pawnStateUpdateEvent -= UpdatePawn;
		}
	}

	private void UpdatePawn(int id) {
		// Find the pawnIcon UI object for the specified id and update it
		for (int i = 0; i < pawnIcons.Count; i ++) {
			PawnIcon icon = pawnIcons[i];
			if (icon.pawnData.id == id) {
				if (pawns[id] == null) {
					icon.gameObject.SetActive(false);
					return;
				}
				else {
					icon.gameObject.SetActive(true);
					icon.Init(pawns[id]);
					return;
				}
			}
		}
		// If we couldn't find a pawnIcon
		if (pawns[id] != null) {
			AddNewPawnIcon(pawns[id]);
		}
	}

	public void Refresh()
	{
		foreach (PawnIcon icon in pawnIcons)
			icon.gameObject.SetActive(false);
		int j = 0;									// Track the pawnIcons list position
		for (int i = 0; i < pawns.Length; i ++)		// Iterate through the master list of pawns (may contain holes)
		{
			Pawn pawn = pawns[i];
			if (pawn != null)
			{
				if (j >= pawnIcons.Count)			// If we need more pawn icons, add new ones to the list
				{
					AddNewPawnIcon(pawn);
				}
				else
				{
					pawnIcons[j].Init(pawn);        // If not, re-initialize the pawn icon
					pawnIcons[j].gameObject.SetActive(true);
				}
				j++;
			}
		}
		SortByHeroType();
	}

	/// <summary>
	/// Sorts the list of pawnIcons by the hero type, and also by level within each hero type.
	/// Also sets the sibling index so the order is rendered in any layout groups. Uses insertion sort.
	/// </summary>
	public void SortByHeroType()
	{
		// Insertion sort
		for (int i = 1; i < pawnIcons.Count; i ++) {
			PawnIcon x = pawnIcons[i];
			int j = i - 1;
			while (j >= 0 && CompareHeroAndLevel(pawnIcons[j].pawnData, x.pawnData) < 0) {
				pawnIcons[j + 1] = pawnIcons[j];
				j--;
			}
			pawnIcons[j + 1] = x;
		}

		// Set sibling index (hierarchy position)
		for (int i = 0; i < pawnIcons.Count; i ++) {
			pawnIcons[i].transform.SetSiblingIndex(i);
		}
	}

	/// <summary>
	/// Sorts the list of pawnIcons by level.
	/// Also sets the sibling index so the order is rendered in any layout groups. Uses insertion sort.
	/// </summary>
	public void SortByLevel()
	{
		for (int i = 1; i < pawnIcons.Count; i++)
		{
			PawnIcon x = pawnIcons[i];
			int j = i - 1;
			while (j >= 0 && 
			       pawnIcons[j].pawnData.level < x.pawnData.level && 
			       pawnIcons[j].pawnData.tier  < x.pawnData.tier)
			{
				pawnIcons[j + 1] = pawnIcons[j];
				j--;
			}
			pawnIcons[j + 1] = x;
		}
		for (int i = 0; i < pawnIcons.Count; i++)       // set the hierarchy position of each pawnIcon
		{
			pawnIcons[i].transform.SetSiblingIndex(i);
		}
	}

	public int IndexOfPawnIcon(PawnIcon icon) {
		return pawnIcons.IndexOf(icon);
	}


	// ==== Helper methods ====
	private void AddNewPawnIcon(Pawn pawn)
	{
		GameObject o = Instantiate(pawnIconPrefab);
		o.transform.SetParent(contentFolder, false);        // set physical position
		PawnIcon pawnIcon = o.GetComponent<PawnIcon>();
		pawnIcon.Init(pawn);                                // initialize PawnIcon data
		pawnIcons.Add(pawnIcon);                            // add this object to the list for future manipulation
	}

	/// <summary>
	/// Compares the hero type and level of two pawns.
	/// </summary>
	/// <returns> less than 0 if pawn1 has a lower level or heroType than pawn2, 0 if they are the same hero and level
	/// and greater than 0 if pawn1 has a higher level or heroType than pawn2. </returns>
	/// <param name="pawn1">Pawn1.</param>
	/// <param name="pawn2">Pawn2.</param>
	private int CompareHeroAndLevel(Pawn pawn1, Pawn pawn2) {
		if (pawn1.type == pawn2.type) {
			if (pawn1.level < pawn2.level) {
				return -1;
			}
			else if (pawn1.level > pawn2.level) {
				return 1;
			}
			else {
				return 0;
			}
		}
		else {
			return pawn1.type.CompareTo(pawn2.type);
		}
	}
}
