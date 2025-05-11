namespace NetSocketGenerator.Utils;

public static class TaskUtils
{
   public static async Task WhenAllCatchAll(List<Task> tasks)
   {
      var taskAll = Task.WhenAll(tasks);

      try
      {
         await taskAll;
      }
      catch (Exception)
      {
         if (taskAll.Exception is { } error)
         {
            throw error;
         }

         throw;
      }
   }

   public static async Task WhenAllCatchAll<T>(List<Task<T>> tasks)
   {
      var taskAll = Task.WhenAll(tasks);

      try
      {
         await taskAll;
      }
      catch (Exception)
      {
         if (taskAll.Exception is { } error)
         {
            throw error;
         }

         throw;
      }
   }
}