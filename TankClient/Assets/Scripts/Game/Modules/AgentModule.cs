using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;


namespace Glazman.Tank
{
	/// <summary>
	/// A CharacterController and functionality to control its movement.
	/// </summary>
	public sealed class AgentModule : Module
	{
		public override int Priority { get { return ModulePriority.Agent; } }

		public override ModuleType ModuleType { get { return ModuleType.Agent; } }

		public override ModuleType[] Dependencies { get { return null; } }


		private AgentBehaviour _agentBehaviour;

		private Transform MyTransform => _agentBehaviour.transform;
		private GameObject MyGameObject => _agentBehaviour.gameObject;
		private CharacterController MyController => _agentBehaviour.controller;
		
		public AgentModule(string prefabName, Vector3 worldPosition)
		{
			var prefab = Resources.Load<AgentBehaviour>(prefabName);
			if (prefab == null)
				throw new Exception($"AgentBehaviour prefab not found in Resources: {prefabName}");
			
			_agentBehaviour = GameObject.Instantiate<AgentBehaviour>(prefab);
		}
		
		public override void LinkToDependency(Module dependency)
		{
			switch (dependency.ModuleType)
			{
				case ModuleType.Transform:
					(dependency as TransformModule).AttachToParent(MyTransform, Vector3.zero);
					break;
			}
		}

		public override void OnControllerColliderHit(ControllerColliderHit hit)
		{
			ApplyFriction(hit.normal);
		}
		
		public override void FixedUpdate(float fixedDeltaTime)
		{
			StepFacing(fixedDeltaTime);
			StepVelocity(fixedDeltaTime);
			StepMovement(fixedDeltaTime);

			if (DrawDebug)
			{
				Debug.DrawLine(MyTransform.position + new Vector3(0f, 0f, 0f), MyTransform.position + _velocity + new Vector3(0f, 0f, 0f), new Color(0f, 1f, 0f));
				Debug.DrawLine(MyTransform.position + new Vector3(0f, 0.5f, 0f), MyTransform.position + _desiredDirection + new Vector3(0f, 0.5f, 0f), new Color(0f, 0.8f, 0f));
				//Debug.DrawLine( MyTransform.position + new Vector3( 0f, 1f, 0f ), MyTransform.position + _velocity + new Vector3( 0f, 1f, 0f ), new Color( 0f, 0.6f, 0f ) );
			}
		}

		
		

		/// <summary>Use CharacterController.Move() or SimpleMove()</summary>
		private bool _simpleMove = false;

		/// <summary>maximum speed we can turn around</summary>
		private float _turnSpeed = 5.0f;

		/// <summary>determines how quickly we start/stop</summary>
		private float _acceleration = 30.0f;

		/// <summary>simulated friction coefficient when we collide with other objects</summary>
		private float _friction = 0.3f;

		/// <summary>our current rotation</summary>
		private Quaternion _rotation = Quaternion.identity;

		/// <summary>our rotation from the previous update</summary>
		private Quaternion _prevRotation = Quaternion.identity;

		/// <summary>are we currently on the ground or not?</summary>
		private bool _isGrounded = false;

		/// <summary>collision mask</summary>
		private int _layerMask = 1 << LayerMask.NameToLayer("Default");

		/// <summary>trigger collider event callback</summary>
		private System.Action<Collider> _eventTriggerCallback = null;
		
		public void SetEventTriggerCallback(System.Action<Collider> callback)
		{
			_eventTriggerCallback = callback;
		}

		/// <summary>current velocity</summary>
		private Vector3 _velocity;

		public Vector3 Velocity { get { return _velocity; } }

		/// <summary>the direction we wish to move in</summary>
		private Vector3 _desiredDirection = Vector3.zero;

		public Vector3 DesiredDirection { get { return _desiredDirection; } }

		public void SetDesiredDirection(Vector3 dir)
		{
			_desiredDirection = dir;
		}

		/// <summary>the speed we wish to move at</summary>
		private float _desiredSpeed = 0f;

		public float DesiredSpeed { get { return _desiredSpeed; } }

		public void SetDesiredSpeed(float speed)
		{
			_desiredSpeed = speed;
		}

		/// <summary>the direction we wish we were facing</summary>
		private float _desiredFacingAngle = 0f;

		public float DesiredFacingAngle { get { return _desiredFacingAngle; } }

		public void SetDesiredFacing(float angle)
		{
			_desiredFacingAngle = angle;
		}

