using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Longs;

namespace NetSocketGenerator.CacheQueue.Tests;

public sealed class SimpleLocalLongTests
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

      await client.Longs.Set(keyOne, 2000);
      await client.Longs.Set(keyTwo, 1023143243);
      
      var valueOne = await client.Longs.Get(keyOne);
      var valueTwo = await client.Longs.Get(keyTwo);
      
      await Assert.That(valueOne).IsEqualTo(2000);
      await Assert.That(valueTwo).IsEqualTo(1023143243);
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

      await client.Longs.Set(keyOne, 2000);
      await client.Longs.Delete(keyOne);
      
      var valueOne = await client.Longs.Get(keyOne);
      
      await Assert.That(valueOne).IsEqualTo(null);
   }
   
   [Test, NotInParallel]
   [ClassDataSource<CacheQueueLocalServerFactory, CacheQueueLocalClientFactory>(Shared = [SharedType.None])]
   public async Task TestSimpleAddSubtract(
      CacheQueueLocalServerFactory serverFactory,
      CacheQueueLocalClientFactory clientFactory)
   {
      const string keyOne = "test1";
      
      var server = serverFactory.Server;
      var client = clientFactory.Client;

      server.Start();
      client.Connect();

      await Assert.That(await client.Longs.Add(keyOne, 2000)).IsEqualTo(2000);
      await Assert.That(await client.Longs.Add(keyOne, 2000)).IsEqualTo(4000);
      await Assert.That(await client.Longs.Subtract(keyOne, 100)).IsEqualTo(3900);
      await Assert.That(await client.Longs.Increment(keyOne)).IsEqualTo(3901);
      await Assert.That(await client.Longs.Decrement(keyOne)).IsEqualTo(3900);
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
         .Longs.Set(keyOne, 10)
         .Longs.Add(keyOne, 5)
         .Longs.Subtract(keyOne, 2)
         .Longs.Increment(keyOne)
         .Longs.Decrement(keyOne)
         .Send();

      await Assert.That(batch.GetAck<AddLongCommandAck>(4)!.NewValue).IsEqualTo(13);
   }
}