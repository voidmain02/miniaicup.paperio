using System;
using System.Diagnostics;
using Microsoft.VisualStudio.DebuggerVisualizers;
using MiniAiCup.Paperio.Core;
using MiniAiCup.Paperio.DebuggerVisualizer;

[assembly: DebuggerVisualizer(typeof(GameStateVisualizer), Target=typeof(DebugStateView))]

namespace MiniAiCup.Paperio.DebuggerVisualizer
{
	public class GameStateVisualizer : DialogDebuggerVisualizer
	{
		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{
			if (windowService == null)
			{
				throw new ArgumentNullException(nameof(windowService));
			}

			if (objectProvider == null)
			{
				throw new ArgumentNullException(nameof(objectProvider));
			}

			var gameState = objectProvider.GetObject() as DebugStateView;
			using (var displayForm = new GameStateForm(gameState))
			{
				windowService.ShowDialog(displayForm);
			}
		}
	}
}
