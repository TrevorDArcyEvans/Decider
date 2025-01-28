/*
  Copyright © Iain McDonald 2010-2024
  
  This file is part of Decider.
*/
using Decider.Csp.Integer;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Decider.Csp.BaseTypes
{
	public class ConstrainedArray<T> : List<T>  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		private VariableInteger<T> Index { get; set; }

		public MetaExpressionInteger<T> this[VariableInteger<T> index]
		{
			get
			{
				Index = index;

				return new MetaExpressionInteger<T>(GetVariableInteger(), this.Evaluate, this.EvaluateBounds, this.Propagator, new[] { Index });
			}
		}

		public ConstrainedArray(IEnumerable<T> elements)
		{
			this.AddRange(elements);
		}

		private VariableInteger<T> GetVariableInteger()
		{
			return new VariableInteger<T>(Index.Name + this.ToString(), Elements());
		}

		private List<T> Elements()
		{
			return Enumerable.Range(int.CreateChecked(Index.Domain.LowerBound), int.CreateChecked(Index.Domain.UpperBound - Index.Domain.LowerBound + T.One))
				.Select(T.CreateChecked)
				.Where(i => Index.Domain.Contains(i))
				.Select(i => this[int.CreateChecked(i)])
				.ToList();
		}

		private SortedList<T, IList<T>> SortedElements()
		{
			var kvps = Enumerable.Range(int.CreateChecked(Index.Domain.LowerBound),int.CreateChecked(Index.Domain.UpperBound - Index.Domain.LowerBound + T.One)).
				Select(T.CreateChecked).
				Where(i => Index.Domain.Contains(i)).
				Select(i => new { Index = this[int.CreateChecked(i)], Value = i });

			var sortedList = new SortedList<T, IList<T>>();

			foreach (var kvp in kvps)
			{
				if (sortedList.ContainsKey(kvp.Index))
					sortedList[kvp.Index].Add(kvp.Value);
				else
					sortedList[kvp.Index] = new List<T>(new[] { kvp.Value });
			}

			return sortedList;
		}

		private T Evaluate(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return this[int.CreateChecked(Index.Value)];
		}

		private Bounds<T> EvaluateBounds(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			var elements = Elements();

			return new Bounds<T>(elements.DefaultIfEmpty().Min(), elements.DefaultIfEmpty().Max());
		}

        private ConstraintOperationResult Propagator(ExpressionInteger<T> left, ExpressionInteger<T> right, Bounds<T> enforce)
		{
			var result = ConstraintOperationResult.Undecided;

			var sortedElements = SortedElements();

			if (enforce.UpperBound < sortedElements.First().Key || enforce.LowerBound > sortedElements.Last().Key)
				return ConstraintOperationResult.Violated;

			var remove = sortedElements.
				TakeWhile(v => v.Key < enforce.LowerBound).
				Select(v => v.Value).
				Concat(sortedElements.
					Reverse().
					TakeWhile(v => v.Key > enforce.UpperBound).
					Select(v => v.Value)).
				SelectMany(i => i.ToList()).
				ToList();

			if (remove.Any())
			{
				result = ConstraintOperationResult.Propagated;

				foreach (var value in remove)
				{
					Index.Remove(value, out DomainOperationResult domainOperation);

					if (domainOperation == DomainOperationResult.EmptyDomain)
						return ConstraintOperationResult.Violated;
				}

				left.Bounds = EvaluateBounds(left, null);
			}

			return result;
		}
	}
}
