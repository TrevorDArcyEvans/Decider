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
	public class MetaExpressionInteger<T> : ExpressionInteger<T>, IMetaExpression<T>  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		private readonly IList<IVariable<T>> support;

		public IList<IVariable<T>> Support
		{
			get { return this.support; }
		}

		public MetaExpressionInteger(Expression<T> left, Expression<T> right, IEnumerable<IVariable<T>> support)
			: base(left, right)
		{
			this.support = support.ToList();
		}

		public MetaExpressionInteger(T integer, IEnumerable<IVariable<T>> support)
			: base(integer)
		{
			this.support = support.ToList();
		}

		internal MetaExpressionInteger(VariableInteger<T> variable,
			Func<ExpressionInteger<T>, ExpressionInteger<T>, T> evaluate,
			Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>> evaluateBounds,
			Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>, ConstraintOperationResult> propagator,
			IEnumerable<IVariable<T>> support)
			: base(variable, evaluate, evaluateBounds, propagator)
		{
			this.support = support.ToList();
		}
	}
}
