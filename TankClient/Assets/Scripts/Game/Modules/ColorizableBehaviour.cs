﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	public class ColorizableBehaviour : MonoBehaviour
	{
		[SerializeField] private Renderer[] _renderers;

		public void SetColor(Color color)
		{
			foreach (var rend in _renderers)
			{
				if (rend != null)
					rend.material.color = color;
			}
		}
	}
}