		private void ApplyFriction(Vector3 normal)
		{
			_velocity += normal * _velocity.magnitude * _friction;

			if (DrawDebug)
				Debug.DrawLine(MyTransform.position + new Vector3(0f, 1f, 0f), MyTransform.position + normal + new Vector3(0f, 1f, 0f), Color.white);
		}

		private void StepFacing(float deltaTime)
		{
//		if (GameModeManager.Mode == GameMode.Isometric)
//		{
//			// don't turn immediately in all cases, wait until we hit a threshold first
//			float turnAngle = Vector3.Angle(transform.forward, UserInputManager.CursorWorldPosition - transform.position);
//			float diffAngle = Mathf.Abs(DesiredFacingAngle - turnAngle);
//			Debug.Log ("  >> angle dif: " + diffAngle);
//			if (diffAngle <= 60.0f)
//			{
//				return;
//			}
//		}

			// since all actors in this game are permanently grounded, we can only turn around our Y-axis;
			// this is a useful design constraint in a networked environment because we only need to
			// sync a single float rather than an entire quaternion
			_rotation = Quaternion.Lerp(_rotation, Quaternion.Euler(0.0f, DesiredFacingAngle, 0.0f), _turnSpeed * deltaTime);

			// only apply if it actually changed
			if (_rotation != _prevRotation)
			{
				_prevRotation = _rotation;
				MyTransform.rotation = _rotation; // this is more expensive than you might expect
			}
		}

		private void StepVelocity(float deltaTime)
		{
			// calculate the direction we are trying to move in
			Vector3 desiredVelocity = DesiredDirection * DesiredSpeed;

			// calculate our desired change in XZ-velocity; y-axis is controlled by gravity
			Vector3 velocityDelta = desiredVelocity;
			velocityDelta.x -= _velocity.x;
			velocityDelta.z -= _velocity.z;

			float accel = _acceleration * deltaTime;
			float sqrAccel = accel * accel;
			float sqrVelocityDelta = velocityDelta.sqrMagnitude;

//		Debug.Log("  " + Time.time + " ======> DesiredDirection=" + DesiredDirection + "  DesiredSpeed=" + DesiredSpeed + "  desiredVelocity=" + desiredVelocity + "  _velocity=" + _velocity + "  velocityDelta=" + velocityDelta + "   sqrVelocityDelta=" + sqrVelocityDelta + "  accel=" + accel + "  sqrAccel=" + sqrAccel);

			// limit changes in velocity by our acceleration
			if (sqrVelocityDelta > sqrAccel)
			{
				velocityDelta = velocityDelta.normalized * accel;
//			Debug.Log(" velocityDelta CLAMPED=" + velocityDelta);
			}

			// apply delta
			_velocity += velocityDelta;

			// apply gravity
			_velocity.y += Physics.gravity.y * deltaTime * 4f;

//		Debug.Log("  new velocity=" + _velocity);
		}

		private void StepMovement(float deltaTime)
		{
			//Debug.Log( "velocity=" + Velocity );

			// calculate our step distance for this frame
			// NOTE: SimpleMove() takes speed, but Move() takes velocity
			Vector3 stepDistance = _velocity;
			if (!_simpleMove)
			{
				stepDistance *= deltaTime;

				// snap to the ground
				if (_isGrounded)
				{
					// CharacterController.stepOffset controls how high UP we can step but doesn't
					// snap us to the ground in a similar way when stepping DOWN, so we have to snap
					// ourselves to the ground here to keep from bouncing down inclines when using Move()
					if (Physics.Raycast(MyTransform.position, Vector3.down, MyController.stepOffset, _layerMask))
						stepDistance.y -= MyController.stepOffset;
				}
			}

			// take a step
			if (_simpleMove)
			{
				_isGrounded = MyController.SimpleMove(stepDistance);
			}
			else
			{
				CollisionFlags flags = MyController.Move(stepDistance);
				_isGrounded = ((flags & CollisionFlags.CollidedBelow) != 0);
			}

			if (_isGrounded)
			{
				// cancel our Y-axis movement so it doesn't build up while moving along the ground
				if (_velocity.y < 0)
					_velocity.y = 0;
			}

			//Vector3 xz = new Vector3( _velocity.x, 0f, _velocity.z ).normalized;
			//if ( Physics.Raycast( Transform.position, xz, ( MyAgent.Collider as SphereCollider ).radius, _layerMask ) )
			//	_velocity -= xz.normalized;
		}
	}
}
