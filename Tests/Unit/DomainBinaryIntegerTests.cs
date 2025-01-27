namespace Decider.Tests.Csp;

using Xunit;
using Decider.Csp.Integer;

public class DomainBinaryIntegerTests
{
  [Fact]
  public void Domain_Returns_Expected_Integers()
  {
    var sut = new DomainBinaryInteger(-100, 5000);

    var domain = sut.Domain();

    Assert.Equal(domain[domain.Length - 1], 8191u);
  }
  [Fact]
  public void Domain_Returns_Expected_Size()
  {
    var sut = new DomainBinaryInteger(-100, 5000);

    var domain = sut.Domain();

    Assert.Equal(domain.Length, 160);
  }
}
