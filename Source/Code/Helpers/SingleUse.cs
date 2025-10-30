namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public struct SingleUse<T> where T : struct
	{
		public SingleUse(T val)
		{
			Value = val;
		}

		public T? Value
		{
			get
			{
				T? value = field;
				field = null;
				return value;
			}
			set;
		}

		public static implicit operator SingleUse<T>(T? _) => new();

		public static implicit operator SingleUse<T>(T v) => new(v);

		public static implicit operator T?(SingleUse<T> s) => s.Value;
	}
}
