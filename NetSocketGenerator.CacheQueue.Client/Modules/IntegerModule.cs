﻿namespace NetSocketGenerator.CacheQueue.Client.Modules;

public sealed class IntegerModule(CacheQueueClient client)
{
   public Task<int?> Increment(string keyName)
   {
      return Add(keyName, 1);
   }
   
   public Task<int?> Decrement(string keyName)
   {
      return Add(keyName, -1);
   }
   
   public Task<int?> Subtract(string keyName, int value)
   {
      return Add(keyName, -value);
   }
   
   public Task<int?> Add(string keyName, int value)
   {
      var command = new AddIntegerCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Add(command);
   }

   public async Task<int?> Add(AddIntegerCommand command)
   {
      command.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<AddIntegerCommandAck>(command.RequestId, client.Options.ServerAckTimeout);
      
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
         StoreTypeSet = StoreTypes.Integer
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
   
   public Task<int?> Get(string keyName)
   {
      var command = new GetIntegerCommand()
      {
         KeyName = keyName,
         AwaitsAck = true
      };

      return Get(command);
   }

   public async Task<int?> Get(GetIntegerCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<GetIntegerCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value;
   }

   public Task<bool> Set(string keyName, int value)
   {
      var command = new SetIntegerCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Set(command);
   }

   public async Task<bool> Set(SetIntegerCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<SetIntegerCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value is not null;
   }

   public void SetNoAck(string keyName, int value)
   {
      var command = new SetIntegerCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      };
      SetNoAck(command);
   }

   public void SetNoAck(SetIntegerCommand command)
   {
      command.AwaitsAck = false;
      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
   }
}