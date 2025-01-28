/*
  Copyright © Iain McDonald 2010-2022
  
  This file is part of Decider.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Decider.Csp.BaseTypes;

namespace Decider.Csp.Integer
{
	public class DomainBinaryInteger<T> : IDomain<T>  where T : INumber<T>, IMinMaxValue<T>, IBinaryNumber<T>
	{
		#region Underlying domain datatype
		
		//	IMPORTANT -	These lines must be updated together to reflect the correct size of bits per datatype
		private readonly T BitsPerDatatype =T.CreateChecked(8 * sizeof(uint));
		private T[] domain;
		private readonly T AllSet = T.MaxValue;// T.CreateChecked(0xFFFFFFFF);
		
		#endregion

		private T lowerBound;
		private T upperBound;
		private T size;
		private T offset;

		public bool Contains(T index)
		{
			return IsInDomain(index);
		}

		private bool IsInDomain(T index)
		{
			index += offset;
			var domIdx = ((index + T.One) % BitsPerDatatype == T.Zero) ?
				(index + T.One) / BitsPerDatatype - T.One : (index + T.One) / BitsPerDatatype;
			return (this.domain[int.CreateChecked(domIdx)] &
			       T.CreateChecked((ulong) (0x1 << int.CreateChecked((index % BitsPerDatatype))))) != T.Zero;
		}

		private void RemoveFromDomain(T index)
		{
			index += offset;
			var domIdx = ((index + T.One) % BitsPerDatatype == T.Zero) ? (index + T.One) / BitsPerDatatype - T.One :
				(index + T.One) / BitsPerDatatype;
			this.domain[int.CreateChecked(domIdx)] &= T.CreateSaturating((uint) ~(0x1 << int.CreateChecked((index % BitsPerDatatype))));
		}

		internal DomainBinaryInteger()
			: this(T.One)
		{
		}

		internal DomainBinaryInteger(T domainSize)
		{
			if (domainSize < T.Zero)
				throw new ArgumentException("Invalid Domain Size");
			
			this.lowerBound = T.Zero;
			this.upperBound = domainSize;
			this.size = upperBound - lowerBound + T.One;
			var domArrSize = ((domainSize + T.One) % BitsPerDatatype == T.Zero) ?
				(domainSize + T.One) / BitsPerDatatype : (domainSize + T.One) / BitsPerDatatype + T.One;
			this.domain = new T[int.CreateChecked(domArrSize)];

			for (var i = 0; i < this.domain.Length - 1; ++i)
				this.domain[i] = AllSet;

			if ((domainSize + T.One) % BitsPerDatatype == T.Zero)
				this.domain[this.domain.Length - 1] = AllSet;
			else
				for (var i = 0; i < int.CreateChecked((domainSize + T.One) % BitsPerDatatype); ++i)
					this.domain[this.domain.Length - 1] |= T.CreateChecked((uint) (0x1 << i));
		}

		internal DomainBinaryInteger(T lowerBound, T upperBound)
			: this(upperBound + (lowerBound < T.Zero ? -lowerBound : T.Zero))
		{
			if (lowerBound < T.Zero)
				this.offset = -lowerBound;

			this.lowerBound = T.Max(lowerBound, T.Zero);
			this.size = upperBound - lowerBound + T.One;
			var count = T.Zero;
			while (count < lowerBound)
				RemoveFromDomain(count++);
		}

		internal T[] Domain()
		{
			return this.domain;
		}

		public static IDomain<T> CreateDomain(T lowerBound, T upperBound)
		{
			if (lowerBound > upperBound)
				throw new ArgumentException("Invalid Domain Bounds");

			var domainImpl = new DomainBinaryInteger<T>(lowerBound, upperBound);
			return domainImpl;
		}

		public static IDomain<T> CreateDomain(IList<T> elements)
		{
			var lowerBound = elements.Min();
			var upperBound = elements.Max();

			var domainImpl = new DomainBinaryInteger<T>(lowerBound, upperBound);

			for (var i = lowerBound; i <= upperBound; ++i)
				if (!elements.Contains(i))
					domainImpl.RemoveFromDomain(i);

			return domainImpl;
		}

		#region IDomain<int> Members

		public T InstantiatedValue
		{
			get
			{
				if (!Instantiated())
					throw new DeciderException("Trying to access InstantiatedValue of an uninstantiated domain.");

				return this.lowerBound - offset;
			}
		}

		public void Instantiate(out DomainOperationResult result)
		{
			InstantiateLowest(out result);
		}

		public void Instantiate(T value, out DomainOperationResult result)
		{
			if (!IsInDomain(value))
			{
				result = DomainOperationResult.ElementNotInDomain;
				return;
			}

			this.size = T.One;
			this.lowerBound = this.upperBound = value - offset;
			result = DomainOperationResult.InstantiateSuccessful;
		}

		public void InstantiateLowest(out DomainOperationResult result)
		{
			if (!IsInDomain(this.lowerBound - offset))
			{
				result = DomainOperationResult.ElementNotInDomain;
				return;
			}

			this.size = T.One;
			this.upperBound = this.lowerBound;
			result = DomainOperationResult.InstantiateSuccessful;
		}

		public void Remove(T element, out DomainOperationResult result)
		{
			result = DomainOperationResult.EmptyDomain;
			if (element < -offset || !IsInDomain(element))
			{
				result = DomainOperationResult.ElementNotInDomain;
				return;
			}

			RemoveFromDomain(element);

			if (this.size == T.One)
			{
				this.size = T.Zero;
				this.lowerBound = this.upperBound + T.One;
				return;
			}

			if (element + offset == this.lowerBound)
			{
				while (this.lowerBound <= this.upperBound && !IsInDomain(this.lowerBound - offset))
				{
					++this.lowerBound;
					--this.size;
				}
			}
			else if (element + offset == this.upperBound)
			{
				while (this.upperBound >= this.lowerBound && !IsInDomain(this.upperBound - offset))
				{
					--this.upperBound;
					--this.size;
				}
			}

			if (this.lowerBound > this.upperBound || this.size == T.Zero)
				return;

			result = DomainOperationResult.RemoveSuccessful;
		}

		public override string ToString()
		{
			var domainRange = Enumerable.Range(int.CreateChecked(lowerBound),int.CreateChecked(upperBound - lowerBound + T.One))
				.Select(T.CreateChecked)
				.Select(x => x - offset)
				.Where(IsInDomain);

			return "[" + string.Join(", ", domainRange) + "]";
		}

		public bool Instantiated()
		{
			return this.upperBound == this.lowerBound;
		}

		public T Size()
		{
			return this.size;
		}

		public T LowerBound
		{
			get { return this.lowerBound - offset; }
		}

		public T UpperBound
		{
			get { return this.upperBound - offset; }
		}

		#endregion

		#region ICloneable Members

		public IDomain<T> Clone()
		{
			var clone = new DomainBinaryInteger<T> { domain = new T[this.domain.Length] };
			Array.Copy(this.domain, clone.domain, this.domain.Length);
			clone.lowerBound = this.lowerBound;
			clone.upperBound = this.upperBound;
			clone.size = this.size;
			clone.offset = this.offset;

			return clone;
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (var i = this.lowerBound - offset; i <= this.upperBound - offset; ++i)
				if (IsInDomain(i))
					yield return i;
		}

		#endregion
	}
}
