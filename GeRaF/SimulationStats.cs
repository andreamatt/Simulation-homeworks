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
		public List<BaseStat> dutyLambdas = new List<BaseStat>();
		public List<BaseStat> lambdaNs = new List<BaseStat>();
		public List<BaseStat> shapeStats = new List<BaseStat>();
	}

	class BaseStat
	{
		public ProtocolVersion protocolVersion;
		public double duty;
		public double lambda;
		public double N;
		public EmptyRegionType shape;
		public List<double> delay = new List<double>();
		public List<double> success = new List<double>();
		public List<double> energy = new List<double>();
		public List<List<double>> traffic = new List<List<double>>();
		public List<List<double>> failurePoints = new List<List<double>>();
	}
}
