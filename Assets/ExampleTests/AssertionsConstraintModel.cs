using NUnit.Framework;

[TestFixture]
public class AssertionsConstraintModel : AssertionHelper
{
    [Test]
	public void TestShould_UseComparisonConstraints()
    { 
		int myInt = 42;

		Assert.That(myInt, Is.GreaterThan(40) & Is.LessThan(45));

		Expect(myInt, GreaterThan(40).And.LessThan(45));
    }

	[Test]
	public void TestShould_UseStringConstraints()
	{ 
		string phrase = "Make your tests fail before passing!";

		Expect(phrase, Contains("tests fail").And.Contains("make").IgnoreCase);
	}
}
