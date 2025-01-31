namespace Decider.Tests.Example;

using Decider.Csp.BaseTypes;
using Decider.Example.TeacherTimetable;
using Xunit;

public class TeacherTimetableTest
{
  [Fact]
  public void TeacherTimetable_Solved()
  {
    var state = TeacherTimetable.GetTimetableTeachersState(out _);

    var result = state.Search(10);

    Assert.Equal(result, StateOperationResult.Solved);
  }

  [Fact]
  public void TeacherTimetable_Returns_Expected()
  {
    var state = TeacherTimetable.GetTimetableTeachersState(out _);

    _ = state.Search(10);

    Assert.Equal(state.Backtracks, 11);
    Assert.Equal(state.Solutions.Count, 1);
  }
}
