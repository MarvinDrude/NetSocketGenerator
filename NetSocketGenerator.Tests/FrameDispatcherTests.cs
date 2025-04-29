using NetSocketGenerator.Events;
using NetSocketGenerator.Tcp.Frames;

namespace NetSocketGenerator.Tests;

public sealed class FrameDispatcherTests
{
   [Test]
   public async Task SimpleOneHandlerLiteralFirstSuccess()
   {
      var fired = false;
      var dispatcher = new ServerFrameDispatcher();
      dispatcher.AddKeyHandler("test?", (connection, id, message) =>
      {
         fired = true;
         return Task.CompletedTask;
      });

      await dispatcher.Dispatch(new TcpFrame()
      {
         Identifier = "testA",
      }, null!);
      
      await Assert.That(fired).IsEqualTo(true);
   }
   
   [Test]
   public async Task SimpleOneHandlerLiteralFirstWithWildcardSuccess()
   {
      var fired = false;
      var dispatcher = new ServerFrameDispatcher();
      dispatcher.AddKeyHandler("test:*", (connection, id, message) =>
      {
         fired = true;
         return Task.CompletedTask;
      });

      await dispatcher.Dispatch(new TcpFrame()
      {
         Identifier = "test:AAAA43543´´sa?à",
      }, null!);
      
      await Assert.That(fired).IsEqualTo(true);
   }
   
   [Test]
   public async Task SimpleTwoHandlerLiteralFirstSuccess()
   {
      var fired = false;
      var firedTwo = false;
      
      var dispatcher = new ServerFrameDispatcher();
      dispatcher.AddKeyHandler("t?ester?", (connection, id, message) =>
      {
         fired = true;
         return Task.CompletedTask;
      });
      dispatcher.AddKeyHandler("tast[jk]", (connection, id, message) =>
      {
         firedTwo = true;
         return Task.CompletedTask;
      });

      await dispatcher.Dispatch(new TcpFrame()
      {
         Identifier = "tAesterA",
      }, null!);
      await dispatcher.Dispatch(new TcpFrame()
      {
         Identifier = "tastk",
      }, null!);
      
      await Assert.That(fired).IsEqualTo(true);
      await Assert.That(firedTwo).IsEqualTo(true);
   }
}