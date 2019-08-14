using System.Collections.Generic;
using System.Linq;
using MiniAiCup.Paperio.Core;
using NUnit.Framework;

namespace MiniAiCup.Paperio.Tests
{
	public class TerritoryCapturerTests
	{
		private readonly BfsTerritoryCapturer _capturer = new BfsTerritoryCapturer();

		static TerritoryCapturerTests()
		{
			Game.Initialize();
		}

		[Test]
		public void SimpleCaptureTest()
		{
			/*
			 * +---------------------+
			 * |                     |
			 * |                     |
			 * |                     |
			 * |       + + + +       |
			 * |       +     +       |
			 * |       +     +       |
			 * |       X X X X       |
			 * |       X X X X       |
			 * |                     |
			 * |                     |
			 * +---------------------+
			 */

			var territory = new[] {
				new Point(5, 5),
				new Point(6, 5),
				new Point(7, 5),
				new Point(8, 5),
				new Point(5, 6),
				new Point(6, 6),
				new Point(7, 6),
				new Point(8, 6)
			};

			var tail = new[] {
				new Point(5, 7),
				new Point(5, 8),
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(8, 9),
				new Point(8, 8),
				new Point(8, 7),
				new Point(8, 6)
			};

			var expectedCapture = new[] {
				new Point(5, 7),
				new Point(6, 7),
				new Point(7, 7),
				new Point(8, 7),
				new Point(5, 8),
				new Point(6, 8),
				new Point(7, 8),
				new Point(8, 8),
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(8, 9)
			};

			var factCapture = _capturer.Capture(new PointsSet(territory), new Path(tail));

			Assert.IsTrue(AreEquals(factCapture, expectedCapture));
		}

		[Test]
		public void SeparateTerritoryTest()
		{
			/*
			 * +---------------------+
			 * |                     |
			 * |                     |
			 * |                     |
			 * |       + + + + +     |
			 * |       +       +     |
			 * |       +       +     |
			 * |       X X   X X     |
			 * |       X X   X X     |
			 * |                     |
			 * |                     |
			 * +---------------------+
			 */

			var territory = new[] {
				new Point(5, 5),
				new Point(6, 5),
				new Point(8, 5),
				new Point(9, 5),
				new Point(5, 6),
				new Point(6, 6),
				new Point(8, 6),
				new Point(9, 6)
			};

			var tail = new[] {
				new Point(5, 7),
				new Point(5, 8),
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(8, 9),
				new Point(9, 9),
				new Point(9, 8),
				new Point(9, 7),
				new Point(9, 6)
			};

			var expectedCapture = new[] {
				new Point(5, 7),
				new Point(5, 8),
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(8, 9),
				new Point(9, 9),
				new Point(9, 8),
				new Point(9, 7)
			};

			var factCapture = _capturer.Capture(new PointsSet(territory), new Path(tail));

			Assert.IsTrue(AreEquals(factCapture, expectedCapture));
		}

		[Test]
		public void AroundTerritoryTest()
		{
			/*
			 * +---------------------+
			 * |                     |
			 * |                     |
			 * |                     |
			 * |                     |
			 * |                     |
			 * |     + + + + + +     |
			 * |     + X X X X +     |
			 * |     + X X X X +     |
			 * |                     |
			 * |                     |
			 * +---------------------+
			 */

			var territory = new[] {
				new Point(5, 5),
				new Point(6, 5),
				new Point(7, 5),
				new Point(8, 5),
				new Point(5, 6),
				new Point(6, 6),
				new Point(7, 6),
				new Point(8, 6)
			};

			var tail = new[] {
				new Point(4, 5),
				new Point(4, 6),
				new Point(4, 7),
				new Point(5, 7),
				new Point(6, 7),
				new Point(7, 7),
				new Point(8, 7),
				new Point(9, 7),
				new Point(9, 6),
				new Point(9, 5),
				new Point(8, 5)
			};

			var expectedCapture = new[] {
				new Point(4, 5),
				new Point(4, 6),
				new Point(4, 7),
				new Point(5, 7),
				new Point(6, 7),
				new Point(7, 7),
				new Point(8, 7),
				new Point(9, 7),
				new Point(9, 6),
				new Point(9, 5)
			};

			var factCapture = _capturer.Capture(new PointsSet(territory), new Path(tail));

			Assert.IsTrue(AreEquals(factCapture, expectedCapture));
		}

