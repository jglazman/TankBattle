

namespace Glazman.Tank
{
	/// <summary>
	/// Derive from this class if you want to use common Unity methods.
	/// </summary>
	public abstract class UnityBehaviour
	{
		public abstract int Priority { get; }

		public UnityBehaviour()
		{
			UnityWrapper.Register(this);
		}
		
		public virtual void OnApplicationFocus(bool focus) { }
		public virtual void Update(float deltaTime) { }
		public virtual void FixedUpdate(float deltaTime) { }
		public virtual void LateUpdate(float deltaTime) { }
		public virtual void OnDestroy() { }
	}
}
