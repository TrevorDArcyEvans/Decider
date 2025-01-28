/*
  Copyright © Iain McDonald 2010-2022

  This file is part of Decider.
*/
using System;
using System.Numerics;
using Decider.Csp.BaseTypes;

namespace Decider.Csp.Integer
{
	public class ExpressionInteger<T> : Expression<T>  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		public static ExpressionInteger<T> operator +(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value + r.Value,
				evaluateBounds = (l, r) =>
					{
						var leftBounds = l.GetUpdatedBounds();
						var rightBounds = r.GetUpdatedBounds();

						return new Bounds<T>
						(
							leftBounds.LowerBound + rightBounds.LowerBound,
							leftBounds.UpperBound + rightBounds.UpperBound
						);
					},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;
					if (first.Bounds.LowerBound < enforce.LowerBound - second.Bounds.UpperBound)
					{
						first.Bounds.LowerBound = enforce.LowerBound - second.Bounds.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.UpperBound > enforce.UpperBound - second.Bounds.LowerBound)
					{
						first.Bounds.UpperBound = enforce.UpperBound - second.Bounds.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (second.Bounds.LowerBound < enforce.LowerBound - first.Bounds.UpperBound)
					{
						second.Bounds.LowerBound = enforce.LowerBound - first.Bounds.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (second.Bounds.UpperBound > enforce.UpperBound - first.Bounds.LowerBound)
					{
						second.Bounds.UpperBound = enforce.UpperBound - first.Bounds.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator -(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			var expression = new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value - r.Value,
				evaluateBounds = (l, r) =>
					{
						var leftBounds = l.GetUpdatedBounds();
						var rightBounds = r.GetUpdatedBounds();

						return new Bounds<T>
						(
							leftBounds.LowerBound - rightBounds.UpperBound,
							leftBounds.UpperBound - rightBounds.LowerBound
						);
					},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;
					if (first.Bounds.LowerBound < enforce.LowerBound + second.Bounds.LowerBound)
					{
						first.Bounds.LowerBound = enforce.LowerBound + second.Bounds.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.UpperBound > enforce.UpperBound + second.Bounds.UpperBound)
					{
						first.Bounds.UpperBound = enforce.UpperBound + second.Bounds.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (second.Bounds.LowerBound < first.Bounds.LowerBound - enforce.UpperBound)
					{
						second.Bounds.LowerBound = first.Bounds.LowerBound - enforce.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (second.Bounds.UpperBound > first.Bounds.UpperBound - enforce.LowerBound)
					{
						second.Bounds.UpperBound = first.Bounds.UpperBound - enforce.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};

			expression.remove = prune =>
			{
				var result = DomainOperationResult.ElementNotInDomain;
				if (expression.left.Bounds.UpperBound - expression.left.Bounds.LowerBound == T.Zero)
					result = ((ExpressionInteger<T>) expression.right).remove(expression.left.Bounds.LowerBound - prune);

				if (result == DomainOperationResult.EmptyDomain)
					return result;

				if (expression.right.Bounds.UpperBound - expression.right.Bounds.LowerBound == T.Zero)
					result = ((ExpressionInteger<T>) expression.left).remove(expression.right.Bounds.LowerBound + prune);

				return result;
			};

			return expression;
		}

		public static ExpressionInteger<T> operator /(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value / r.Value,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.LowerBound / rightBounds.UpperBound,
						leftBounds.UpperBound / rightBounds.LowerBound
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;
					if (first.Bounds.LowerBound < second.Bounds.LowerBound * enforce.LowerBound)
					{
						first.Bounds.LowerBound = second.Bounds.LowerBound * enforce.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.UpperBound > second.Bounds.UpperBound * enforce.UpperBound)
					{
						first.Bounds.UpperBound = second.Bounds.UpperBound * enforce.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (enforce.UpperBound == T.Zero)
						return result;

					if (second.Bounds.LowerBound < first.Bounds.LowerBound / enforce.UpperBound)
					{
						second.Bounds.LowerBound = first.Bounds.LowerBound / enforce.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (enforce.LowerBound == T.Zero)
						return result;

					if (second.Bounds.UpperBound > first.Bounds.UpperBound / enforce.LowerBound)
					{
						second.Bounds.UpperBound = first.Bounds.UpperBound / enforce.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator *(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value * r.Value,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.LowerBound * rightBounds.LowerBound,
						leftBounds.UpperBound * rightBounds.UpperBound
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (second.Bounds.UpperBound == T.Zero || second.Bounds.LowerBound == T.Zero)
						return result;

					if (first.Bounds.LowerBound < enforce.LowerBound / second.Bounds.UpperBound)
					{
						first.Bounds.LowerBound = enforce.LowerBound / second.Bounds.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.UpperBound > enforce.UpperBound / second.Bounds.LowerBound)
					{
						first.Bounds.UpperBound = enforce.UpperBound / second.Bounds.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.UpperBound == T.Zero || first.Bounds.LowerBound == T.Zero)
						return result;

					if (second.Bounds.LowerBound < enforce.LowerBound / first.Bounds.UpperBound)
					{
						second.Bounds.LowerBound = enforce.LowerBound / first.Bounds.UpperBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (second.Bounds.UpperBound > enforce.UpperBound / first.Bounds.LowerBound)
					{
						second.Bounds.UpperBound = enforce.UpperBound / first.Bounds.LowerBound;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator &(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => (l.Value != T.Zero) && (r.Value != T.Zero) ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						(leftBounds.LowerBound != T.Zero) && (rightBounds.LowerBound != T.Zero) ? T.One : T.Zero,
						(leftBounds.UpperBound != T.Zero) && (rightBounds.UpperBound != T.Zero) ? T.One : T.Zero
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.LowerBound > T.Zero)
					{
						if (first.Bounds.UpperBound == T.Zero || second.Bounds.UpperBound == T.Zero)
							result = ConstraintOperationResult.Violated;
						else
						{
							first.Bounds.LowerBound = T.One;
							second.Bounds.LowerBound = T.One;
							result = ConstraintOperationResult.Propagated;
						}
					}
					else if (enforce.UpperBound == T.Zero && enforce.UpperBound < enforce.LowerBound)
					{
						if (first.Bounds.LowerBound == T.One)
						{
							if (second.Bounds.LowerBound > T.Zero)
								result = ConstraintOperationResult.Violated;
							else if (second.Bounds.UpperBound == T.One)
							{
								second.Bounds.UpperBound = T.Zero;
								result = ConstraintOperationResult.Propagated;
							}
						}

						if (second.Bounds.LowerBound == T.One)
						{
							if (second.Bounds.LowerBound > T.Zero)
								result = ConstraintOperationResult.Violated;
							else if (first.Bounds.UpperBound == T.One)
							{
								first.Bounds.UpperBound = T.Zero;
								result = ConstraintOperationResult.Propagated;
							}
						}
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator |(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => (l.Value != T.Zero) || (r.Value != T.Zero) ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						(leftBounds.LowerBound != T.Zero) || (rightBounds.LowerBound != T.Zero) ? T.One : T.Zero,
						(leftBounds.UpperBound != T.Zero) || (rightBounds.UpperBound != T.Zero) ? T.One : T.Zero
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.LowerBound > T.Zero)
					{
						if (first.Bounds.UpperBound == T.Zero || second.Bounds.UpperBound == T.Zero)
						{
							second.Bounds.LowerBound = T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (second.Bounds.UpperBound == T.Zero)
						{
							first.Bounds.LowerBound = T.One;
							result = ConstraintOperationResult.Propagated;
						}
					}
					else if (enforce.UpperBound == T.Zero)
					{
						first.Bounds.UpperBound = T.Zero;
						second.Bounds.UpperBound = T.Zero;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator ^(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => ((l.Value != T.Zero) || (r.Value != T.Zero)) && ((l.Value == T.Zero) || (r.Value == T.Zero)) ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						(leftBounds.LowerBound == leftBounds.UpperBound) && (rightBounds.LowerBound > T.Zero) &&
							(leftBounds.LowerBound != rightBounds.LowerBound) ? T.One : T.Zero,
						(leftBounds.LowerBound == leftBounds.UpperBound) && (rightBounds.LowerBound == rightBounds.UpperBound) &&
							(leftBounds.LowerBound == rightBounds.LowerBound) ? T.Zero : T.One
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;
					
					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;
					
					if (enforce.LowerBound > T.Zero)
					{
						if (first.Bounds.UpperBound == T.Zero)
						{
							second.Bounds.LowerBound = T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.LowerBound == T.One)
						{
							second.Bounds.UpperBound = T.Zero;
							result = ConstraintOperationResult.Propagated;
						}

						if (second.Bounds.UpperBound == T.Zero)
						{
							first.Bounds.LowerBound = T.One;
							result = ConstraintOperationResult.Propagated;
						}
						
						if (second.Bounds.LowerBound == T.One)
						{
							first.Bounds.UpperBound = T.Zero;
							result = ConstraintOperationResult.Propagated;
						}
					}
					else if (enforce.UpperBound == T.Zero)
					{
						if (first.Bounds.UpperBound == T.Zero)
						{
							second.Bounds.UpperBound = T.Zero;
							result = ConstraintOperationResult.Propagated;
						}
						
						if (first.Bounds.LowerBound == T.One)
						{
							second.Bounds.LowerBound = T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (second.Bounds.UpperBound == T.Zero)
						{
							first.Bounds.UpperBound = T.Zero;
							result = ConstraintOperationResult.Propagated;
						}
						
						if (second.Bounds.LowerBound == T.One)
						{
							first.Bounds.LowerBound = T.One;
							result = ConstraintOperationResult.Propagated;
						}
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator !(ExpressionInteger<T> operand)
		{
			return new ExpressionInteger<T>(operand, null)
			{
				evaluate = (l, r) => l.Value == T.Zero ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var bounds = l.GetUpdatedBounds();

					return new Bounds<T>(bounds.UpperBound == T.Zero ? T.One : T.Zero, bounds.LowerBound == T.Zero ? T.One : T.Zero);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.UpperBound == T.Zero)
					{
						first.Bounds.LowerBound = T.One;
						result = ConstraintOperationResult.Propagated;
					}

					if (enforce.LowerBound > T.Zero)
					{
						first.Bounds.UpperBound = T.Zero;
						result = ConstraintOperationResult.Propagated;
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator <(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value < r.Value ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.UpperBound < rightBounds.LowerBound ? T.One : T.Zero,
						leftBounds.LowerBound <= leftBounds.UpperBound ? T.One : T.Zero
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.LowerBound > T.Zero) // enforce a < b
					{
						if (second.Bounds.LowerBound <= first.Bounds.LowerBound)
						{
							second.Bounds.LowerBound = first.Bounds.LowerBound + T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound >= second.Bounds.UpperBound)
						{
							first.Bounds.UpperBound = second.Bounds.UpperBound - T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.LowerBound >= second.Bounds.UpperBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}
					else if (enforce.UpperBound == T.Zero) // enforce a >= b
					{
						if (first.Bounds.LowerBound < second.Bounds.LowerBound)
						{
							first.Bounds.LowerBound = second.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (second.Bounds.UpperBound > first.Bounds.UpperBound)
						{
							second.Bounds.UpperBound = first.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound < second.Bounds.LowerBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator >(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value > r.Value ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.LowerBound > rightBounds.UpperBound ? T.One : T.Zero,
						leftBounds.UpperBound >= leftBounds.LowerBound ? T.One : T.Zero
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.LowerBound > T.Zero) // enforce a > b
					{
						if (first.Bounds.LowerBound <= second.Bounds.LowerBound)
						{
							first.Bounds.LowerBound = second.Bounds.LowerBound + T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (second.Bounds.UpperBound >= first.Bounds.UpperBound)
						{
							second.Bounds.UpperBound = first.Bounds.UpperBound - T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound <= second.Bounds.LowerBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}
					else if (enforce.UpperBound == T.Zero) // enforce a <= b
					{
						if (second.Bounds.LowerBound < first.Bounds.LowerBound)
						{
							second.Bounds.LowerBound = first.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound > second.Bounds.UpperBound)
						{
							first.Bounds.UpperBound = second.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.LowerBound > second.Bounds.UpperBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator <=(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value <= r.Value ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.UpperBound <= rightBounds.LowerBound ? T.One : T.Zero,
						leftBounds.LowerBound <= leftBounds.UpperBound ? T.One : T.Zero
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.LowerBound > T.Zero) // enforce a <= b
					{
						if (second.Bounds.LowerBound < first.Bounds.LowerBound)
						{
							second.Bounds.LowerBound = first.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound > second.Bounds.UpperBound)
						{
							first.Bounds.UpperBound = second.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.LowerBound > second.Bounds.UpperBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}
					else if (enforce.UpperBound == T.Zero) // enforce a > b
					{
						if (first.Bounds.LowerBound <= second.Bounds.LowerBound)
						{
							first.Bounds.LowerBound = second.Bounds.LowerBound + T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (second.Bounds.UpperBound >= first.Bounds.UpperBound)
						{
							second.Bounds.UpperBound = first.Bounds.UpperBound - T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound <= second.Bounds.LowerBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator >=(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value >= r.Value ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.LowerBound >= rightBounds.UpperBound ? T.One : T.Zero,
						leftBounds.UpperBound >= leftBounds.LowerBound ? T.One : T.Zero
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.LowerBound > T.Zero) // enforce a >= b
					{
						if (first.Bounds.LowerBound < second.Bounds.LowerBound)
						{
							first.Bounds.LowerBound = second.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (second.Bounds.UpperBound > first.Bounds.UpperBound)
						{
							second.Bounds.UpperBound = first.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound < second.Bounds.LowerBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}
					else if (enforce.UpperBound == T.Zero) // enforce a < b
					{
						if (second.Bounds.LowerBound <= first.Bounds.LowerBound)
						{
							second.Bounds.LowerBound = first.Bounds.LowerBound + T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound >= second.Bounds.UpperBound)
						{
							first.Bounds.UpperBound = second.Bounds.UpperBound - T.One;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.LowerBound >= second.Bounds.UpperBound)
						{
							result = ConstraintOperationResult.Violated;
						}
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator ==(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value == r.Value ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.LowerBound == leftBounds.UpperBound && rightBounds.LowerBound == rightBounds.UpperBound &&
						leftBounds.LowerBound == rightBounds.LowerBound ? T.One : T.Zero,
						leftBounds.UpperBound < rightBounds.LowerBound || leftBounds.LowerBound > rightBounds.UpperBound ? T.Zero : T.One
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.LowerBound > T.Zero)
					{
						if (first.Bounds.LowerBound < second.Bounds.LowerBound)
						{
							first.Bounds.LowerBound = second.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}
						else if (second.Bounds.LowerBound < first.Bounds.LowerBound)
						{
							second.Bounds.LowerBound = first.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound < second.Bounds.UpperBound)
						{
							second.Bounds.UpperBound = first.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}
						else if (second.Bounds.UpperBound < first.Bounds.UpperBound)
						{
							first.Bounds.UpperBound = second.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public static ExpressionInteger<T> operator !=(ExpressionInteger<T> left, ExpressionInteger<T> right)
		{
			return new ExpressionInteger<T>(left, right)
			{
				evaluate = (l, r) => l.Value != r.Value ? T.One : T.Zero,
				evaluateBounds = (l, r) =>
				{
					var leftBounds = l.GetUpdatedBounds();
					var rightBounds = r.GetUpdatedBounds();

					return new Bounds<T>
					(
						leftBounds.UpperBound < rightBounds.LowerBound || leftBounds.LowerBound > rightBounds.UpperBound ? T.One : T.Zero,
						leftBounds.LowerBound == leftBounds.UpperBound && rightBounds.LowerBound == rightBounds.UpperBound &&
						leftBounds.LowerBound == rightBounds.LowerBound ? T.Zero : T.One
					);
				},
				propagator = (first, second, enforce) =>
				{
					var result = ConstraintOperationResult.Undecided;

					if (enforce.LowerBound == T.Zero && enforce.LowerBound < enforce.UpperBound)
						return result;

					if (enforce.UpperBound == T.Zero)
					{
						if (first.Bounds.LowerBound < second.Bounds.LowerBound)
						{
							first.Bounds.LowerBound = second.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}
						else if (second.Bounds.LowerBound < first.Bounds.LowerBound)
						{
							second.Bounds.LowerBound = first.Bounds.LowerBound;
							result = ConstraintOperationResult.Propagated;
						}

						if (first.Bounds.UpperBound < second.Bounds.UpperBound)
						{
							second.Bounds.UpperBound = first.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}
						else if (second.Bounds.UpperBound < first.Bounds.UpperBound)
						{
							first.Bounds.UpperBound = second.Bounds.UpperBound;
							result = ConstraintOperationResult.Propagated;
						}
					}
					else
					{
						if (first.Bounds.UpperBound == first.Bounds.LowerBound &&
							second.remove(first.Bounds.LowerBound) == DomainOperationResult.EmptyDomain)
							return ConstraintOperationResult.Violated;

						if (second.Bounds.UpperBound == second.Bounds.LowerBound &&
							first.remove(second.Bounds.LowerBound) == DomainOperationResult.EmptyDomain)
							return ConstraintOperationResult.Violated;
					}

					if (first.Bounds.LowerBound > first.Bounds.UpperBound || second.Bounds.LowerBound > second.Bounds.UpperBound)
						result = ConstraintOperationResult.Violated;

					return result;
				}
			};
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		protected Expression<T> left;
		protected Expression<T> right;
		protected T integer;
		protected Func<ExpressionInteger<T>, ExpressionInteger<T>, T> evaluate;
		protected Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>> evaluateBounds;
		protected Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>, ConstraintOperationResult> propagator;
		protected Func<T, DomainOperationResult> remove;

		public Expression<T> Left { get { return this.left; } }
		public Expression<T> Right { get { return this.right; } }
		public T Integer { get { return this.integer; } }
		public Func<ExpressionInteger<T>, ExpressionInteger<T>, T> Evaluate { get { return this.evaluate; } }
		public Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>> EvaluateBounds { get { return this.evaluateBounds; } }
		public Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>, ConstraintOperationResult> Propagator { get { return this.propagator; } }

		public override bool IsBound
		{
			get
			{
				if (((object) this.left) == null && ((object) this.right) == null)
					return true;

				return ((object) this.right) == null ? this.left.IsBound : this.left.IsBound && this.right.IsBound;
			}
		}

		public override Bounds<T> GetUpdatedBounds()
		{
			if (this.Evaluate == null)
			{
				if (this.left is VariableInteger<T>)
					this.Bounds = this.left.GetUpdatedBounds();

				return this.Bounds;
			}

			this.Bounds = this.EvaluateBounds((ExpressionInteger<T>) this.left, (ExpressionInteger<T>) this.right);
			return this.Bounds;
		}

		public override T Value
		{
			get
			{
				if (this.Evaluate == null)
					return this.left is VariableInteger<T> ? this.left.Value : this.integer;

				return this.Evaluate((ExpressionInteger<T>) this.left, (ExpressionInteger<T>) this.right);
			}
		}

		public override void Propagate(Bounds<T> enforceBounds, out ConstraintOperationResult result)
		{
			left.GetUpdatedBounds();

			if (right != null)
				right.GetUpdatedBounds();

			var propagated = false;
			var intermediateResult = propagator((ExpressionInteger<T>) left, (ExpressionInteger<T>) right, enforceBounds);

			while (intermediateResult == ConstraintOperationResult.Propagated)
			{
				var leftResult = ConstraintOperationResult.Undecided;
				var rightResult = ConstraintOperationResult.Undecided;

				if (!left.IsBound)
					left.Propagate(left.Bounds, out leftResult);

				if (right != null && !right.IsBound)
					right.Propagate(right.Bounds, out rightResult);

				intermediateResult = (leftResult | rightResult) & ConstraintOperationResult.Propagated;
				if (intermediateResult != ConstraintOperationResult.Propagated)
					continue;

				propagated = true;
				intermediateResult = propagator((ExpressionInteger<T>) left, (ExpressionInteger<T>) right, enforceBounds);
			}

			if (intermediateResult == ConstraintOperationResult.Violated)
				result = ConstraintOperationResult.Violated;
			else
				result = propagated ? ConstraintOperationResult.Propagated : ConstraintOperationResult.Undecided;
		}

		public ExpressionInteger(Expression<T> left, Expression<T> right)
		{
			this.left = left;
			this.right = right;
		}

		public ExpressionInteger(T integer)
		{
			this.integer = integer;
			Bounds = new Bounds<T>(integer, integer);
			remove = _ => DomainOperationResult.ElementNotInDomain;
		}

		internal ExpressionInteger(VariableInteger<T> variable,
			Func<ExpressionInteger<T>, ExpressionInteger<T>, T> evaluate,
			Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>> evaluateBounds,
			Func<ExpressionInteger<T>, ExpressionInteger<T>, Bounds<T>, ConstraintOperationResult> propagator)
		{
			this.left = variable;
			this.evaluate = evaluate;
			this.evaluateBounds = evaluateBounds;
			this.propagator = propagator;
		}

		public static implicit operator ExpressionInteger<T>(T i)
		{
			return new ExpressionInteger<T>(i);
		}

		internal ExpressionInteger() { }
	}
}

