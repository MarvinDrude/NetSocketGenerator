﻿namespace NetSocketGenerator.CacheQueue.Client.Modules;

public sealed class ULongModule(CacheQueueClient client)
{
   public Task<ulong?> Increment(string keyName)
   {
      return Add(keyName, 1);
   }
   
   public Task<ulong?> Decrement(string keyName)
   {
      return Add(keyName, -1);
   }
   
   public Task<ulong?> Subtract(string keyName, long value)
   {
      return Add(keyName, -value);
   }
   
   public Task<ulong?> Add(string keyName, long value)
   {
      var command = new AddULongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Add(command);
   }

   public async Task<ulong?> Add(AddULongCommand command)
   {
      command.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<AddULongCommandAck>(command.RequestId, client.Options.ServerAckTimeout);
      
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
         StoreTypeSet = StoreTypes.ULong
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
   
   public Task<ulong?> Get(string keyName)
   {
      var command = new GetULongCommand()
      {
         KeyName = keyName,
         AwaitsAck = true
      };

      return Get(command);
   }

   public async Task<ulong?> Get(GetULongCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<GetULongCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value;
   }

   public Task<bool> Set(string keyName, ulong value)
   {
      var command = new SetULongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Set(command);
   }

   public async Task<bool> Set(SetULongCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<SetULongCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value is not null;
   }

   public void SetNoAck(string keyName, ulong value)
   {
      var command = new SetULongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      };
      SetNoAck(command);
   }

   public void SetNoAck(SetULongCommand command)
   {
      command.AwaitsAck = false;
      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
   }
}