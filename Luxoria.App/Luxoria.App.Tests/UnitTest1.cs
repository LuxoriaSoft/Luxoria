using Luxoria.Modules;
using Luxoria.Modules.Interfaces;

namespace Luxoria.App.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestBusExist()
        {
        
            IEventBus eventBus = new EventBus();
        
            Assert.NotNull(eventBus);
        }
    }
}