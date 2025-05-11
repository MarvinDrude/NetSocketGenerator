namespace NetSocketGenerator.CacheQueue.Tests;

public sealed class SimpleLocalULongTests
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

      await client.ULongs.Set(keyOne, 2000);
      await client.ULongs.Set(keyTwo, 1023143243);
      
      var valueOne = await client.ULongs.Get(keyOne);
      var valueTwo = await client.ULongs.Get(keyTwo);
      
      await Assert.That(valueOne).IsEqualTo((ulong)2000);
      await Assert.That(valueTwo).IsEqualTo((ulong)1023143243);
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

      await client.ULongs.Set(keyOne, 2000);
      await client.ULongs.Delete(keyOne);
      
      var valueOne = await client.ULongs.Get(keyOne);
      
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

      await Assert.That(await client.ULongs.Add(keyOne, 2000)).IsEqualTo(2000u);
      await Assert.That(await client.ULongs.Add(keyOne, 2000)).IsEqualTo(4000u);
      await Assert.That(await client.ULongs.Subtract(keyOne, 100)).IsEqualTo(3900u);
      await Assert.That(await client.ULongs.Increment(keyOne)).IsEqualTo(3901u);
      await Assert.That(await client.ULongs.Decrement(keyOne)).IsEqualTo(3900u);
      
      await Assert.That(await client.ULongs.Subtract(keyOne, 3901)).IsEqualTo(ulong.MaxValue);
   }
}