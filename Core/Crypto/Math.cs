namespace QServer.Reporting
{
    public class Math
    {
        const double π = System.Math.PI;
        const double π2 = π / 2;
        const double π4 = π / 4;

        public static double Sin(double x)
        {

            if (x == 0) { return 0; }
            if (x < 0) { return -Sin(-x); }
            if (x > π) { return -Sin(x - π); }
            if (x > π4) { return Cos(π2 - x); }

            double x2 = x * x;

            return x * (x2 / 6 * (x2 / 20 * (x2 / 42 * (x2 / 72 * (x2 / 110 * (x2 / 156 - 1) + 1) - 1) + 1) - 1) + 1);
        }

        public static double Cos(double x)
        {
            if (x == 0) { return 1; }
            if (x < 0) { return Cos(-x); }
            if (x > π) { return -Cos(x - π); }
            if (x > π4) { return Sin(π2 - x); }

            double x2 = x * x;

            return x2 / 2 * (x2 / 12 * (x2 / 30 * (x2 / 56 * (x2 / 90 * (x2 / 132 - 1) + 1) - 1) + 1) - 1) + 1;
        }
    }
}