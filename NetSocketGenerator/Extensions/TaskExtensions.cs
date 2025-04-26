namespace NetSocketGenerator.Extensions;

/// <summary>
/// Provides extension methods for tasks to enhance functionality without modifying their behavior.
/// </summary>
public static class TaskExtensions
{
   /// <summary>
   /// Ensures that a Task is executed without awaiting its result, while safely handling potential exceptions.
   /// </summary>
   /// <param name="task">The Task to be executed without awaiting. If the Task encounters a fault,
   /// its exception is observed to prevent unhandled task exceptions.</param>
   public static void PipeFireAndForget(this Task task)
   {
      task?.ContinueWith(t => GC.KeepAlive(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
   }
}