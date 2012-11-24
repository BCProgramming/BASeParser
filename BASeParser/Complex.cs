using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
    //defines immutable Complex class.

    public class Complex : IComparable<Complex>
    {
        private decimal _real = 0;
        private decimal _imag = 0;
        private decimal _rho = 0;
        private decimal _theta = 0;

        //static routines
        public static Complex Empty = new Complex();


        public static Complex FromPolar(decimal pRho, decimal pTheta)
        {
            return new Complex((double) pRho*Math.Cos((double) pTheta), (double) pRho*Math.Sin((double) pTheta));




        }

        //operator overloads...

        #region operator overloads

        #region Addition overloads.

        public static Complex operator +(Complex opa, Complex opb)
        {
            return opa.Add(opb);

        }

        public static Complex operator +(Complex opa, decimal opb)
        {
            return opa.Add(opb);

        }

        public static Complex operator +(Complex opa, double opb)
        {
            return opa.Add(opb);

        }

        public static Complex operator +(decimal opa, Complex opb)
        {
            return new Complex(opa).Add(opb);

        }

        public static Complex operator +(Double opa, Complex opb)
        {
            return new Complex(opa).Add(opb);

        }

        #endregion


        public static Complex operator -(Complex op)
        {
            return op.Negate();

        }

        #region subtraction overloads

        public static Complex operator -(Complex opa, Complex opb)
        {
            return opa.Subtract(opb);

        }

        public static Complex operator -(Complex opa, decimal opb)
        {
            return opa.Subtract(opb);

        }

        public static Complex operator -(Complex opa, double opb)
        {
            return opa.Subtract(opb);

        }

        public static Complex operator -(decimal opa, Complex opb)
        {
            return new Complex(opa).Subtract(opb);

        }

        public static Complex operator -(double opa, Complex opb)
        {
            return new Complex(opa).Subtract(opb);

        }

        #endregion

        #region Multiplication overloads 

        public static Complex operator *(Complex opa, Complex opb)
        {
            return opa.Multiply(opb);


        }

        public static Complex operator *(Complex opa, decimal opb)
        {
            return opa.Multiply(opb);

        }

        public static Complex operator *(Complex opa, double opb)
        {
            return opa.Multiply(opb);

        }

        public static Complex operator *(decimal opa, Complex opb)
        {
            return new Complex(opa).Multiply(opb);


        }

        public static Complex operator *(double opa, Complex opb)
        {
            return new Complex(opa).Multiply(opb);


        }

        #endregion

        #region Division operator overloads

        public static Complex operator /(Complex opa, Complex opb)
        {

            return opa.Divide(opb);

        }

        public static Complex operator /(Complex opa, decimal opb)
        {
            return opa.Divide(opb);

        }

        public static Complex operator /(Complex opa, double opb)
        {
            return opa.Divide(opb);

        }

        public static Complex operator /(decimal opa, Complex opb)
        {
            return new Complex(opa).Divide(opb);

        }

        public static Complex operator /(double opa, Complex opb)
        {

            return new Complex(opa).Divide(opb);

        }

        #endregion

        public static Complex operator ~(Complex opA)
        {
            return opA.Conjugate;

        }

        #endregion

        private void CalcPolar()
        {
            var X = _real;
            var Y = _imag;
            if (X == 0 || Y == 0)
                _rho = _theta = 0;
            else
            {
                _rho = (decimal) Math.Sqrt(Math.Pow((double) X, 2) + Math.Pow((double) Y, 2));
                _theta = (decimal) Math.Atan2((double) Y, (double) X);
            }

        }

        /// <summary>
        /// returns the Real part of this complex number.
        /// </summary>
        /// 
        /// 
        /// 
        public decimal Realpart
        {
            get { return _real; }

        }

        /// <summary>
        /// returns the imaginary part of this complex number.
        /// </summary>
        public decimal Imagpart
        {
            get { return _imag; }

        }

        public Complex(decimal prealpart, decimal pimagpart)
        {
            _real = prealpart;
            _imag = pimagpart;

        }

        public Complex(decimal prealpart) : this(prealpart, 0)
        {
            _real = prealpart;

        }

        public Complex(double prealpart, double pimagpart)
        {
            _real = (decimal) prealpart;
            _imag = (decimal) pimagpart;
            CalcPolar();
        }

        public Complex(double prealpart) : this(prealpart, 0)
        {


        }

        private Complex() : this(0d)
        {
        }

        public Complex Negate()
        {
            return new Complex(-_real, -_imag);
        }

        public Complex Add(Complex othervalue)
        {
            return new Complex(_real + othervalue.Realpart, _imag + othervalue.Imagpart);

        }

        public Complex Add(decimal othervalue)
        {
            return new Complex(_real + othervalue, _imag);

        }

        public Complex Add(double othervalue)
        {
            return Add((decimal) othervalue);

        }

        public Complex Subtract(Complex othervalue)
        {
            return Add(othervalue.Negate());

        }

        public Complex Subtract(decimal othervalue)
        {
            return Add(new Complex(-othervalue));

        }

        public Complex Subtract(double othervalue)
        {
            return Subtract((decimal) othervalue);

        }

        public Complex Multiply(Complex Multiplier)
        {
            decimal re_part = _real*Multiplier.Realpart -
                              _imag*Multiplier.Imagpart;
            decimal im_part = _real*Multiplier.Imagpart +
                              _imag*Multiplier.Realpart;
            return new Complex(re_part, im_part);

        }

        public Complex Multiply(decimal othervalue)
        {
            return Multiply(new Complex(othervalue));

        }

        public Complex Multiply(double othervalue)
        {
            return Multiply((decimal) othervalue);

        }

        public Complex Divide(double Divisor)
        {
            return Divide((decimal) Divisor);

        }

        public Complex Divide(decimal Divisor)
        {
            return Divide(new Complex(Divisor));

        }

        public Complex Divide(Complex Divisor)
        {
            var d = (decimal) ((Math.Pow((double) Divisor.Realpart, 2)) + Math.Pow((double) Divisor.Realpart, 2));
            if (d == 0) throw new DivideByZeroException();
            var u = ((_real*Divisor.Realpart) + (_imag*Divisor.Imagpart))/d;
            var v = ((_imag*Divisor.Realpart) - (_real*Divisor.Imagpart))/d;
            return new Complex(u, v);



        }

        /// <summary>
        /// Returns the Complex Exponential of this Complex Number.
        /// </summary>
        /// <returns></returns>
        public Complex EExponential()
        {
            var tempcalc = Math.Pow(Math.E, (double) _real);
            return new Complex(tempcalc*(Math.Cos((double) _imag)),
                               tempcalc*(Math.Sin((double) _imag)));

        }

        public Complex Ln(long K)
        {
            double X = (double) _real;
            double Y = (double) _imag;
            return new Complex((1/2)*Math.Log(Math.Pow(X, 2), Math.Pow(Y, 2)), (Math.Atan(Y/X) + 2*K*Math.PI));

        }

        public Complex Logarithm()
        {

            return new Complex((decimal) Math.Log((double) _rho), _theta);

        }

        public Complex Pow(Complex power)
        {


            return power.Multiply(Logarithm()).EExponential();









        }

        public Complex Conjugate
        {
            get { return new Complex(Realpart, -Imagpart); }

        }

        //now the fun bit... trig.


        public Complex Sin()
        {
            return new Complex(Math.Sin((double) _real)*Math.Cosh((double) _imag),
                               Math.Cos((double) _real)*Math.Sinh((double) _imag));



        }

        public Complex Cos()
        {
            return new Complex(Math.Cos((double) _real)*Math.Cosh((double) _imag),
                               -Math.Sin((double) _real)*Math.Sinh((double) _imag));


        }

        public Complex Tangent()
        {
            return new Complex(Math.Sin(2 * (double)_real) / (Math.Cos(2 * (double)_real) + Math.Cosh(2 * (double)_imag)),
                Math.Sinh(2 * (double)_imag) / (Math.Cos(2 * (double)_imag) + Math.Cosh(2 * (double)_imag)));


        }

        

        


        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
        ///                     Value 
        ///                     Meaning 
        ///                     Less than zero 
        ///                     This object is less than the <paramref name="other"/> parameter.
        ///                     Zero 
        ///                     This object is equal to <paramref name="other"/>. 
        ///                     Greater than zero 
        ///                     This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public int CompareTo(Complex other)
        {
            int comparereal = _real.CompareTo(other.Realpart);
            return comparereal != 0 ? comparereal : _imag.CompareTo(other.Imagpart);
        }

        public override string ToString()
        {
            return ToString("G");
        }
        public string ToString(string useformat)
        {

            if (_imag == 0) return _real.ToString(useformat);
            return _real.ToString(useformat) + (_imag < 0 ? " - " : " + ") + Math.Abs(_imag).ToString(useformat) + "i";

     
        }
        
       

    }
}
