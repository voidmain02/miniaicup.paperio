using System;
using Newtonsoft.Json.Linq;

namespace MiniAiCup.Paperio.Client
{
	public class Message
	{
		public MessageType Type { get; set; }

		public JObject Data { get; set; }

		public static Message Load(string input)
		{
			var jMessage = JObject.Parse(input);
			return new Message {
				Type = ParseType((string)jMessage["type"]),
				Data = (JObject)jMessage["params"]
			};
		}

		private static MessageType ParseType(string type)
		{
			switch (type)
			{
				case "start_game": return MessageType.StartGame;
				case "tick" : return MessageType.Tick;
				case "end_game" : return MessageType.EndGame;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}
