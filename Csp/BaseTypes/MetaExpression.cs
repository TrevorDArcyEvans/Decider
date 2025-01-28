/*
  Copyright © Iain McDonald 2010-2022

  This file is part of Decider.

  Unlike the Expression type which is wholly supported on its own, the MetaExpression relies
  on the values of other supporting variables. Thus, if those variables change, the bounds
  of the MetaExpression need to be re-evaluated.
*/
using System.Collections.Generic;
using System.Numerics;

namespace Decider.Csp.BaseTypes
{
	public interface IMetaExpression<T> where T : INumber<T>, IMinMaxValue<T>
	{
		IList<IVariable<T>> Support { get; }
	}
}
