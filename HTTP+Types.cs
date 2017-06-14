using System;
using System.Collections.Generic;

namespace naithar
{

	public partial class HTTP
	{

		public class Array : List<object>
		{

			public Array() : base()
			{
			}

			public Array(IEnumerable<object> list) : base()
			{
				this.AddRange(list);
			}
		}

		public class Dictionary : Dictionary<object, object>
		{

			public Dictionary() : base()
			{
			}

			public Dictionary(IDictionary<object, object> dictionary) : base()
			{
				this.AddRange(dictionary);
			}
		}

		public class Parameters : Dictionary<string, object>
		{
		}

		public class Headers : Dictionary<string, IEnumerable<string>>
		{

		}
	}


}