/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System;
using System.Collections.Generic;

using Decider.Csp.BaseTypes;
using Decider.Csp.Integer;
using Decider.Csp.Global;

namespace Decider.Example.SendMoreMoney
{
	public static class SendMoreMoney
	{
		public static void Main()
		{
			var s = new VariableInteger<int>("s", 0, 9);
			var e = new VariableInteger<int>("e", 0, 9);
			var n = new VariableInteger<int>("n", 0, 9);
			var d = new VariableInteger<int>("d", 0, 9);
			var m = new VariableInteger<int>("m", 1, 9);
			var o = new VariableInteger<int>("o", 0, 9);
			var r = new VariableInteger<int>("r", 0, 9);
			var y = new VariableInteger<int>("y", 0, 9);
			var c0 = new VariableInteger<int>("c0", 0, 1);
			var c1 = new VariableInteger<int>("c1", 0, 1);
			var c2 = new VariableInteger<int>("c2", 0, 1);
			var c3 = new VariableInteger<int>("c3", 0, 1);

			var constraints = new List<IConstraint>
				{
					new AllDifferentInteger<int>(new [] { s, e, n, d, m, o, r, y }),
					new ConstraintInteger<int>(d + e == (10 * c0) + y),
					new ConstraintInteger<int>(n + r + c0 == (10 * c1) + e),
					new ConstraintInteger<int>(e + o + c1 == (10 * c2) + n),
					new ConstraintInteger<int>(s + m + c2 == (10 * c3) + o),
					new ConstraintInteger<int>(c3 == m)
				};

			var variables = new [] { c0, c1, c2, c3, s, e, n, d, m, o, r, y };
			var state = new StateInteger<int>(variables, constraints);

			if (state.Search() == StateOperationResult.Unsatisfiable)
				throw new ApplicationException("Cannot find solution to the SEND + MORE = MONEY problem.");

			Console.WriteLine($"Runtime:\t{state.Runtime}\nBacktracks:\t{state.Backtracks}\n");

			Console.WriteLine($"    {s} {e} {n} {d} ");
			Console.WriteLine($"  + {m} {o} {r} {e} ");
			Console.WriteLine("  ---------");
			Console.WriteLine($"  {m} {o} {n} {e} {y} ");
		}
	}
}
