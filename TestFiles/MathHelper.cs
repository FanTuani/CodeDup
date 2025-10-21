using System;

namespace MathUtils
{
    public class MathHelper
    {
        public static int Add(int a, int b)
        {
            return a + b;
        }
        
        public static int Subtract(int a, int b)
        {
            return a - b;
        }
        
        public static int Multiply(int a, int b)
        {
            return a * b;
        }
        
        public static double Divide(int a, int b)
        {
            if (b == 0)
                throw new DivideByZeroException("Cannot divide by zero");
            return (double)a / b;
        }
        
        public static int Power(int baseNumber, int exponent)
        {
            return (int)Math.Pow(baseNumber, exponent);
        }
        
        public static double SquareRoot(int number)
        {
            if (number < 0)
                throw new ArgumentException("Cannot calculate square root of negative number");
            return Math.Sqrt(number);
        }
    }
}
