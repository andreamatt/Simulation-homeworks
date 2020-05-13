using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
    class EventQueue
    {
        private FastPriorityQueue<Event> queue;

        public void Add(Event e) {
            queue.Enqueue(e, e.time);
        }

        public Event Head() {
            return queue.First;
        }

        public Event Pop() {
            return queue.Dequeue();
        }

        public bool isEmpty => queue.Count == 0;

        public void Clear() {
            queue.Clear();
        }
    }
}
