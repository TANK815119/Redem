using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Functions
{
    internal class Constant : Function
    {
        private double value;
        public Constant(double value)
        {
            this.value = value;
        }

        public override double calculate(double x, double y)
        {
            return value;
        }
    }
}
