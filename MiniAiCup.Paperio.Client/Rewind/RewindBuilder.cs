using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Client.Rewind
{
	public class RewindBuilder
	{
		private readonly List<RewindCommand> _commands = new List<RewindCommand>();

		public void Add(RewindCommand command)
		{
			_commands.Add(command);
		}

		public void AddRange(IEnumerable<RewindCommand> commands)
		{
			_commands.AddRange(commands);
		}

		public override string ToString()
		{
			var sCommands = _commands.Select(c => c.Serialize());
			return $"[{String.Join(",", sCommands)}]";
		}
	}
}
