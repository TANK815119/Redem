using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Functions
{
    internal class Exponentiate : Function
    {
        private Function function1;
        private Function function2;

        public Exponentiate(Function function1, Function function2)
        {
            this.function1 = function1;
            this.function2 = function2;
        }

        public override double calculate(double x, double y)
        {
            return Math.Pow(function1.calculate(x, y), function2.calculate(x, y));
        }

    }
}