		[Test]
		public void TailVoidTest()
		{
			/*
			 * +---------------------+
			 * |                     |
			 * |       + + +         |
			 * |       +   +         |
			 * |       + + +         |
			 * |       + +           |
			 * |       + + + +       |
			 * |       +     +       |
			 * |     X X X X X X     |
			 * |     X X X X X X     |
			 * |                     |
			 * +---------------------+
			 */

			var territory = new[] {
				new Point(4, 4),
				new Point(5, 4),
				new Point(6, 4),
				new Point(7, 4),
				new Point(8, 4),
				new Point(9, 4),
				new Point(4, 5),
				new Point(5, 5),
				new Point(6, 5),
				new Point(7, 5),
				new Point(8, 5),
				new Point(9, 5)
			};

			var tail = new[] {
				new Point(5, 6),
				new Point(5, 7),
				new Point(5, 8),
				new Point(5, 9),
				new Point(5, 10),
				new Point(5, 11),
				new Point(6, 11),
				new Point(7, 11),
				new Point(7, 10),
				new Point(7, 9),
				new Point(6, 9),
				new Point(6, 8),
				new Point(6, 7),
				new Point(7, 7),
				new Point(8, 7),
				new Point(8, 6),
				new Point(8, 5)
			};

			var expectedCapture = new[] {
				new Point(5, 11),
				new Point(6, 11),
				new Point(7, 11),
				new Point(5, 10),
				new Point(6, 10),
				new Point(7, 10),
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(5, 8),
				new Point(6, 8),
				new Point(5, 7),
				new Point(6, 7),
				new Point(7, 7),
				new Point(8, 7),
				new Point(5, 6),
				new Point(6, 6),
				new Point(7, 6),
				new Point(8, 6)
			};

			var factCapture = _capturer.Capture(new PointsSet(territory), new Path(tail));

			Assert.IsTrue(AreEquals(factCapture, expectedCapture));
		}

		[Test]
		public void SeparateTailVoidTest()
		{
			/*
			 * +---------------------+
			 * |                     |
			 * |       + + + +       |
			 * |       +     +       |
			 * |       + + + +       |
			 * |         + +         |
			 * |     + + + + + +     |
			 * |     +         +     |
			 * |   X X X     X X X   |
			 * |   X X X     X X X   |
			 * |                     |
			 * +---------------------+
			 */

			var territory = new[] {
				new Point(3, 4),
				new Point(4, 4),
				new Point(5, 4),
				new Point(3, 5),
				new Point(4, 5),
				new Point(5, 5),
				new Point(8, 4),
				new Point(9, 4),
				new Point(10, 4),
				new Point(8, 5),
				new Point(9, 5),
				new Point(10, 5)
			};

			var tail = new[] {
				new Point(4, 6),
				new Point(4, 7),
				new Point(5, 7),
				new Point(6, 7),
				new Point(6, 8),
				new Point(6, 9),
				new Point(5, 9),
				new Point(5, 10),
				new Point(5, 11),
				new Point(6, 11),
				new Point(7, 11),
				new Point(8, 11),
				new Point(8, 10),
				new Point(8, 9),
				new Point(7, 9),
				new Point(7, 8),
				new Point(7, 7),
				new Point(8, 7),
				new Point(9, 7),
				new Point(9, 6),
				new Point(9, 5)
			};

			var expectedCapture = new[] {
				new Point(5, 11),
				new Point(6, 11),
				new Point(7, 11),
				new Point(8, 11),
				new Point(5, 10),
				new Point(6, 10),
				new Point(7, 10),
				new Point(8, 10),
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(8, 9),
				new Point(6, 8),
				new Point(7, 8),
				new Point(4, 7),
				new Point(5, 7),
				new Point(6, 7),
				new Point(7, 7),
				new Point(8, 7),
				new Point(9, 7),
				new Point(4, 6),
				new Point(9, 6)
			};

			var factCapture = _capturer.Capture(new PointsSet(territory), new Path(tail));

			Assert.IsTrue(AreEquals(factCapture, expectedCapture));
		}

