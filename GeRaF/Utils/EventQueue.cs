using GeRaF.Events;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Utils
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
			if (queue.Count(ev => ev == e) != 1) throw new Exception("WTF");
			queue.Remove(e);
		}

		public void Reschedule(Event e) {
			if (queue.Count(ev => ev == e) != 1) throw new Exception("WTF");
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
