
namespace NetSocketGenerator.Helpers;

/// <summary>
/// A custom implementation of a thread-safe work scheduler, designed to queue I/O tasks
/// and efficiently handle their execution using the thread pool. Inherits from
/// <see cref="PipeScheduler"/> and implements <see cref="IThreadPoolWorkItem"/>.
/// </summary>
internal sealed class IoQueue : PipeScheduler, IThreadPoolWorkItem 
{
   /// <summary>
   /// Represents an unbounded thread-safe channel used to queue work items for the
   /// IoQueue scheduler. The queued work items are primarily I/O tasks encapsulated
   /// as <see cref="Work"/> objects. This channel is used to manage and transfer tasks
   /// between producers and consumers in the IoQueue's scheduling system.
   /// </summary>
   private readonly Channel<Work> _workItems = Channel.CreateUnbounded<Work>();

   /// <summary>
   /// An integer flag indicating whether the IoQueue is currently processing work.
   /// The value of 1 represents that work processing is active, and 0 indicates idle state.
   /// This flag is manipulated using atomic operations to ensure thread safety and to
   /// coordinate the scheduling and execution of queued tasks within the IoQueue.
   /// </summary>
   private int _doingWork;

   /// <summary>
   /// Schedules a work item for execution, adding it to the queue for processing.
   /// If the IoQueue is not currently processing items, it schedules itself to begin
   /// processing the queued work using the thread pool.
   /// </summary>
   /// <param name="action">
   /// The action to perform, represented as a delegate that takes an optional object as its state.
   /// </param>
   /// <param name="state">
   /// An optional state object to be passed to the action when it is invoked.
   /// </param>
   public override void Schedule(Action<object?> action, object? state)
   {
      _workItems.Writer.TryWrite(new Work(action, state));

      // Set working if it wasn't (via atomic Interlocked).
      if (Interlocked.CompareExchange(ref _doingWork, 1, 0) == 0) 
      {
         // Wasn't working, schedule.
         System.Threading.ThreadPool.UnsafeQueueUserWorkItem(this, preferLocal: false);
      }
   }

   /// <summary>
   /// Executes all pending work items in the queue. Implements the IThreadPoolWorkItem interface
   /// to integrate with the thread pool for efficient task execution. This method processes each
   /// queued work item by invoking its callback with the associated state and ensures thread-safe
   /// handling of the queue state. The processing continues until all work items are completed or
   /// no additional work is detected.
   /// </summary>
   void IThreadPoolWorkItem.Execute() 
   {
      while (true) 
      {
         while (_workItems.Reader.TryRead(out var item)) 
         {
            item.Callback(item.State);
         }

         // All work done.

         // Set _doingWork (0 == false) prior to checking IsEmpty to catch any missed work in interim.
         // This doesn't need to be volatile due to the following barrier (i.e. it is volatile).
         _doingWork = 0;

         // Ensure _doingWork is written before IsEmpty is read.
         // As they are two different memory locations, we insert a barrier to guarantee ordering.
         Thread.MemoryBarrier();

         // Check if there is work to do
         if (_workItems.Reader.Count == 0) 
         {
            // Nothing to do, exit.
            break;
         }

         // Is work, can we set it as active again (via atomic Interlocked), prior to scheduling?
         if (Interlocked.Exchange(ref _doingWork, 1) == 1) 
         {
            // Execute has been rescheduled already, exit.
            break;
         }

         // Is work, wasn't already scheduled so continue loop.
      }

   }

   /// <summary>
   /// Represents a unit of work that encapsulates a callback and its associated state.
   /// Used within the scheduling system to queue and execute operations effectively.
   /// </summary>
   private readonly struct Work(Action<object?> callback, object? state)
   {
      /// <summary>
      /// Represents the delegate to be invoked as part of a queued unit of work. The callback is
      /// associated with a specific operation or task, and it executes with the provided state
      /// within the IoQueue's scheduling system.
      /// </summary>
      public readonly Action<object?> Callback = callback;

      /// <summary>
      /// Represents the user-defined state object associated with a unit of work in the scheduling system.
      /// This state is passed to the callback when the work item is executed, providing contextual information
      /// needed for the operation.
      /// </summary>
      public readonly object? State = state;
   }
}