using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class RelayInfo
	{
		public int id;
		public double X;
		public double Y;
		public double awakeTime;
		public double asleepTime;

		public override string ToString() {
			return $"{id}|{X}|{Y}|{awakeTime}|{asleepTime}";
		}
	}

	class RunResult
	{
		public ProtocolParameters basePP;
		public SimulationParameters baseSP;
		public List<DutyLambdaStat> dutyLambdas;
		public List<LambdaNStat> lambdaNs;
	}

	class DutyLambdaStat
	{
		public double duty;
		public double lambda;
		public List<double> delay = new List<double>();
		public List<double> success = new List<double>();
		public List<double> energy = new List<double>();
	}

	class LambdaNStat
	{
		public double lambda;
		public double N;
		public List<double> delay = new List<double>();
		public List<double> success = new List<double>();
		public List<double> energy = new List<double>();
	}
}
