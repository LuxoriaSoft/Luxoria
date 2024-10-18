using Luxoria.Modules;
using Luxoria.Modules.Interfaces;

namespace Luxoria.App.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Assert
            
            Assert.True(true);
            
        }
        [Fact]
        public void Test2()
        {
            // Assert
        
            IEventBus eventBus = new EventBus();
        
            Assert.NotNull(eventBus);
        }
    }
}