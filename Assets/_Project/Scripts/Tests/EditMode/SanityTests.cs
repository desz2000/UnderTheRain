using NUnit.Framework;
using UnderTheRain.Core;

namespace UnderTheRain.Tests.EditMode
{
    public class SanityTests
    {
        [Test]
        public void RuntimeAssembly_IsLoaded()
        {
            Assert.IsNotNull(typeof(GameBootstrap).Assembly);
        }
    }
}
