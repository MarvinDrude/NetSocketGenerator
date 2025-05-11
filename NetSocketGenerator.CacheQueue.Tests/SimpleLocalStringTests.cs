using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Strings;

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

      await client.Strings.Set(keyOne, "adsadsa");
      await client.Strings.Delete(keyOne);
      
      var valueOne = await client.Strings.Get(keyOne);
      
      await Assert.That(valueOne).IsEqualTo(null);
   }
   
   [Test, NotInParallel]
   [ClassDataSource<CacheQueueLocalServerFactory, CacheQueueLocalClientFactory>(Shared = [SharedType.None])]
   public async Task TestSimpleBatch(
      CacheQueueLocalServerFactory serverFactory,
      CacheQueueLocalClientFactory clientFactory)
   {
      const string keyOne = "test1";
      
      var server = serverFactory.Server;
      var client = clientFactory.Client;

      server.Start();
      client.Connect();

      var batch = await client
         .CreateBatch()
         .Strings.Set(keyOne, "1")
         .Strings.Delete(keyOne)
         .Strings.Set(keyOne, "2")
         .Send();

      await Assert.That(batch.GetAck<SetStringCommandAck>(2)!.Value).IsEqualTo("2");
   }
}