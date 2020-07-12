using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Utils
{
	static class RNG
	{
		static private readonly Random _global = new Random((int)DateTime.Now.Ticks);
		[ThreadStatic]
		static private Random _local;
		static public double rand() {
			if (_local == null) {
				lock (_global) {
					if (_local == null) {
						int seed = _global.Next();
						_local = new Random(seed);
					}
				}
			}
			return _local.NextDouble();
		}

		static public double rand_expon(double lambda) {
			return -Math.Log(rand()) / lambda;
		}

		static public int rand_int(int min, int max) {
			return _local.Next(min, max);
		}
	}
}
