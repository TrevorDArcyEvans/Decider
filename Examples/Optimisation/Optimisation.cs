/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System;
using System.Collections.Generic;

using Decider.Csp.BaseTypes;
using Decider.Csp.Integer;
using Decider.Csp.Global;

namespace Decider.Example.Optimisation
{
	public static class Optimisation
	{
		public static void Main()
		{
			var a = new VariableInteger<int>("a", 0, 9);
			var b = new VariableInteger<int>("b", 0, 9);
			var c = new VariableInteger<int>("c", 0, 9);
			var d = new VariableInteger<int>("d", 0, 9);
			var e = new VariableInteger<int>("e", 0, 9);
			var f = new VariableInteger<int>("f", 0, 9);
			var g = new VariableInteger<int>("g", 0, 9);
			var h = new VariableInteger<int>("h", 0, 9);
			var optimise = new VariableInteger<int>("optimise");

			var array = new ConstrainedArray<int>(new int[] { 60, 52, 52, 62, 35, 73, 47, 20, 87, 27 });

			var constraints = new List<IConstraint>
				{
					new AllDifferentInteger<int>(new [] { a, b, c, d }),
					new AllDifferentInteger<int>(new [] { e, f, g, h }),
					new ConstraintInteger<int>(a + b < 10),
					new ConstraintInteger<int>(c + d > 15),
					new ConstraintInteger<int>(h > e),
					new ConstraintInteger<int>(array[a] < 40),
					new ConstraintInteger<int>(optimise == a + b + c + d + e + f + g + h)
				};

			var variables = new[] { a, b, c, d, e, f, g, h, optimise };
			var state = new StateInteger<int>(variables, constraints);

			if (state.Search(optimise, 10) == StateOperationResult.Unsatisfiable)
				throw new ApplicationException("Cannot find a solution to constraint problem.");

			Console.WriteLine($"a: {state.OptimalSolution["a"]}");
			Console.WriteLine($"b: {state.OptimalSolution["b"]}");
			Console.WriteLine($"c: {state.OptimalSolution["c"]}");
			Console.WriteLine($"d: {state.OptimalSolution["d"]}");
			Console.WriteLine($"e: {state.OptimalSolution["e"]}");
			Console.WriteLine($"f: {state.OptimalSolution["f"]}");
			Console.WriteLine($"g: {state.OptimalSolution["g"]}");
			Console.WriteLine($"h: {state.OptimalSolution["h"]}\n");

			Console.WriteLine($"Optimised Variable: {state.OptimalSolution["optimise"]}\n");

			Console.WriteLine($"Runtime:\t{state.Runtime}\nBacktracks:\t{state.Backtracks}\n");
		}
	}
}
