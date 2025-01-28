/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using Decider.Csp.BaseTypes;

namespace Decider.Csp.Integer
{
	public sealed class VariableInteger<T> : ExpressionInteger<T>, IVariable<T>  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		private struct DomInt<T> where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
		{
			internal readonly IDomain<T> Domain;
			internal readonly int Depth;

			internal DomInt(IDomain<T> domain, int depth)
			{
				this.Domain = domain;
				this.Depth = depth;
			}

			public object Clone()
			{
				return new DomInt<T>(this.Domain.Clone(), this.Depth);
			}
		}

		public IVariable<T> Clone()
		{
			return new VariableInteger<T>
				{
					domainStack = new Stack<DomInt<T>>(domainStack.Select(d => d.Clone()).Reverse().Cast<DomInt<T>>()),
					State = State,
					Name = Name
				};
		}

		private Stack<DomInt<T>> domainStack;
		public IState<T> State { get; set; }
		public string Name { get; private set; }
		public IDomain<T> Domain { get { return this.domainStack.Peek().Domain; } }

		internal VariableInteger()
		{
			this.remove = prune =>
			{
				DomainOperationResult result;
				Remove(prune, out result);
				return result;
			};
		}

		public VariableInteger(string name)
			: this()
		{
			this.Name = name;
			this.domainStack = new Stack<DomInt<T>>();
			this.domainStack.Push(new DomInt<T>(DomainBinaryInteger<T>.CreateDomain(T.MinValue, T.MaxValue), -1));
		}

		public VariableInteger(string name, IList<T> elements)
			: this()
		{
			this.Name = name;
			this.domainStack = new Stack<DomInt<T>>();
			this.domainStack.Push(new DomInt<T>(DomainBinaryInteger<T>.CreateDomain(elements), -1));
		}

		public VariableInteger(string name, T lowerBound, T upperBound)
			: this()
		{
			this.Name = name;
			this.domainStack = new Stack<DomInt<T>>();
			this.domainStack.Push(new DomInt<T>(DomainBinaryInteger<T>.CreateDomain(lowerBound, upperBound), -1));
		}

		public T InstantiatedValue
		{
			get
			{
				return this.Domain.InstantiatedValue;
			}
		}

		public void Instantiate(int depth, out DomainOperationResult result)
		{
			var instantiatedDomain = this.Domain.Clone();
			instantiatedDomain.Instantiate(out result);
			if (result != DomainOperationResult.InstantiateSuccessful)
				return;

			this.domainStack.Push(new DomInt<T>(instantiatedDomain, depth));
		}

		public void Instantiate(T value, int depth, out DomainOperationResult result)
		{
			var instantiatedDomain = this.Domain.Clone();
			instantiatedDomain.Instantiate(value, out result);
			if (result != DomainOperationResult.InstantiateSuccessful)
				return;

			this.domainStack.Push(new DomInt<T>(instantiatedDomain, depth));
		}

		public void Backtrack(int fromDepth)
		{
			while (this.domainStack.Peek().Depth >= fromDepth)
				this.domainStack.Pop();
		}

		public void Remove(T value, int depth, out DomainOperationResult result)
		{
			if (this.domainStack.Peek().Depth != depth)
			{
				this.domainStack.Push(new DomInt<T>(this.Domain.Clone(), depth));

				this.Domain.Remove(value, out result);

				if (result == DomainOperationResult.ElementNotInDomain)
					this.domainStack.Pop();
			}
			else
				this.Domain.Remove(value, out result);
		}

		public void Remove(T value, out DomainOperationResult result)
		{
			if (Instantiated() || value > this.Domain.UpperBound || value < this.Domain.LowerBound)
			{
				result = DomainOperationResult.ElementNotInDomain;
				return;
			}

			Remove(value, this.State.Depth, out result);
		}

		public bool Instantiated()
		{
			return this.Domain.Instantiated();
		}

		public T Size()
		{
			return this.Domain.Size();
		}

		public void SetState(IState<T> state)
		{
			this.State = state;
		}

		public int CompareTo(IVariable<T> otherVariable)
		{
			return int.CreateChecked(Size() - otherVariable.Size());
		}

		public override T Value
		{
			get { return this.InstantiatedValue; }
		}

		public override bool IsBound
		{
			get { return Instantiated(); }
		}

		public override Bounds<T> GetUpdatedBounds()
		{
			this.Bounds = new Bounds<T>(this.Domain.LowerBound, this.Domain.UpperBound);
			return this.Bounds;
		}

		public override void Propagate(Bounds<T> enforceBounds, out ConstraintOperationResult result)
		{
			result = ConstraintOperationResult.Undecided;

			if (this.State == null)
				return;

			var domainIntStack = this.domainStack.Peek();
			var isDomainNew = false;
			IDomain<T> propagatedDomain;

			if (domainIntStack.Depth == this.State.Depth)
			{
				propagatedDomain = domainIntStack.Domain;
			}
			else
			{
				isDomainNew = true;
				propagatedDomain = domainIntStack.Domain.Clone();
				this.domainStack.Push(new DomInt<T>(propagatedDomain, this.State.Depth));
			}

			var domainResult = DomainOperationResult.RemoveSuccessful;

			while (enforceBounds.LowerBound > propagatedDomain.LowerBound &&
				domainResult == DomainOperationResult.RemoveSuccessful)
			{
				propagatedDomain.Remove(propagatedDomain.LowerBound, out domainResult);
				result = ConstraintOperationResult.Propagated;
			}

			while (enforceBounds.UpperBound < propagatedDomain.UpperBound &&
				domainResult == DomainOperationResult.RemoveSuccessful)
			{
				propagatedDomain.Remove(propagatedDomain.UpperBound, out domainResult);
				result = ConstraintOperationResult.Propagated;
			}

			if (isDomainNew && result != ConstraintOperationResult.Propagated)
				this.domainStack.Pop();
		}

		public override string ToString()
		{
			if (this.IsBound)
				return this.InstantiatedValue.ToString();

			return this.Domain.ToString();
		}
	}
}
