﻿
using System.Text;
using UnityEngine;

namespace Glazman.Tank
{
	public enum ModuleType
	{
		Agent,
		GameObject,
		NpcAgent,
		Pathfinding,
		Prefab,
		UserAgent,
		Transform
	}

	public static class ModulePriority
	{
		public const int Default = 0;
		public const int Agent = 1;
	}

	public abstract class Module : UnityBehaviour
	{
		/// <summary>Indentifies which type of module we were instantiated as.</summary>
		public abstract ModuleType ModuleType { get; }

		/// <summary>An ordered list of module types that this module depends on (in dependency order).</summary>
		public abstract ModuleType[] Dependencies { get; }


		private Entity _myEntity = null;
		public Entity entity => _myEntity;


		/// <summary>
		/// Called immediately after we are added to our parent entity
		/// and before we are linked to any dependencies.
		/// </summary>
		public void Initialize(Entity e)
		{
			_myEntity = e;
			InitializeInternal();
		}

		protected virtual void InitializeInternal() { }


		/// <summary>
		/// Call this to destroy all modules and invalidate this entity.
		/// </summary>
		public void Destroy()
		{
			DestroyInternal();
			_myEntity = null;
		}

		protected virtual void DestroyInternal() { }

		
		/// <summary>
		/// Create a weak reference between this module and one of our dependencies.
		/// Throws an exception if the implementing class ignores the given module dependency.
		/// </summary>
		public virtual void LinkToDependency(Module dependency)
		{
			throw new System.ArgumentException($"Tried to link module {this.ModuleType} to unsupported dependency: {dependency.ModuleType}");
		}



		public override string ToString()
		{
			var sb = new StringBuilder("Module(");

			var moduleTypes = (ModuleType[])System.Enum.GetValues(typeof(ModuleType));
			foreach (var type in moduleTypes)
			{
				if (IsModuleType(type))
					sb.Append($"{type.ToString()}|");
			}

			sb.Append(")");
			return sb.ToString();
		}

		
		public static bool DrawDebug = true;
	}

}
