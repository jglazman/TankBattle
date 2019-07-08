﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Glazman.Tank
{
	public class Entity : UnityBehaviour
	{
		public override int Priority { get { return 0; } }


		// @todo this dictionary provides quick lookups but limits each entity to a single module of each type (1:1);
		// a 1:N structure would probably allow for much more interesting behaviors, but could prohibitively increase
		// the complexity of interactions between entities.
		private Dictionary<ModuleType, Module> _modulesMap = new Dictionary<ModuleType, Module>();

		/// <summary>Our module instances.</summary>
		private List<Module> _modules = new List<Module>(4);

		
		/// <summary>Add the given modules to ourself as a component, using our previously added modules as dependencies.</summary>
		public void AddModule(Module module)
		{
			if (_modulesMap.ContainsKey(module.ModuleType))
				throw new ArgumentException($"Entity already has a module of the given type: {module.ModuleType}");
			
			module.Initialize(this);

			var dependencyTypes = module.Dependencies;
			if (dependencyTypes != null)
			{
				for (int i = 0; i < dependencyTypes.Length; i++)
				{
					if (dependencyTypes[i] == module.ModuleType)
						throw new Exception($"A module cannot depend on itself: {module.ModuleType}");
					
					Module dependency;
					if (!_modulesMap.TryGetValue(dependencyTypes[i], out dependency))
						throw new Exception($"Missing module dependency: {module.ModuleType} depends on {dependencyTypes[i]}");

					module.LinkToDependency(dependency);
				}
			}

			_modules.Add(module);
			_modulesMap.Add(module.ModuleType, module);
		}

		public bool TryGetModule(ModuleType moduleType, out Module module)
		{
			return _modulesMap.TryGetValue(moduleType, out module);
		}

		public T TryGetModule<T>(ModuleType moduleType) where T : Module
		{
			Module module;
			if (TryGetModule(moduleType, out module))
				return module as T;

			return default(T);
		}


		public override void OnDestroy()
		{
			_modules.Clear();
			_modulesMap.Clear();

			base.OnDestroy();
		}
	}

}
