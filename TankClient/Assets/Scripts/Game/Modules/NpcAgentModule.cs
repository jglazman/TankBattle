using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Glazman.Tank
{
	[Serializable]
	public class NpcAgentModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.NpcAgent; } }

		public override ModuleType[] Dependencies { get { return new[] { ModuleType.Agent, ModuleType.Pathfinding, ModuleType.Health, ModuleType.Collision }; } }


		private enum State
		{
			Idle,
			Pathfinding,
			AvoidingObstacle,
			Dead
		}
		
		private AgentModule _agent;
		private PathfindingModule _pathfinding;
		private HealthModule _health;

		private State _state = State.Idle;
		private float _speed = 0f;
		private float _facing = 0f;
		private Vector3 _direction = Vector3.zero;
		private List<Vector3> _path = null;
		private int _pathIndex = -1;
		private float _timeInState;
		private float _timeOfAttention;
		private float _timeUntilNextShot;
		private Team _team;

		public Team Team => _team;
		
		private List<Entity> _bullets = new List<Entity>();
		

		public NpcAgentModule(Team team)
		{
			_team = team;
			_timeUntilNextShot = 5f + UnityEngine.Random.value * 10f;
		}
		
		protected override void InitializeInternal()
		{
			SetState(State.Idle);
		}
		
		protected override void DestroyInternal()
		{
			foreach (var bullet in _bullets)
				bullet?.Destroy();
			_bullets.Clear();
		}


		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Agent))
			{
				_agent = dependency as AgentModule;
			}
			
			if (dependency.IsModuleType(ModuleType.Pathfinding))
			{
				_pathfinding = dependency as PathfindingModule;
			}
			
			if (dependency.IsModuleType(ModuleType.Health))
			{
				_health = dependency as HealthModule;
				_health.OnHealthChanged += OnHealthChanged;
			}
			
			if (dependency.IsModuleType(ModuleType.Collision))
			{
				(dependency as CollisionModule).collision.SetTriggerCallback(OnTrigger);
			}
		}
		
		
		private void OnTrigger(CollisionTag tag, Entity otherEntity)
		{
			if (tag == CollisionTag.Bullet)
			{
				var bullet = otherEntity.GetModule<BulletModule>(ModuleType.Bullet);
				if (bullet != null && bullet.Team != _team)
					_health.ChangeHealth(-1);
				
				otherEntity.Destroy();
			}
		}

		private void OnHealthChanged(Entity e, HealthModule h, int delta)
		{
			if (h.HitPoints <= 0)
			{
//				this.entity.Destroy();	// TODO: boom.
				
				SetState(State.Dead);
			}
		}

		
		public override void Update(float deltaTime)
		{
			if (_agent == null || _state == State.Dead)
				return;

			_timeInState += deltaTime;

			switch (_state)
			{
				case State.Idle:
					UpdateIdle(deltaTime);
					break;
				
				case State.Pathfinding:
					UpdatePathfinding(deltaTime);
					break;
				
				case State.AvoidingObstacle:
					UpdateAvoidingObstacle(deltaTime);
					break;
			}

			UpdateTurret(deltaTime);
			
			_agent.SetDesiredDirection(_direction);
			_agent.SetDesiredFacing(_facing);
			_agent.SetDesiredSpeed(_speed * GameConfig.ENEMY_TANK_SPEED);
		}

		public override void LateUpdate(float deltaTime)
		{
			// reset for next frame
			_direction = Vector2.zero;
		}


		private void SetState(State state)
		{
			_state = state;
			_timeInState = 0f;
			_timeOfAttention = 0f;
			_path = null;
			_pathIndex = -1;

			switch (state)
			{
				case State.Idle:
				{
					_timeOfAttention = 1f; // + UnityEngine.Random.value * 3f;
					_speed = 0f;
				} break;

				case State.Pathfinding:
				{
					_timeOfAttention = 5f + UnityEngine.Random.value * 10f;
					
					var randomOpenTiles = Game.TerrainEntities.
						Select(t => t.GetModule<TerrainModule>(ModuleType.Terrain)).
						Where(t => t.IsOpen()).ToArray();
					
					var tile = randomOpenTiles[UnityEngine.Random.Range(0,randomOpenTiles.Length)];
					if (tile != null)
						_path = _pathfinding.GetPathToTile(tile.X, tile.Y);

					if (_path?.Count > 1)
					{
						_speed = 1f;
						_pathIndex = 0;
					}
					else
					{
						SetState(State.Idle);
					}
				} break;
				
				case State.AvoidingObstacle:
				{
					_timeOfAttention = 1f;
					_speed = 1f;
				} break;

				case State.Dead:
				{
					if (_agent != null)
						_agent.Disable();
				} break;
			}
		}

		private void UpdateIdle(float deltaTime)
		{
			if (_timeInState > _timeOfAttention)
				SetState(State.Pathfinding);
		}

		private void UpdatePathfinding(float deltaTime)
		{
			if (_timeInState > _timeOfAttention)
			{
				SetState(State.Idle);
			}

			if (_pathIndex < 0 || _path == null || _pathIndex >= _path.Count)
			{
				SetState(State.Idle);
				return;
			}
			
			var myPos = _agent.transform.position;
			var waypointPos = _path[_pathIndex];
			var dif = myPos - waypointPos;

			if (dif.sqrMagnitude < 0.05f)
			{
				_pathIndex++;
				if (_pathIndex >= _path.Count)
				{
					SetState(State.Idle);
					return;
				}
				
				waypointPos = _path[_pathIndex];
				dif = myPos - waypointPos;
			}
			
			if (Mathf.Abs(dif.x) > Mathf.Abs(dif.z))
			{
				_facing = dif.x > 0f ? 270f : 90f;
				_direction.x = dif.x > 0f ? -1f : 1f;
				_direction.z = 0f;
			}
			else
			{
				_direction.x = 0f;
				_direction.z = dif.z > 0f ? -1f : 1f;
				_facing = dif.z > 0f ? 180f : 0f;
			}
		}

		private void UpdateAvoidingObstacle(float deltaTime)
		{
			// TODO: bounce off, turn around, etc.
			if (_timeInState > _timeOfAttention)
				SetState(State.Pathfinding);
		}

		private void UpdateTurret(float deltaTime)
		{
			_timeUntilNextShot -= deltaTime;
			if (_timeUntilNextShot > 0f)
				return;

			_timeUntilNextShot = 1f + UnityEngine.Random.value * 10f;
			
			var pos = _agent.transform.position + (Vector3.up * 0.5f) + (_agent.transform.forward * 0.3f);
			var vel = _agent.transform.forward * GameConfig.BULLET_SPEED;
			var bullet = EntityFactory.CreateBullet("Bullet", "Bullet", pos, vel, _team);
			_bullets.Add(bullet);
		}		
	}
}
