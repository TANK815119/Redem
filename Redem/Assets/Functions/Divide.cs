using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Functions
{
    internal class Divide : Function
    {
        private Function function1;
        private Function function2;

        public Divide(Function function1, Function function2)
        {
            this.function1 = function1;
            this.function2 = function2;
        }

        public override double calculate(double x, double y)
        {
            if (function2.calculate(x, y) == 0)
            {
                return calculate(x + 0.01, y + 0.01);
            }
            else
            {
                return function1.calculate(x, y) / function2.calculate(x, y);
            }
        }

    }
}

