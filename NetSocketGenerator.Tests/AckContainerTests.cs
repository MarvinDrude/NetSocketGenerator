using NetSocketGenerator.Acknowledge;

namespace NetSocketGenerator.Tests;

public class AckContainerTests
{
   [Test]
   public async Task SimpleSuccess()
   {
      var container = new AckContainer();
      var id = Guid.CreateVersion7();
      
      var task = container.Enqueue<string>(id, TimeSpan.FromSeconds(10));
      container.TrySetResult(id, "TEST");

      var result = await task;
      
      await Assert.That(result).IsEqualTo("TEST");
      await Assert.That(container.PendingCount).IsEqualTo(0);
   }
   
   [Test]
   public async Task SimpleDelayedSuccess()
   {
      var container = new AckContainer();
      var id = Guid.CreateVersion7();
      
      var task = container.Enqueue<string>(id, TimeSpan.FromSeconds(10));
      _ = Delayed();

      await Task.Delay(TimeSpan.FromMilliseconds(100));
      await Assert.That(container.PendingCount).IsEqualTo(1);

      var result = await task;
      
      await Assert.That(result).IsEqualTo("TEST");
      await Assert.That(container.PendingCount).IsEqualTo(0);

      async Task Delayed()
      {
         await Task.Delay(TimeSpan.FromSeconds(1));
         container.TrySetResult(id, "TEST");
      }
   }
   
   [Test]
   public async Task SimpleDelayedTimeout()
   {
      var container = new AckContainer();
      var id = Guid.CreateVersion7();
      
      var task = container.Enqueue<string>(id, TimeSpan.FromSeconds(0.6));
      _ = Delayed();

      await Task.Delay(TimeSpan.FromMilliseconds(100));
      await Assert.That(container.PendingCount).IsEqualTo(1);

      var result = await task;
      
      await Assert.That(result).IsEqualTo(null);
      await Assert.That(container.PendingCount).IsEqualTo(0);

      async Task Delayed()
      {
         await Task.Delay(TimeSpan.FromSeconds(1));
         container.TrySetResult(id, "TEST");
      }
   }
}