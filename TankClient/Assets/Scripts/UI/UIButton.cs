using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Tank
{
	public class UIButton : Button
	{
		private SelectionState _state = SelectionState.Disabled;

		protected override void Start()
		{
			base.Start();
			
			SetStateNormal();
		}
		
		public void SetStateNormal()
		{
			SetStateInternal(SelectionState.Normal);
		}

		public void SetStateHighlighted()
		{
			SetStateInternal(SelectionState.Highlighted);
		}

		public void SetStatePressed()
		{
			SetStateInternal(SelectionState.Pressed);
		}

		public void SetStateDisabled()
		{
			SetStateInternal(SelectionState.Disabled);
		}

		private void SetStateInternal(SelectionState state)
		{
			if (state != _state)
			{
				_state = state;
				DoStateTransition(state, false);
			}
		}
	}
}
