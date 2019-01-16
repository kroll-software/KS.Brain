using System;

namespace KS.Brain
{
	public static class MathHelpers
	{
		public static double Sigmoid(this double input, double response)
		{
			return (1d / (1d + Math.Exp(-input / response)));
		}

		public static float Sigmoid(this float input, float response)
		{
			return (1f / (1f + (float)Math.Exp(-input / response)));
		}

		public static double Erf(this double x)
		{
			// constants
			double a1 = 0.254829592;
			double a2 = -0.284496736;
			double a3 = 1.421413741;
			double a4 = -1.453152027;
			double a5 = 1.061405429;
			double p = 0.3275911;

			// Save the sign of x
			int sign = 1;
			if (x < 0)
				sign = -1;
			x = Math.Abs(x);

			// A&S formula 7.1.26
			double t = 1.0 / (1.0 + p*x);
			double y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*Math.Exp(-x*x);

			return sign*y;
		}

		public static double Erfc(this double x)
		{
			return 1.0 - Erf (x);
		}
	}
}

