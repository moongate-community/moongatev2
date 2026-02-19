using Moongate.Scripting.Data.Scripts;

namespace Moongate.Tests.Scripting;

public class ScriptResultBuilderTests
{
    [Test]
    public void CreateError_ShouldBuildFailedResult()
    {
        var result = ScriptResultBuilder.CreateError()
                                        .WithMessage("error")
                                        .Build();

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("error"));
            }
        );
    }

    [Test]
    public void CreateSuccess_WithDataAndMessage_ShouldBuildExpectedResult()
    {
        var result = ScriptResultBuilder.CreateSuccess()
                                        .WithMessage("ok")
                                        .WithData(123)
                                        .Build();

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("ok"));
                Assert.That(result.Data, Is.EqualTo(123));
            }
        );
    }
}
