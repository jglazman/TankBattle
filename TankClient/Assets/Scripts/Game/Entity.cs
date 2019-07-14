using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Glazman.Tank
{
	public class Entity
	{
		private List<Module> _modules = new List<Module>(4);

		
		public Entity() { }

		public Entity(params Module[] modules)
		{
			foreach (var mod in modules)
				AddModule(mod);
		}


		/// <summary>
		/// Cleanup and invalidate all modules of this entity.
		/// </summary>
		public void Destroy()
		{
			if (_modules.Count > 0)
			{
				for (int i = _modules.Count; i >= 0; i--)
					_modules[i].Destroy();

				_modules.Clear();
			}
		}

		/// <summary>
		/// Add the given module to ourself as a component, using our previously added modules as dependencies.
		/// </summary>
		public void AddModule(Module module)
		{
			if (module == null)
				throw new ArgumentException("Tried to add a null module to an entity!");

			var moduleTypes = (ModuleType[])Enum.GetValues(typeof(ModuleType));
			foreach (var type in moduleTypes)
			{
				if (module.IsModuleType(type) && _modules.Exists(m => m.IsModuleType(type)))
					throw new ArgumentException($"Entity already has a module of the given type: {type}");
			}

			module.Initialize(this);

			var dependencyTypes = module.Dependencies;
			if (dependencyTypes != null)
			{
				for (int i = 0; i < dependencyTypes.Length; i++)
				{
					var depType = dependencyTypes[i];
					
					if (module.IsModuleType(depType))
						throw new Exception($"A module cannot depend on itself: {depType}");
					
					var dependencyModule = _modules.FirstOrDefault(m => m.IsModuleType(depType));
					if (dependencyModule == null)
						throw new Exception($"Missing module dependency: {depType}");

					module.LinkToDependency(dependencyModule);
				}
			}

			_modules.Add(module);
		}

		public Module GetModule(ModuleType moduleType)
		{
			return _modules.FirstOrDefault(m => m.IsModuleType(moduleType));
		}
		
		public T GetModule<T>(ModuleType moduleType) where T : Module
		{
			return GetModule(moduleType) as T;
		}
		
		public bool TryGetModule(ModuleType moduleType, out Module module)
		{
			module = GetModule(moduleType);
			return module != null;
		}

		public bool TryGetModule<T>(ModuleType moduleType, out T module) where T : Module
		{
			module = GetModule<T>(moduleType);
			return module != null;
		}
	}
}
