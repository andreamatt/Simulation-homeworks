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
		public List<DutyLambdaStat> dutyLambdas = new List<DutyLambdaStat>();
		public List<LambdaNStat> lambdaNs = new List<LambdaNStat>();
		public List<VersionStat> donuts = new List<VersionStat>();
		public List<VersionStat> squares = new List<VersionStat>();
	}

	abstract class BaseStat
	{
		public List<double> delay = new List<double>();
		public List<double> success = new List<double>();
		public List<double> energy = new List<double>();
		public List<List<double>> traffic = new List<List<double>>();
		public List<List<double>> failurePoints = new List<List<double>>();
	}

	class DutyLambdaStat : BaseStat
	{
		public double duty;
		public double lambda;
	}

	class LambdaNStat : BaseStat
	{
		public double lambda;
		public double N;
	}

	class VersionStat : BaseStat
	{
		public ProtocolVersion protocolVersion;
	}
}
