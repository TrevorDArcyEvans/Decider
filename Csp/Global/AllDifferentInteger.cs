/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Decider.Csp.BaseTypes;
using Decider.Csp.Integer;

namespace Decider.Csp.Global
{
	public class AllDifferentInteger<T> : IConstraint  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		private readonly VariableInteger<T>[] variableArray;
		private readonly IDomain<T>[] domainArray;
		private BipartiteGraph<T> Graph { get; set; }
		private readonly CycleDetection cycleDetection;

		private IState<T> State { get; set; }
		private int Depth
		{
			get
			{
				if (this.State == null)
					this.State = this.variableArray[0].State;

				return this.State.Depth;
			}
		}

		public AllDifferentInteger(IEnumerable<VariableInteger<T>> variables)
		{
			this.variableArray = variables.ToArray();
			this.domainArray = new IDomain<T>[this.variableArray.Length];
			this.cycleDetection = new CycleDetection();
		}

		public void Check(out ConstraintOperationResult result)
		{
			for (var i = 0; i < this.variableArray.Length; ++i)
				this.domainArray[i] = variableArray[i].Domain;

			if (!FindMatching())
			{
				result = ConstraintOperationResult.Violated;
				return;
			}

			if (this.variableArray.Cast<IVariable<T>>().Any(variable => !variable.Instantiated()))
			{
				result = ConstraintOperationResult.Undecided;
				return;
			}

			result = ConstraintOperationResult.Satisfied;
		}

		private bool FindMatching()
		{
			return this.Graph.MaximalMatching() >= this.variableArray.Length;
		}

		public void Propagate(out ConstraintOperationResult result)
		{
			this.Graph = new BipartiteGraph<T>(this.variableArray);

			if (!FindMatching())
			{
				result = ConstraintOperationResult.Violated;
				return;
			}

			this.cycleDetection.Graph = this.Graph;
			this.cycleDetection.DetectCycle();
			
			result = ConstraintOperationResult.Undecided;
			foreach (var cycle in this.cycleDetection.StronglyConnectedComponents)
			{
				foreach (var node in cycle)
				{
					if (!(node is NodeVariable<T>) || node == this.Graph.NullNode)
						continue;

					var variable = ((NodeVariable<T>) node).Variable;
					foreach (var value in variable.Domain.Cast<T>().Where(value =>
						this.Graph.Values[value].CycleIndex != node.CycleIndex &&
						((NodeValue<T>) this.Graph.Pair[node]).Value != value))
					{
						result = ConstraintOperationResult.Propagated;

						variable.Remove(value, this.Depth, out DomainOperationResult domainResult);

						if (domainResult != DomainOperationResult.EmptyDomain)
							continue;

						result = ConstraintOperationResult.Violated;
						return;
					}
				}
			}
		}

		public bool StateChanged()
		{
			return this.variableArray.Where((t, i) => t.Domain != this.domainArray[i]).Any();
		}
	}
}
