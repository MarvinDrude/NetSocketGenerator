namespace NetSocketGenerator.CacheQueue.Tests;

public class SimpleLocalDoubleTests
{
   [Test, NotInParallel]
   [ClassDataSource<CacheQueueLocalServerFactory, CacheQueueLocalClientFactory>(Shared = [SharedType.None])]
   public async Task TestSimpleTwoKeySetGet(
      CacheQueueLocalServerFactory serverFactory,
      CacheQueueLocalClientFactory clientFactory)
   {
      const string keyOne = "test1";
      const string keyTwo = "test2";
      
      var server = serverFactory.Server;
      var client = clientFactory.Client;

      server.Start();
      client.Connect();

      await client.Doubles.Set(keyOne, 2000d);
      await client.Doubles.Set(keyTwo, 1023143243d);
      
      var valueOne = await client.Doubles.Get(keyOne);
      var valueTwo = await client.Doubles.Get(keyTwo);
      
      await Assert.That(valueOne).IsEqualTo(2000d);
      await Assert.That(valueTwo).IsEqualTo(1023143243d);
   }

   [Test, NotInParallel]
   [ClassDataSource<CacheQueueLocalServerFactory, CacheQueueLocalClientFactory>(Shared = [SharedType.None])]
   public async Task TestSimpleDelete(
      CacheQueueLocalServerFactory serverFactory,
      CacheQueueLocalClientFactory clientFactory)
   {
      const string keyOne = "test1";
      
      var server = serverFactory.Server;
      var client = clientFactory.Client;

      server.Start();
      client.Connect();

      await client.Doubles.Set(keyOne, 2000d);
      await client.Doubles.Delete(keyOne);
      
      var valueOne = await client.Doubles.Get(keyOne);
      
      await Assert.That(valueOne).IsEqualTo(null);
   }
}