/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System.Collections.Generic;
using System.Numerics;
using Decider.Csp.Integer;

namespace Decider.Csp.Global
{
	internal class Node
	{
		internal int Index { get; set; }
		internal int Link { get; set; }
		internal LinkedList<Node> AdjoiningNodes { get; set; }
		internal string Label { get; private set; }
		internal int CycleIndex { get; set; }

		internal Node()
		{
			this.AdjoiningNodes = new LinkedList<Node>();
			this.Index = -1;
			this.Link = -1;
			this.CycleIndex = -1;
		}

		internal Node(string label)
			: this()
		{
			this.Label = label;
		}

		public override string ToString()
		{
			return this.Label;
		}
	}

	internal class NodeVariable<T> : Node where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		internal VariableInteger<T> Variable { get; set; }

		internal NodeVariable(VariableInteger<T> variable, string label)
			: base(label)
		{
			this.Variable = variable;
		}

		internal NodeVariable(VariableInteger<T> variable)
		{
			this.Variable = variable;
		}

	}

	internal class NodeValue<T> : Node where T : INumber<T>, IMinMaxValue<T>
	{
		internal T Value { get; set; }

		internal NodeValue(T value, string label)
			: base(label)
		{
			this.Value = value;
		}

		internal NodeValue(T value)
		{
			this.Value = value;
		}
	}
}
