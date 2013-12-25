using NUnit.Framework;

[TestFixture]
public class TrafficLight
{    
    [Test]
	public void TestShould_1_HighlightTestMethodWithRed()
    {
		Assert.Fail("Red highlight");
	}

    [Test]
    [Ignore]
	public void TestShould_2_HighlightTestMethodWithYellow()
    {
    }

	[Test]
	public void TestShould_3_HighlightTestMethodWithGreen()
	{
		int i = 1;
		Assert.AreEqual(1, i);
	}
}
