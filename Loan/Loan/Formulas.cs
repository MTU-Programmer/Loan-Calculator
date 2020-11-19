using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loan
{
    class Formulas
    {
        public const int TERMS = 1, X12 = 2, APR = 3, EFF = 4, PRINCIPAL = 5, PAYMENT = 6;
        public Info info { get; set; }
        public double[] mi, mp, mb;         //monthly interest payment, monthly principal payment, 
                                            //monthly balance remaining on the loan
        public double[] tmi, tmp;
        int numYears;

        /*---------------------------------------------
         * Convert Eff rate to Apr rate.
         * Given the effective rate (true rate) return the apr (nominal rate)
         * 
         */
        private double EffToApr(double eff, double x12 = 12)
        {
            double x, y, apr;

            x = 1.0 + (eff / 100.0);
            y = 1.0 / x12;
            apr = (Math.Pow(x, y) - 1) * x12 * 100.0;
            return apr;
        }
        /*---------------------------------------------
         * The formula and calculations are as follows: 
         * Effective annual interest rate = 
         * (1 + (nominal rate / number of compounding periods)) ^ (number of compounding periods) - 1. 
         * For investment A, this would be: 10.47% = (1 + (10% / 12)) ^ 12 - 1
         * Convert Apr to Eff.
         * Given the nominal rate (apr) return the effective rate (true rate)
         */
        private double AprToEff(double apr, double x12 = 12)
        {
            double x, y, eff;

            x = 1 + (apr / 100 / x12);
            y = x12;
            eff = (Math.Pow(x, y) - 1) * 100;
            return eff;
        }
        private void checkInterestRate()
        {
            if (info.Apr == 0.0 && info.Eff != 0.0)
            {
                info.Apr = EffToApr(info.Eff, info.X12);
            }
            else if (info.Eff == 0.0 && info.Apr != 0.0)
            {
                info.Eff = AprToEff(info.Apr, info.X12);
            }

        }
        //------------------------------------------------------------------------
        // calculate the apr rate (nominal rate)
        // Entry:
        //    p     = principle  (the amount borrowed)
        //    terms = total # of payments
        //    x12   = compounding periods per year, usually 12
        //		pmt   = amount of each payment
        //
        //Exit:
        // results are to 3 digits after decimal point
        // for example, if p = €4667, terms = 48 months, and periodic pmt = €127
        // the result is ARP 13.772%

        double iRate(double p, double terms, double x12, double pmt)
        {
            double app, x, y, lastopr, ny, np, stp, Pmt, i, result;

            app = pmt; //app = the payment we are to target
            i = .001;

            stp = 0.01;
            lastopr = 1;  //last operation on stp: -1 = smaller, +1 = bigger


        top:

            i = i + (stp * lastopr);
            //calculate payment
            x = (i / x12 + 1);
            y = -1 * terms;
            x = 1 - Math.Pow(x, y);
            Pmt = (p * (i / x12)) / x;
            //round payment
            if ((Pmt >= (app - .0001)) && (Pmt <= (app + .0001))) goto found;

            if (Pmt < app)
            {

                if (lastopr == -1)
                {
                    stp = stp / 2;
                    lastopr = 1;
                    goto top;
                }

                stp = stp * 2;
                goto top;
            }

            if (Pmt > app)
            {
                if (lastopr == 1)
                {
                    stp = stp / 2;
                    lastopr = -1;
                    goto top;
                }

                lastopr = -1;

                goto top;
            }


        found:
            i = i * 100;

            result = i;
        ex:
            return result;   //nominal rate  (ie., 13.772%)
        }

        /*---------------------------------------------
         * Return the present value of a loan.
         */
        private double presentValue(double pmt, double apr, double terms, double x12)
        {

            double i, x, y, p;

            i = apr / 100;
            x = (i / x12 + 1);
            y = -1 * terms;
            x = (1 - Math.Pow(x, y)) * pmt;
            p = x * x12 / i;

            return p;
        }

        /*---------------------------------------------

         This function finds the amount of time in months needed to
         repay a loan
         Entry:
            p = principal (the amount borrowed)
            arp = the yearly interest rate (ie., 13.772%)
            pmt = the amount of each payment (ie., €127)
            x12 = compounding periods per year
        Exit:
            the result is in info.Terms (ie., 48 months )        
        */

        private void calcTerms()
        {
            double p = info.Principal;
            double apr = info.Apr;
            double pmt = info.Payment;
            double x12 = this.info.X12;
            double t, i, x1, x2, n;

            // Make sure Eff rate agrees with Apr rate.
            info.Eff = AprToEff(apr, info.X12);

            n = x12;                            //assume 12 payments per year
            i = apr / 100;
            x1 = 1 - ((p * i) / (n * pmt));
            x2 = 1 + (i / n);
            t = (-(Math.Log(x1) / (n * Math.Log(x2)))) * x12;
            info.Terms = t;
        }

        /*----------------------------------------
         * Find out how many times the interest on a loan is being compounded.
         * 
         * There is a problem with this function...
         */
        private void calcX12()
        {
            double pmt = info.Payment;
            double apr = info.Apr;
            double terms = info.Terms;
            double principal = info.Principal;
            double x12 = 0;
            double p;

            for (int i = 1; i < 54; i += 1)
            {
                x12 = i;
                p = presentValue(pmt, apr, terms, x12);
                if (p >= principal)
                {
                    break;
                }

            }
            info.X12 = x12;
            info.Eff = AprToEff(info.Apr, info.X12);

        }
        /*-------------------------------------------
         * Find the Apr rate.
         * Assume the following:
         * Principal    = 4667.00
         * Terms        = 48 months
         * X12          = 12 interest is compounded 12 times per year
         * Payment      127.00  loan repayment per month
         * Therefore Apr = 13.772%
         */
        private void calcApr()
        {
            info.Apr = iRate(info.Principal, info.Terms, info.X12, info.Payment);
            info.Eff = AprToEff(info.Apr, info.X12);
        }
        /*----------------------------------------
         *  Also known as Pv = find the present value of a loan
		    given pmt, apr, terms, x12 as input variables.
		    -- double Pv(double pmt, double apr, double terms, double x12) --
		    suppose you know a loan of x at an nominal rate of 13.772% over 48 months
		    will cost 6096 what is the present value of the loan today,
		    in this example the present value is x = 4667
		    Amount to borrow is the principal.

         */
        private void calcPrincipal()
        {
            double i, x, y, p;

            checkInterestRate();
            //info.Eff = AprToEff(info.Apr, info.X12);
            i = info.Apr / 100;
            x = (i / info.X12 + 1);
            y = -1 * info.Terms;
            x = (1 - Math.Pow(x, y)) * info.Payment;

            p = x * info.X12 / i;
            info.Principal = p;

        }
        /*----------------------------------------
         * This function calculates the amount of the monthly payment
            Entry:
                p     = principal (ie., 999)
                apr   = (ie., 41.94%)
                terms = total # of monthly payments
                x12   = compounding periods per year
            Exit:
                result = €71.74
         */
        private void calcPayment()
        {
            double x, y, Pmt, i;
            checkInterestRate();
           i = info.Apr / 100;

            x = i / info.X12 + 1;
            y = -1 * info.Terms;
            x = 1 - Math.Pow(x, y);
            Pmt = (info.Principal * (i / info.X12)) / x;
            info.Payment = Pmt;


        }
        /*----------------------------------------
         * Constructor
         */
        public Formulas()
        {
            //Sets up default values
            this.info = new Info();
            
        }

        /*---------------------------------
         * Based on the value of info.SolveFor brance to perform the appropriate calculation.
         * Then put the results into this.info. The info object can be accessed publically in order
         * to return the results to the caller for display.
         */
        private void solveForSwitch()
        {
            switch (info.SolveFor)
            {
                case TERMS:
                    this.calcTerms();
                    break;
                case X12:
                    this.calcX12();
                    break;
                case APR:
                    this.calcApr();
                    break;
                case EFF:
                    this.calcApr();
                    break;
                case PRINCIPAL:
                    this.calcPrincipal();
                    break;
                case PAYMENT:
                    this.calcPayment();
                    break;

            }
        }
        /*---------------------------------------------
         * Generate the Monthly Amortization table of 3 columns
         * for the entire repayment schedule over the term of the loan.
         * store these 3 columns of figures in arrays
         * this.mi[]. this.mp[], this.mb[]
         * Nothing is displayed by this method. It just stores calculations in arrays.
         */
        public void generateMonthlyAmortizationTable()
        {
            
  
            int terms = (int)info.Terms;
            if ((info.Terms - terms) > 0.001)
            {
                terms += 1;
            }
            this.mi = new double[terms];
            this.mp = new double[terms];
            this.mb = new double[terms];

            int numYears = (int)((terms - 1) / info.X12 + 1);
            this.tmi = new double[numYears];
            this.tmp = new double[numYears];

            int m_at = terms;     
            double pmt = info.Payment;
            double balance = info.Principal;
            double factor = info.Apr / (info.X12 * 100);

            double totMonI = 0;
            double totMonP = 0;
            int countMonths = 0;               
            double x12 = info.X12;
            int i;
            int countYears = 0;
            int months = terms;             
            double monthlyInterest;
            double monthlyPrincipal;

            int mo = 1000;
            if (m_at < mo) mo = m_at;
            if (months > 1000) months = 1000;
            //fill the arrays money.tmi[] & money.tmp[] with the yearly results
            for (i = 1; i <= months; i++)
            {

                monthlyInterest = balance * factor;
                monthlyPrincipal = pmt - monthlyInterest;

                if (monthlyPrincipal >= balance)
                {
                    i = months;   //flag: end of loop
                    monthlyPrincipal = balance;
                    balance = 0;
                }
                else
                {
                    balance = balance - monthlyPrincipal;
                }


                //collect the data for the month-by-month schedule for the first mo months.
                if (i <= mo)
                {
                    this.mi[i - 1] = monthlyInterest;
                    this.mp[i - 1] = monthlyPrincipal;
                    this.mb[i - 1] = balance;
                }

                //collect totals for each years
                totMonI = totMonI + monthlyInterest;
                totMonP = totMonP + monthlyPrincipal;
                countMonths++;
                if (countMonths == x12)     //if end of that year store totals
                {
                    countYears++;
                    this.tmi[countYears - 1] = totMonI;
                    this.tmp[countYears - 1] = totMonP;
                    countMonths = 0;        //Reset countMonths back to 0 because a full 12 months was stored
                    totMonI = 0;
                    totMonP = 0;
                }

            }//next i

            if (countMonths > 0) //if there was a partial final year
            {
                countYears++;
                this.tmi[countYears - 1] = totMonI;
                this.tmp[countYears - 1] = totMonP;
            }
            this.numYears = countYears;	//# of years

        }
        /*---------------------------------------------
         * For debugging
         */
        public void displayAmortization()
        {
            Console.WriteLine();

            int j = this.mi.Length;
            for (int i = 0; i < j; i +=1)
            {
                Console.WriteLine($"{i + 1}    {this.mi[i]}      {this.mp[i]}       {this.mb[i]}");
            } 
        }

        /*---------------------------------------------
            This is the main method.
            The this.info object has to be filled with appropriate values first.
            this method is called from outside this class.
         */
        public void generateAmortization()
        {
            solveForSwitch();
            generateMonthlyAmortizationTable();
            //displayAmortization();
        }
    }
}