		[Test]
		public void MultipleTailVoidsTest()
		{
			/*
			 * +-----------------------------+
			 * |                             |
 * 			 * |                       X X   |
			 * |         + + + + + + + X X   |
			 * |         +     + +     X X   |
			 * |         + + + + + +         |
			 * |   X X     + +     +         |
			 * |   X X + + + + + + +         |
			 * |   X X                       |
			 * |                             |
			 * +-----------------------------+
			 */

			var territory = new[] {
				new Point(1, 1),
				new Point(2, 1),
				new Point(1, 2),
				new Point(2, 2),
				new Point(1, 3),
				new Point(2, 3),
				new Point(11, 5),
				new Point(12, 5),
				new Point(11, 6),
				new Point(12, 6),
				new Point(11, 7),
				new Point(12, 7)
			};

			var tail = new[] {
				new Point(3, 2),
				new Point(4, 2),
				new Point(5, 2),
				new Point(5, 3),
				new Point(5, 4),
				new Point(4, 4),
				new Point(4, 5),
				new Point(4, 6),
				new Point(5, 6),
				new Point(6, 6),
				new Point(7, 6),
				new Point(7, 5),
				new Point(7, 4),
				new Point(6, 4),
				new Point(6, 3),
				new Point(6, 2),
				new Point(7, 2),
				new Point(8, 2),
				new Point(9, 2),
				new Point(9, 3),
				new Point(9, 4),
				new Point(8, 4),
				new Point(8, 5),
				new Point(8, 6),
				new Point(9, 6),
				new Point(10, 6),
				new Point(11, 6)
			};

			var expectedCapture = new[] {
				new Point(4, 6),
				new Point(5, 6),
				new Point(6, 6),
				new Point(7, 6),
				new Point(8, 6),
				new Point(9, 6),
				new Point(10, 6),
				new Point(4, 5),
				new Point(5, 5),
				new Point(6, 5),
				new Point(7, 5),
				new Point(8, 5),
				new Point(4, 4),
				new Point(5, 4),
				new Point(6, 4),
				new Point(7, 4),
				new Point(8, 4),
				new Point(9, 4),
				new Point(5, 3),
				new Point(6, 3),
				new Point(7, 3),
				new Point(8, 3),
				new Point(9, 3),
				new Point(3, 2),
				new Point(4, 2),
				new Point(5, 2),
				new Point(6, 2),
				new Point(7, 2),
				new Point(8, 2),
				new Point(9, 2)
			};

			var factCapture = _capturer.Capture(new PointsSet(territory), new Path(tail));

			Assert.IsTrue(AreEquals(factCapture, expectedCapture));
		}

		[Test]
		public void MultipleVoidsBetweenTerritoryAndTailTest()
		{
			/*
			 * +---------------------+
			 * |                     |
			 * |                     |
			 * |                     |
			 * |       + + + +       |
			 * |       +     +       |
			 * |       +     + + +   |
			 * |       X X X X   +   |
			 * |       X X X X + +   |
			 * |                     |
			 * |                     |
			 * +---------------------+
			 */

			var territory = new[] {
				new Point(5, 5),
				new Point(6, 5),
				new Point(7, 5),
				new Point(8, 5),
				new Point(5, 6),
				new Point(6, 6),
				new Point(7, 6),
				new Point(8, 6)
			};

			var tail = new[] {
				new Point(5, 7),
				new Point(5, 8),
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(8, 9),
				new Point(8, 8),
				new Point(8, 7),
				new Point(9, 7),
				new Point(10, 7),
				new Point(10, 6),
				new Point(10, 5),
				new Point(9, 5),
				new Point(8, 5)
			};

			var expectedCapture = new[] {
				
				new Point(5, 9),
				new Point(6, 9),
				new Point(7, 9),
				new Point(8, 9),
				new Point(5, 8),
				new Point(6, 8),
				new Point(7, 8),
				new Point(8, 8),
				new Point(5, 7),
				new Point(6, 7),
				new Point(7, 7),
				new Point(8, 7),
				new Point(9, 7),
				new Point(10, 7),
				new Point(9, 6),
				new Point(10, 6),
				new Point(9, 5),
				new Point(10, 5)
			};

			var factCapture = _capturer.Capture(new PointsSet(territory), new Path(tail));

			Assert.IsTrue(AreEquals(factCapture, expectedCapture));
		}

		private static bool AreEquals(IEnumerable<Point> fact, IEnumerable<Point> expected)
		{
			var orderedFact = fact.OrderBy(p => p.Y*GameParams.MapSize.Height + p.X).ToList();
			var orderedExpected = expected.OrderBy(p => p.Y*GameParams.MapSize.Height + p.X).ToList();

			return orderedFact.SequenceEqual(orderedExpected);
		}
	}
}
