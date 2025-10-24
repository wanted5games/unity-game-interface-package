using System.Collections;
using System.Threading.Tasks;

public static class TaskExtensions
{
    public static IEnumerator WaitForTask(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
            throw task.Exception;
    }
}