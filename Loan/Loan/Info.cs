using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loan
{
    
    //TERMS = 1, X12 = 2, APR = 3, EFF = 4, PRINCIPAL = 5, PAYMENT = 6 ;
    class Info
    {
        public double Terms { get; set; }
        public double X12 { get; set; }
        public double Apr { get; set; }
        public double Eff { get; set; }
        public double Principal { get; set; }
        public double Payment { get; set; }
        public int SolveFor { get; set; }
 
        public Info(double terms = 48, double x12 = 12, double apr = 10.0, double eff = 10.471,
                    double principal = 15000.00, double payment = 380.44, int sf = 6)
        {
            Terms = terms;
            X12 = x12;
            Apr = apr;
            Eff = eff;
            Principal = principal;
            Payment = payment;
            SolveFor = sf;                      // 6 = Solve for Payment
        }
    }
}
