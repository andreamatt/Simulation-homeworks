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
		private SimplePriorityQueue<Event, double> queue = new SimplePriorityQueue<Event, double>();

		public void Add(Event e) {
			queue.Enqueue(e, e.time);
		}

		public Event Head() {
			return queue.First;
		}

		public Event Pop() {
			return queue.Dequeue();
		}

		public void Remove(Event e) {
			queue.Remove(e);
		}

		public void Reschedule(Event e) {
			queue.UpdatePriority(e, e.time);
		}

		public bool isEmpty => queue.Count == 0;

		public void Clear() {
			queue.Clear();
		}

		public List<Event> ToList() {
			return queue.ToList();
		}
	}
}
