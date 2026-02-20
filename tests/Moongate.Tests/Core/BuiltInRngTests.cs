using Moongate.Core.Random;
using NUnit.Framework;
using ShaiRandom.Generators;

namespace Moongate.Tests.Core;

public class BuiltInRngTests
{
    [Test]
    public void Generator_ShouldImplementIEnhancedRandom()
    {
        Assert.That(BuiltInRng.Generator, Is.AssignableTo<IEnhancedRandom>());
    }
}
