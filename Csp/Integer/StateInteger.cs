/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Decider.Csp.BaseTypes;

namespace Decider.Csp.Integer
{
	public class StateInteger<T> : IState<T>  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		public IList<IConstraint> Constraints { get; private set; }
		public IList<IVariable<T>> Variables { get; private set; }

		public int Depth { get; private set; }
		public int Backtracks { get; private set; }
		public TimeSpan Runtime { get; private set; }
		public IDictionary<string, IVariable<T>> OptimalSolution { get; private set; }
		public IList<IDictionary<string, IVariable<T>>> Solutions { get; private set; }

		private IVariable<T>[] LastSolution { get; set; }

		public StateInteger(IEnumerable<IVariable<T>> variables, IEnumerable<IConstraint> constraints)
		{
			SetVariables(variables);
			SetConstraints(constraints);
			this.Depth = 0;
			this.Backtracks = 0;
			this.Runtime = new TimeSpan(0);
			this.Solutions = new List<IDictionary<string, IVariable<T>>>();
		}

		public void SetVariables(IEnumerable<IVariable<T>> variableList)
		{
			this.Variables = variableList.ToList();

			foreach (var variable in this.Variables)
				variable.SetState(this);
		}

		public void SetConstraints(IEnumerable<IConstraint> constraints)
		{
			this.Constraints = constraints?.ToList() ?? new List<IConstraint>();
		}

		public StateOperationResult Search()
		{
			var unassignedVariables = this.LastSolution == null
				? new LinkedList<IVariable<T>>(this.Variables)
				: new LinkedList<IVariable<T>>();
			var instantiatedVariables = this.LastSolution ?? new IVariable<T>[this.Variables.Count];
			var stopwatch = Stopwatch.StartNew();
			var searchResult = StateOperationResult.Unsatisfiable;

			if (this.Depth == instantiatedVariables.Length)
			{
				--this.Depth;
				Backtrack(unassignedVariables, instantiatedVariables);
				++this.Depth;
			}
			else if (ConstraintsViolated())
			{
				this.Runtime += stopwatch.Elapsed;
				stopwatch.Stop();
				return searchResult;
			}

			if (Search(out searchResult, unassignedVariables, instantiatedVariables, ref stopwatch))
				this.Solutions.Add(CloneLastSolution());

			this.Runtime += stopwatch.Elapsed;
			stopwatch.Stop();
			return searchResult;
		}

		public StateOperationResult Search(IVariable<T> optimiseVar, int timeOut)
		{
			var unassignedVariables = this.LastSolution == null
				? new LinkedList<IVariable<T>>(this.Variables)
				: new LinkedList<IVariable<T>>();
			var instantiatedVariables = this.LastSolution ?? new IVariable<T>[this.Variables.Count];
			var stopwatch = Stopwatch.StartNew();
			var searchResult = StateOperationResult.Unsatisfiable;

			this.Constraints.Add(new ConstraintInteger<T>((VariableInteger<T>) optimiseVar > T.MinValue));

			while (true)
			{
				if (this.Depth == instantiatedVariables.Length)
				{
					--this.Depth;
					Backtrack(unassignedVariables, instantiatedVariables);
					++this.Depth;
				}
				else if (ConstraintsViolated())
					break;

				if (Search(out searchResult, unassignedVariables, instantiatedVariables, ref stopwatch, timeOut))
				{
					this.Constraints.RemoveAt(this.Constraints.Count - 1);
					this.Constraints.Add(new ConstraintInteger<T>((VariableInteger<T>) optimiseVar > optimiseVar.InstantiatedValue));
					this.OptimalSolution = CloneLastSolution();
				}
				else if (searchResult == StateOperationResult.TimedOut)
					break;
			}

			if (this.LastSolution != null && searchResult == StateOperationResult.Unsatisfiable)
				searchResult = StateOperationResult.Solved;

			this.Runtime += stopwatch.Elapsed;
			stopwatch.Stop();
			return searchResult;
		}

		public StateOperationResult SearchAllSolutions()
		{
			var unassignedVariables = this.LastSolution == null
				? new LinkedList<IVariable<T>>(this.Variables)
				: new LinkedList<IVariable<T>>();
			var instantiatedVariables = this.LastSolution ?? new IVariable<T>[this.Variables.Count];
			var stopwatch = Stopwatch.StartNew();

			var searchResult = StateOperationResult.Unsatisfiable;

			while (true)
			{
				if (this.Depth == -1)
					break;

				if (this.Depth == instantiatedVariables.Length)
				{
					--this.Depth;
					Backtrack(unassignedVariables, instantiatedVariables);
					++this.Depth;
				}
				else if (ConstraintsViolated())
				{
					this.Runtime += stopwatch.Elapsed;
					stopwatch.Stop();
					break;
				}

				if (Search(out searchResult, unassignedVariables, instantiatedVariables, ref stopwatch))
					this.Solutions.Add(CloneLastSolution());
			}

			this.Runtime += stopwatch.Elapsed;
			stopwatch.Stop();
			return Solutions.Any() ? StateOperationResult.Solved : StateOperationResult.Unsatisfiable;
		}

