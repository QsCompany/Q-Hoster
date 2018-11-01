using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;

[assembly: SuppressIldasm]

[assembly: CompilerGenerated]
[module: UnverifiableCode]


namespace QServer.Core
{

    
    public delegate void TaskQueueCallback<T>(T value);
    public delegate void TaskQueueOnError<T>(T value, Exception e);

    public class Operation<T>
    {
        public  T Value;
        public object Stat;
        public DateTime time;
    }

    public class TaskQueue<T>
    {
        [ThreadStatic]
        public static TaskQueue<T> CurrentTask;
        private readonly object mutex = new object();
        private System.Collections.Generic.Queue<Operation<T>> queue = new System.Collections.Generic.Queue<Operation<T>>();
        private bool isExecuting = false;
        public TaskQueueCallback<Operation<T>> Callback;
        public TaskQueueOnError<Operation<T>> OnError;
        private Action invoke;
        private bool isPaused;
        public TaskQueue(TaskQueueCallback<Operation<T>> callback, TaskQueueOnError<Operation<T>> onerror)
        {
            Callback = callback;
            OnError = onerror;
            invoke = new Action(Invoke);
        }

        public int Count { get; internal set; }

        public void Add(T @object)
        {
            lock (mutex)
            {
                queue.Enqueue(new Operation<T>() { Value = @object, time = DateTime.Now });
                if (isExecuting || isPaused) return;
                isExecuting = true;
            }
            (new Task(invoke)).Start();
            //Task.Run(invoke);
        }
        public void Add(T @object,DateTime time,object stat=null)
        {
            lock (mutex)
            {
                queue.Enqueue(new Operation<T>() { Value = @object, time = time, Stat = stat });
                if (isExecuting || isPaused) return;
                isExecuting = true;
            }
            (new Task(invoke)).Start();
            //Task.Run(invoke);
        }
        private void Invoke()
        {
            CurrentTask = this;
            object stat = null;
            while (Next(out var o))
                try
                {
                    Callback(o);
                }
                catch (Exception e)
                {
                    OnErrro(o, stat, e);
                }
            if (continueWith != null)
            {
                try
                {
                    continueWith(this);
                }
                catch { }
                continueWith = null;
            }
            CurrentTask = null;
        }

        private void OnErrro(Operation< T> o, object stat,Exception e)
        {
            try
            {
                OnError(o, e);
            }
            catch
            {
            }
        }

        private bool Next(out Operation< T> a)
        {
            lock (mutex)
            {
                if (queue.Count > 0)
                {
                    isExecuting = true;
                    a = queue.Dequeue();
                    return true;
                }
                else
                {
                    isExecuting = false;
                    a = null;
                    return false;
                }
            }
        }
        private event Action<TaskQueue<T>> continueWith;
        public void ContinueWith(Action<TaskQueue<T>> action)
        {
            this.isPaused = true;
            lock (mutex)
                if (!isExecuting) goto act;
                else
                    continueWith += action;
            return;
            act:
            try
            {
                action(this);
            }
            catch { };
            lock (mutex)
                isPaused = false;
            action = null;
            this.Invoke();
        }

        internal void Dispose()
        {
        }
    }
}
