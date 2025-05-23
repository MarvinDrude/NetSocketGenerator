﻿namespace NetSocketGenerator.CacheQueue.Client.Modules;

public sealed class DoubleModule(CacheQueueClient client)
{
   public Task<double?> Increment(string keyName)
   {
      return Add(keyName, 1);
   }
   
   public Task<double?> Decrement(string keyName)
   {
      return Add(keyName, -1);
   }
   
   public Task<double?> Subtract(string keyName, double value)
   {
      return Add(keyName, -value);
   }
   
   public Task<double?> Add(string keyName, double value)
   {
      var command = new AddDoubleCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Add(command);
   }

   public async Task<double?> Add(AddDoubleCommand command)
   {
      command.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<AddDoubleCommandAck>(command.RequestId, client.Options.ServerAckTimeout);
      
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
         StoreTypeSet = StoreTypes.Double
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
   
   public Task<double?> Get(string keyName)
   {
      var command = new GetDoubleCommand()
      {
         KeyName = keyName,
         AwaitsAck = true
      };

      return Get(command);
   }

   public async Task<double?> Get(GetDoubleCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<GetDoubleCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value;
   }

   public Task<bool> Set(string keyName, double value)
   {
      var command = new SetDoubleCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = true
      };

      return Set(command);
   }

   public async Task<bool> Set(SetDoubleCommand command)
   {
      command.AwaitsAck = true;

      var task = client.AckContainer
         .Enqueue<SetDoubleCommandAck>(command.RequestId, client.Options.ServerAckTimeout);

      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
      var result = await task;

      return result?.Value is not null;
   }

   public void SetNoAck(string keyName, double value)
   {
      var command = new SetDoubleCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      };
      SetNoAck(command);
   }

   public void SetNoAck(SetDoubleCommand command)
   {
      command.AwaitsAck = false;
      client.Tcp.Send<BaseCommand>(EventNames.Command, command);
   }
}