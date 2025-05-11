namespace NetSocketGenerator.CacheQueue.Client.Modules;

public sealed class LongModule(CacheQueueClient client)
{
   public Task<long?> Increment(string keyName)
   {
      return Add(keyName, 1);
   }
   
   public Task<long?> Decrement(string keyName)
   {
      return Add(keyName, -1);
   }
   
   public Task<long?> Subtract(string keyName, long value)
   {
      return Add(keyName, -value);
   }
   
   public Task<long?> Add(string keyName, long value)
   {
      var command = new AddLongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Add(command);
   }

   public async Task<long?> Add(AddLongCommand command)
   {
      command.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<AddLongCommandAck>(command.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.NewValue;
   }
   
   public Task<bool> Delete(string keyName)
   {
      var command = new DeleteCommand()
      {
         KeyName = keyName,
         AwaitsAck = true,
         StoreTypeSet = StoreTypes.Long
      };
      
      return Delete(command);
   }

   public async Task<bool> Delete(DeleteCommand command)
   {
      command.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<DeleteCommandAck>(command.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.WasFound ?? false;
   }
   
   public Task<long?> Get(string keyName)
   {
      var command = new GetLongCommand()
      {
         KeyName = keyName,
         AwaitsAck = true
      };

      return Get(command);
   }

   public async Task<long?> Get(GetLongCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<GetLongCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value;
   }

   public Task<bool> Set(string keyName, long value)
   {
      var command = new SetLongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Set(command);
   }

   public async Task<bool> Set(SetLongCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<SetLongCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value is not null;
   }

   public void SetNoAck(string keyName, long value)
   {
      var command = new SetLongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      };
      SetNoAck(command);
   }

   public void SetNoAck(SetLongCommand command)
   {
      command.AwaitsAck = false;
      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
   }
}