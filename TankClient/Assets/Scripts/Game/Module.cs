
using System.Text;
using UnityEngine;

namespace Glazman.Tank
{
	[System.Flags]
	public enum ModuleType
	{
		Undefined	 = 0,
		Transform	 = 1 << 0,
		Agent		 = 1 << 1,
		Prefab		 = 1 << 2,
		UserAgent	 = 1 << 3,
		NpcAgent	 = 1 << 4,
		Pathfinding	 = 1 << 5,
		Terrain		 = 1 << 6,
		Bullet		 = 1 << 7,
		Collision	 = 1 << 8,
		Health		 = 1 << 9,
		Destructible = 1 << 10
	}

	public static class ModulePriority
	{
		public const int UserInput 		= -100;
		public const int Default		= 0;
		public const int UserAgent		= 10;
		public const int Agent 			= 11;
		public const int Bullet			= 20;
	}

	public abstract class Module : UnityBehaviour
	{
		/// <summary>Indentifies which type of module we were instantiated as.</summary>
		protected abstract ModuleType ModuleType { get; }

		public bool IsModuleType(ModuleType type)
		{
			return (ModuleType & type) != 0;
		}

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
			UnityWrapper.Unregister(this);
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
