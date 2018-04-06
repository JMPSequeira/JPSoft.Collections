using System;

namespace JPSoft.Collections.Generics
{
	public class DuplicateEntryException : ArgumentException
	{
		public DuplicateEntryException(string message, string paramName) : base(message, paramName) { }
	}
}