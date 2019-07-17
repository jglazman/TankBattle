using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	/// <summary>
	/// User input control over an agent.
	/// </summary>
	public class UserAgentModule : Module
	{
		public override int Priority { get { return ModulePriority.UserAgent; } }

		protected override ModuleType ModuleType { get { return ModuleType.UserAgent; } }

		public override ModuleType[] Dependencies { get { return new[] { ModuleType.Agent }; } }

		private AgentModule _agent;

		private float _speed = 0f;
		private float _facing = 0f;
		private Vector3 _moving = Vector3.zero;

		protected override void InitializeInternal()
		{
			GameUI.ListenForMessages(HandleUIMessage);
		}

		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Agent))
			{
				_agent = dependency as AgentModule;
			}
		}
		
		public override void Update(float deltaTime)
		{
			if (_agent == null)
				return;
			
			_agent.SetDesiredDirection(_moving);
			_agent.SetDesiredFacing(_facing);
			_agent.SetDesiredSpeed(_speed * GameConfig.PLAYER_TANK_SPEED);
		}

		public override void LateUpdate(float deltaTime)
		{
			// reset for next frame
			_moving = Vector2.zero;
		}
		
		
		private void HandleUIMessage(UIMessage message)
		{
			switch (message.type)
			{
				case UIMessage.MessageType.MoveUp:
					_moving.x = 0f;
					_moving.z = 1f;
					_speed = 1f;
					break;

				case UIMessage.MessageType.MoveDown:
					_moving.x = 0f;
					_moving.z = -1f;
					_speed = 1f;
					break;

				case UIMessage.MessageType.MoveLeft:
					_moving.x = -1f;
					_moving.z = 0f;
					_speed = 1f;
					break;

				case UIMessage.MessageType.MoveRight:
					_moving.x = 1f;
					_moving.z = 0f;
					_speed = 1f;
					break;
				
				case UIMessage.MessageType.LookUp:
					_facing = 0f;
					break;

				case UIMessage.MessageType.LookDown:
					_facing = 180f;
					break;

				case UIMessage.MessageType.LookLeft:
					_facing = 270f;
					break;

				case UIMessage.MessageType.LookRight:
					_facing = 90f;
					break;
				
				case UIMessage.MessageType.Shoot:
					var pos = _agent.transform.position + (Vector3.up * 0.5f) + (_agent.transform.forward * 0.3f);
					var vel = _agent.transform.forward * GameConfig.BULLET_SPEED;
					var bullet = EntityFactory.CreateBullet("Bullet", "Bullet", pos, vel);
					break;
			}
		}

	}
}