		private IDictionary<string, IVariable<T>> CloneLastSolution()
		{
			return this.LastSolution.Select(v => v.Clone())
				.Cast<IVariable<T>>()
				.Select(v => new KeyValuePair<string, IVariable<T>>(v.Name, v))
				.OrderBy(kvp => kvp.Key)
				.ToDictionary(k => k.Key, v => v.Value);
		}

		private bool Search(out StateOperationResult searchResult, LinkedList<IVariable<T>> unassignedVariables,
			IList<IVariable<T>> instantiatedVariables, ref Stopwatch stopwatch, int timeOut = Int32.MaxValue)
		{
			searchResult = StateOperationResult.Unsatisfiable;
			if (unassignedVariables.Any(x => x.Size() == T.Zero))
			{
				this.Depth = -1;
				return false;
			}

			while (true)
			{
				if (this.Depth == this.Variables.Count)
				{
					searchResult = StateOperationResult.Solved;
					this.Runtime += stopwatch.Elapsed;
					stopwatch = Stopwatch.StartNew();

					this.LastSolution = instantiatedVariables.ToArray();

					return true;
				}

				instantiatedVariables[this.Depth] = GetMostConstrainedVariable(unassignedVariables);
				instantiatedVariables[this.Depth].Instantiate(this.Depth, out DomainOperationResult instantiateResult);

				if (instantiateResult != DomainOperationResult.InstantiateSuccessful)
					return false;

				if (ConstraintsViolated() || unassignedVariables.Any(v => v.Size() == T.Zero))
				{
					if (!Backtrack(unassignedVariables, instantiatedVariables))
						return false;
				}

				if (stopwatch.Elapsed.TotalSeconds > timeOut)
				{
					searchResult = StateOperationResult.TimedOut;
					return false;
				}

				++this.Depth;
			}
		}

		private bool Backtrack(LinkedList<IVariable<T>> unassignedVariables, IList<IVariable<T>> instantiatedVariables)
		{
			DomainOperationResult removeResult;
			do
			{
				if (this.Depth < 0)
					return false;

				unassignedVariables.AddFirst(instantiatedVariables[this.Depth]);
				BackTrackVariable(instantiatedVariables[this.Depth], out removeResult);
			} while (removeResult == DomainOperationResult.EmptyDomain);

			return true;
		}

		private bool ConstraintsViolated()
		{
			foreach (var constraint in this.Constraints.Where(constraint => constraint.StateChanged()))
			{
				constraint.Propagate(out ConstraintOperationResult result);
				if ((result & ConstraintOperationResult.Violated) == ConstraintOperationResult.Violated)
					return true;

				constraint.Check(out result);
				if ((result & ConstraintOperationResult.Violated) == ConstraintOperationResult.Violated)
					return true;
			}

			return false;
		}

		private void BackTrackVariable(IVariable<T> variablePrune, out DomainOperationResult result)
		{
			++this.Backtracks;
			var value = variablePrune.InstantiatedValue;

			foreach (var variable in this.Variables)
				variable.Backtrack(this.Depth);
			--this.Depth;

			variablePrune.Remove(value, this.Depth, out result);
		}

		private static IVariable<T> GetMostConstrainedVariable(LinkedList<IVariable<T>> list)
		{
			var temp = list.First;
			var node = list.First;

			while (node != null)
			{
				if (node.Value.Size() < temp.Value.Size())
					temp = node;

				if (temp.Value.Size() == T.One)
					break;

				node = node.Next;
			}
			list.Remove(temp);

			return temp.Value;
		}

		private readonly Random ran = new Random();
		private IVariable<int> GetRandomVariable(LinkedList<IVariable<int>> list)
		{
			var index = ran.Next(0, list.Count - 1);
			var node = list.First;
			while (--index >= 0)
				node = node.Next;
			list.Remove(node);
			return node.Value;
		}

		private IVariable<int> GetFirstVariable(LinkedList<IVariable<int>> list)
		{
			var first = list.First;
			list.Remove(first);
			return first.Value;
		}

		private IVariable<int> GetLastVariable(LinkedList<IVariable<int>> list)
		{
			var last = list.Last;
			list.Remove(last);
			return last.Value;
		}
	}
}
