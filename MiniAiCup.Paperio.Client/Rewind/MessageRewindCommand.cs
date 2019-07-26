namespace MiniAiCup.Paperio.Client.Rewind
{
	public class MessageRewindCommand : RewindCommand
	{
		public string Text { get; set; }

		public override string Serialize()
		{
			return $"{{\"type\":\"message\",\"message\":\"{Text + "\\n"}\"}}";
		}
	}
}
