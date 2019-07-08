
namespace Glazman.Tank
{
	public enum ModuleType
	{
		Agent,
		GameObject,
		Model,
		NpcAgent,
		UserAgent,
		Appearance,
		Pathfinding
	}

	public abstract class Module : UnityBehaviour
	{
		/// <summary>Indentifies which type of module we were instantiated as.</summary>
		public abstract ModuleType ModuleType { get; }

		
		private ModuleType[] _myDependencies = null;

		/// <summary>A list of module types that this module depends on.</summary>
		public ModuleType[] Dependencies { get { return _myDependencies; } }

		
		private Entity _myEntity = null;

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
