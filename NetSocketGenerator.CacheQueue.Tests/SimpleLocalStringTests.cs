namespace NetSocketGenerator.CacheQueue.Tests;

public sealed class SimpleLocalStringTests
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

      await client.Strings.Set(keyOne, "key1value");
      await client.Strings.Set(keyTwo, "key2value");
      
      var valueOne = await client.Strings.Get(keyOne);
      var valueTwo = await client.Strings.Get(keyTwo);
      
      await Assert.That(valueOne).IsEqualTo("key1value");
      await Assert.That(valueTwo).IsEqualTo("key2value");
   }
}