using NUnit.Framework;
using Quartz;

namespace Quartz.Tests.Unit;

[TestFixture]
public class CronExpressionValidationTests
{
    [Test]
    public void ValidateExpression_RejectsMoreThanSevenFields()
    {
        const string expression = "0 0 12 * * ? 2026 extra";

        Assert.That(CronExpression.IsValidExpression(expression), Is.False);
        Assert.That(() => CronExpression.ValidateExpression(expression), Throws.TypeOf<System.FormatException>());
    }
}
