﻿namespace PlayerActions
{
	using UnityEngine;

	[System.Serializable]
	public class PA_EffectAttached : PA_Effect
	{
		/** Set in Inspector */
		public SimpleAnimationPlayer anim;
		[SerializeField] private bool offsetMatchesFlipX = false;

		public override void Init(Player player)
		{
			base.Init(player);
		}

		protected override void DoAction()
		{
			if (anim == null)
				return;
			if (offsetMatchesFlipX)
			{
				int sign = offsetMatchesFlipX ? -1 : 1; // set the sign of the X position based on whether the player 
														// sprite is flipped or not
				anim.transform.localPosition = new Vector2(anim.transform.localPosition.x * sign, anim.transform.localPosition.y);
			}
			// Set the rotation for the effect
			if (rotationType != RotationType.Set)
				rotation = GetRotation();

			// Initialize the effect properties
			TempObjectInfo info = new TempObjectInfo();
			info.targetColor = color;
			if (duration < 0)
				duration = anim.anim.TimeLength;
			info.lifeTime = duration;
			info.fadeOutTime = 0.1f;

			// Initialize the effect and play it
			anim.GetComponent<TempObject>().Init(
				rotation,
				anim.transform.position,
				anim.anim.frames[0],
				info);
			anim.Play(anim.anim);
		}

		void OnDisable() {
			anim.gameObject.SetActive(false);
		}
	}
}
