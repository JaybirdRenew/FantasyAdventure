using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unitTestAdventure.ConsoleRedirect
{
	/// <summary>
	/// Helper class used in tests of Console output. Created by Dan Muldrew.
	/// </summary>
	internal class ConsoleRedirector : IDisposable
	{
		private StringWriter _consoleOutput = new StringWriter();
		private StringReader _consoleInput;

        public ConsoleRedirector()
		{
			Console.SetOut(_consoleOutput);
		}
		public void Dispose()
		{
			Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
			Console.SetIn(new StreamReader(Console.OpenStandardInput()));
			this._consoleOutput.Dispose();
			this._consoleInput.Dispose();
		}
		public void SetUserInput(string str)
		{
			_consoleInput = new StringReader(str);
			Console.SetIn(_consoleInput);
		}
		public override string ToString()
		{
			return this._consoleOutput.ToString().Trim('\r', '\n');
		}
	}
}
