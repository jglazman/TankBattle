using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	public class TankModelBehaviour : MonoBehaviour
	{
		[SerializeField] private Renderer[] _renderers;
		[SerializeField] private Transform _body;
		[SerializeField] private Transform _turret;

		private AgentModule _agent;
		
		public void SetAgent(AgentModule agent)
		{
			_agent = agent;
		}
		
		public void SetColor(Color color)
		{
			foreach (var rend in _renderers)
			{
				// TODO: should use sharedMaterial with optimized assets
				if (rend != null)
					rend.material.color = color;
			}
		}

		private void LateUpdate()
		{
			if (_agent == null)
				return;

			// turret always points in the direction we are facing
			if (_turret != null)
				_turret.rotation = _agent.transform.rotation;

			// body always points in the direction we are moving
			var velocity = _agent.Velocity;
			if (velocity.sqrMagnitude > 0.1f)
			{
				var v = velocity.normalized;
				_desiredFacing = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;

				// TODO: this slightly improves animations by avoiding silly wraparound lerps, but a proper animation system would be better
				if (Mathf.Abs(_desiredFacing - _facing) > 180f)
					_desiredFacing *= -1f;
			}

			_facing = Mathf.Lerp(_facing, _desiredFacing, Time.deltaTime * 4f);

			if (_body != null)
				_body.rotation = Quaternion.Euler(0f, _facing, 0f);
		}

		private float _desiredFacing = 0f;
		private float _facing = 0f;
	}
}
