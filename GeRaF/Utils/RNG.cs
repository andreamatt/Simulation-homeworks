﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	static class RNG
	{
		static private Random random = new Random((int)DateTime.Now.Ticks);
		static public double rand() {
			return random.NextDouble();
		}

		static public double rand_expon(double lambda) {
			return -Math.Log(rand()) / lambda;
		}

		static public int rand_int(int min, int max) {
			return random.Next(min, max);
		}
	}
}
