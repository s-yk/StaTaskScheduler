using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StaTaskScheduler
{
    public class StaTaskScheduler : TaskScheduler, IDisposable
    {

        /// <summary>
        /// Taskのコレクション
        /// </summary>
        private BlockingCollection<Task> _taskCollection;

        /// <summary>
        /// Scheduler Thread
        /// </summary>
        private readonly Thread _thread;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StaTaskScheduler()
        {
            this._taskCollection = new BlockingCollection<Task>();

            this._thread = new Thread(this.ThreadStart);
            this._thread.TrySetApartmentState(ApartmentState.STA);
            this._thread.IsBackground = true;
            if (!this._thread.IsAlive)
            {
                this._thread.Start();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this._taskCollection.CompleteAdding();
            this._taskCollection.Dispose();
            this._taskCollection = null;
        }

        /// <summary>
        /// Taskの配列を返す
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this._taskCollection.ToArray();
        }

        /// <summary>
        /// Taskをキュー（コレクション）に追加する
        /// </summary>
        /// <param name="task"></param>
        protected override void QueueTask(Task task)
        {
            this._taskCollection.Add(task);
        }

        /// <summary>
        /// Taskのインライン十個うを促す
        /// ただし、スケジューラのスレッドと同一である場合のみとする
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskWasPreviouslyQueued"></param>
        /// <returns></returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (this._thread != Thread.CurrentThread) return false;

            this.TryExecuteTask(task);
            return true;
        }

        /// <summary>
        /// キュー（コレクション）内のTaskを実行する
        /// スケジューラのスレッドで実行する
        /// </summary>
        private void ThreadStart()
        {
            foreach (var task in this._taskCollection.GetConsumingEnumerable())
            {
                this.TryExecuteTask(task);
            }
        }
    }
}
