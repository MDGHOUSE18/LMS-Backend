using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class EmiCalculatorService
    {
        /// <summary>
        /// Calculate EMI using standard formula: EMI = [P × R × (1+R)ⁿ] / [(1+R)ⁿ – 1]
        /// </summary>
        /// <param name="principal">Loan amount in rupees</param>
        /// <param name="annualInterestRate">Annual interest rate (e.g., 10 for 10%)</param>
        /// <param name="tenureMonths">Tenure in months</param>
        /// <returns>Monthly EMI amount rounded to nearest rupee</returns>
        public decimal CalculateEMI(decimal principal, decimal annualInterestRate, int tenureMonths)
        {
            if (principal <= 0)
                throw new ArgumentException("Principal amount must be greater than zero");
            
            if (annualInterestRate <= 0)
                throw new ArgumentException("Interest rate must be greater than zero");
            
            if (tenureMonths <= 0)
                throw new ArgumentException("Tenure must be greater than zero");

            // R = Monthly interest rate (annual rate / 12 / 100)
            double monthlyRate = (double)annualInterestRate / 12 / 100;
            
            // n = tenure in months
            int n = tenureMonths;
            
            // (1+R)^n
            double compoundFactor = Math.Pow(1 + monthlyRate, n);
            
            // EMI = [P × R × (1+R)ⁿ] / [(1+R)ⁿ – 1]
            double emi = ((double)principal * monthlyRate * compoundFactor) / (compoundFactor - 1);
            
            return (decimal)Math.Round(emi, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Generate complete amortization schedule
        /// </summary>
        public List<AmortizationSchedule> GenerateAmortizationSchedule(
            decimal principal, 
            decimal annualInterestRate, 
            int tenureMonths)
        {
            var schedule = new List<AmortizationSchedule>();
            
            decimal emi = CalculateEMI(principal, annualInterestRate, tenureMonths);
            double monthlyRate = (double)annualInterestRate / 12 / 100;
            
            double remainingPrincipal = (double)principal;
            
            for (int month = 1; month <= tenureMonths; month++)
            {
                // Interest for this month
                double interestPart = remainingPrincipal * monthlyRate;
                
                // Principal part of EMI
                double principalPart = (double)emi - interestPart;
                
                // Handle last month rounding
                if (month == tenureMonths)
                {
                    principalPart = remainingPrincipal;
                    emi = (double)principalPart + interestPart;
                }
                
                remainingPrincipal -= principalPart;
                
                schedule.Add(new AmortizationSchedule
                {
                    Month = month,
                    EMI = (decimal)emi,
                    PrincipalPart = (decimal)Math.Round(principalPart, 2),
                    InterestPart = (decimal)Math.Round(interestPart, 2),
                    RemainingBalance = (decimal)Math.Round(Math.Max(0, remainingPrincipal), 2)
                });
            }
            
            return schedule;
        }

        /// <summary>
        /// Calculate total payable amount and total interest
        /// </summary>
        public (decimal totalPayable, decimal totalInterest) CalculateTotals(
            decimal principal, 
            decimal annualInterestRate, 
            int tenureMonths)
        {
            decimal emi = CalculateEMI(principal, annualInterestRate, tenureMonths);
            decimal totalPayable = emi * tenureMonths;
            decimal totalInterest = totalPayable - principal;
            
            return (totalPayable, totalInterest);
        }
    }

    public class AmortizationSchedule
    {
        public int Month { get; set; }
        public decimal EMI { get; set; }
        public decimal PrincipalPart { get; set; }
        public decimal InterestPart { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
