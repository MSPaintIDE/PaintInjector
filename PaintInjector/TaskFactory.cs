using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFramework
{
    public class TaskFactory {
        private static HashSet<Task> _tasks = new HashSet<Task>();

        public static Task<T> StartNew<T>(Func<T> func) {
            var t = Task.Factory.StartNew(func);
            t.ContinueWith(x => {
                lock(_tasks) {
                    _tasks.Remove(x);
                }
            });
            lock(_tasks) {
                _tasks.Add(t);
            }
            return t;
        }

        public static void WaitAll() {
            Task[] tasks;
            lock(_tasks) {
                tasks = _tasks.ToArray();
            }
            Task.WaitAll(tasks);
        }
    }
}