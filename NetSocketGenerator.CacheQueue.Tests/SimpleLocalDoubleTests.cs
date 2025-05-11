using NetSocketGenerator.CacheQueue.Bucketing.Stores;
using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Doubles;

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

      await Assert.That(await client.Doubles.Add(keyOne, 2000d)).IsEqualTo(2000d);
      await Assert.That(await client.Doubles.Add(keyOne, 2000d)).IsEqualTo(4000d);
      await Assert.That(await client.Doubles.Subtract(keyOne, 100d)).IsEqualTo(3900d);
      await Assert.That(await client.Doubles.Increment(keyOne)).IsEqualTo(3901d);
      await Assert.That(await client.Doubles.Decrement(keyOne)).IsEqualTo(3900d);
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
         .Doubles.Set(keyOne, 10d)
         .Doubles.Add(keyOne, 5d)
         .Doubles.Subtract(keyOne, 2d)
         .Doubles.Increment(keyOne)
         .Doubles.Decrement(keyOne)
         .Send();

      await Assert.That(batch.GetAck<AddDoubleCommandAck>(4)!.NewValue).IsEqualTo(13);
   }
}