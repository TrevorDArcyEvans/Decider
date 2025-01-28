using System;
using Decider.Csp.BaseTypes;

namespace Decider.Tests.Csp;

using Xunit;
using Decider.Csp.Integer;

public class DomainBinaryIntegerTests
{
  private readonly DomainBinaryInteger<int> sut;

  // setup
  public DomainBinaryIntegerTests()
  {
    sut = new DomainBinaryInteger<int>(-100, 5000);
  }

  [Fact]
  public void Constructor_Invalid_Throws()
  {
    Assert.Throws<ArgumentException>(() => new DomainBinaryInteger<int>(-1));
  }

  [Fact]
  public void Constructor_Bits_Per_Datatype_Returns_Size()
  {
    var sut2 = new DomainBinaryInteger<int>(31);

    var size = sut2.Size();

    Assert.Equal(size, 32);
  }

  [Fact]
  public void Domain_Returns_Expected_Integers()
  {
    var domain = sut.Domain();

    Assert.Equal(domain[domain.Length - 1], 8191);
  }

  [Fact]
  public void Domain_Returns_Expected_Size()
  {
    var domain = sut.Domain();

    Assert.Equal(domain.Length, 160);
  }

  [Fact]
  public void Size_Returns_Expected_Size()
  {
    var size = sut.Size();

    Assert.Equal(size, 5101);
  }

  [Fact]
  public void InstantiatedValue_Throws()
  {
    Assert.Throws<DeciderException>(() => _ = sut.InstantiatedValue);
  }

  [Fact]
  public void InstantiateLowest_Fails()
  {
    var sut2 = new DomainBinaryInteger<int>(2, 1);

    sut2.InstantiateLowest(out var retVal);

    Assert.Equal(retVal, DomainOperationResult.ElementNotInDomain);
  }

  [Fact]
  public void Instantiate_Succeeds()
  {
    sut.Instantiate(out var retVal);

    Assert.Equal(retVal, DomainOperationResult.InstantiateSuccessful);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(100)]
  [InlineData(-11)]
  [InlineData(5000)]
  [InlineData(0)]
  [InlineData(-120)]
  public void Contains_Succeeds(int val)
  {
    var retVal = sut.Contains(val);

    Assert.Equal(retVal, true);
  }

  [Theory]
  [InlineData(5001)]
  [InlineData(5005)]
  [InlineData(5018)]
  public void Contains_Fails(int val)
  {
    var retVal = sut.Contains(val);

    Assert.Equal(retVal, false);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(100)]
  [InlineData(-11)]
  [InlineData(5000)]
  [InlineData(-120)]
  public void Instantiate_With_Value_Succeeds(int val)
  {
    sut.Instantiate(val, out var retVal);

    Assert.Equal(retVal, DomainOperationResult.InstantiateSuccessful);
  }

  [Theory]
  [InlineData(5001)]
  [InlineData(5012)]
  public void Instantiate_With_Value_Fails(int val)
  {
    sut.Instantiate(val, out var retVal);

    Assert.Equal(retVal, DomainOperationResult.ElementNotInDomain);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(100)]
  [InlineData(-100)]
  [InlineData(-11)]
  [InlineData(5000)]
  [InlineData(0)]
  public void Remove_Succeeds(int val)
  {
    sut.Remove(val, out var retVal);

    Assert.Equal(retVal, DomainOperationResult.RemoveSuccessful);
  }

  [Theory]
  [InlineData(5001)]
  [InlineData(5005)]
  [InlineData(5018)]
  public void Remove_Not_In_Domain_Returns_Expected(int val)
  {
    sut.Remove(val, out var retVal);

    Assert.Equal(retVal, DomainOperationResult.ElementNotInDomain);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(100)]
  [InlineData(-11)]
  [InlineData(5000)]
  [InlineData(-100)]
  public void Instantiate_With_Value_Remove_Returns_Expected(int val)
  {
    sut.Instantiate(val, out var _);

    sut.Remove(val, out var retVal);

    Assert.Equal(retVal, DomainOperationResult.EmptyDomain);
  }

  [Fact]
  public void Instantiate_InstantiatedValue_Expected()
  {
    sut.Instantiate(out var _);

    Assert.Equal(sut.InstantiatedValue, -100);
  }

  [Fact]
  public void Instantiate_Size_Expected()
  {
    sut.Instantiate(out var _);

    Assert.Equal(sut.Size(), 1);
  }

  [Fact]
  public void Instantiate_Instantiated_Expected()
  {
    sut.Instantiate(out var _);

    Assert.Equal(sut.Instantiated(), true);
  }

  [Fact]
  public void Instantiate_LowerBound_Expected()
  {
    sut.Instantiate(out var _);

    Assert.Equal(sut.LowerBound, -100);
  }

  [Fact]
  public void Instantiate_UpperBound_Expected()
  {
    sut.Instantiate(out var _);

    Assert.Equal(sut.UpperBound, -100);
  }

  [Fact]
  public void CreateDomain_Succeeds()
  {
    _ = DomainBinaryInteger<int>.CreateDomain(new[] { -10, -9, -7, 0, 1, 21, 5003 });
  }

  [Fact]
  public void CreateDomain_Throws()
  {
    Assert.Throws<ArgumentException>(() => DomainBinaryInteger<int>.CreateDomain(2,1));
  }
}
