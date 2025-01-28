/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Decider.Csp.BaseTypes;

namespace Decider.Csp.Integer
{
	public class ConstraintInteger<T> : ExpressionInteger<T>, IConstraint  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		private readonly IVariable<T>[] variableArray;
		private readonly IDomain<T>[] domainArray;

		public ConstraintInteger(Expression<T> expression)
		{
			var expressionInt = (ExpressionInteger<T>) expression;
			this.left = expressionInt.Left;
			this.right = expressionInt.Right;
			this.evaluate = expressionInt.Evaluate;
			this.evaluateBounds = expressionInt.EvaluateBounds;
			this.propagator = expressionInt.Propagator;
			this.integer = expressionInt.Integer;

			var variableSet = new HashSet<IVariable<T>>();
			ConstructVariableList((ExpressionInteger<T>) expression, variableSet);
			this.variableArray = variableSet.ToArray();
			this.domainArray = new IDomain<T>[this.variableArray.Length];
		}

		private static void ConstructVariableList(ExpressionInteger<T> expression, ISet<IVariable<T>> variableSet)
		{
			if (expression.Left is VariableInteger<T>)
			{
				variableSet.Add((VariableInteger<T>) expression.Left);
			}
			else if (expression.Left is MetaExpressionInteger<T>)
			{
				ConstructVariableList((ExpressionInteger<T>) expression.Left, variableSet);
				foreach (var variable in ((IMetaExpression<T>) expression.Left).Support)
				{
					variableSet.Add(variable);
				}
			}
			else if (expression.Left is ExpressionInteger<T>)
			{
				ConstructVariableList((ExpressionInteger<T>) expression.Left, variableSet);
			}


			if (expression.Right is VariableInteger<T>)
			{
				variableSet.Add((VariableInteger<T>) expression.Right);
			}
			else if (expression.Right is MetaExpressionInteger<T>)
			{
				ConstructVariableList((ExpressionInteger<T>) expression.Right, variableSet);
				foreach (var variable in ((IMetaExpression<T>) expression.Right).Support)
				{
					variableSet.Add(variable);
				}
			}
			else if (expression.Right is ExpressionInteger<T>)
			{
				ConstructVariableList((ExpressionInteger<T>) expression.Right, variableSet);
			}
		}

		public void Check(out ConstraintOperationResult result)
		{
			for (var i = 0; i < this.variableArray.Length; ++i)
				this.domainArray[i] = ((VariableInteger<T>) variableArray[i]).Domain;

			if (this.variableArray.Any(variable => !variable.Instantiated()))
			{
				result = ConstraintOperationResult.Undecided;
				return;
			}

			try
			{
				result = this.Value != T.Zero ? ConstraintOperationResult.Satisfied : ConstraintOperationResult.Violated;
			}
			catch (DivideByZeroException)
			{
				result = ConstraintOperationResult.Violated;
			}
		}

		public void Propagate(out ConstraintOperationResult result)
		{
			var enforce = new Bounds<T>(T.One, T.One);

			do
			{
				Propagate(enforce, out result);
			} while ((result & ConstraintOperationResult.Propagated) == ConstraintOperationResult.Propagated);
		}

		public bool StateChanged()
		{
			return this.variableArray.Where((variable, index) => ((VariableInteger<T>) variable)
				.Domain != this.domainArray[index]).Any();
		}
	}
}

