
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

		/// <summary>A list of module types that this module depends on.</summary>
		public abstract ModuleType[] Dependencies { get; }


		private Entity _myEntity = null;
		protected Entity MyEntity => _myEntity;

		/// <summary>Our parent entity.</summary>
		public Entity Entity { get { return _myEntity; } }


		/// <summary>
		/// Called immediately after we are added to our parent entity
		/// and before we are linked to any dependencies.
		/// </summary>
		public void Initialize(Entity entity)
		{
			_myEntity = entity;
			
			InitializeInternal();
		}

		protected virtual void InitializeInternal() { }
		
		
		/// <summary>
		/// Called after all dependencies have been linked.
		/// </summary>
		public virtual void Start() { }

		
		/// <summary>
		/// Create a weak reference between this module and one of our dependencies.
		/// Throws an exception if the implementing class ignores the given module dependency.
		/// </summary>
		public virtual void LinkToDependency(Module dependency)
		{
			throw new System.ArgumentException($"Tried to link module {this.ModuleType} to unsupported dependency: {dependency.ModuleType}");
		}

		
		public static bool DrawDebug = false;
	}

}
