using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class EligibilityService : IEligibilityService
    {
        private const int MIN_AGE = 21;
        private const int MAX_AGE = 60;
        private const decimal MIN_MONTHLY_SALARY = 25000;
        private const decimal MAX_DEBT_TO_INCOME_RATIO = 0.50m; // 50%
        private const decimal MAX_LOAN_TO_INCOME_RATIO = 20; // 20x monthly income
        
        public async Task<EligibilityResult> EvaluateAsync(Guid loanId, User user, LoanApplication loan, LoanFinancialDetails financialDetails)
        {
            var result = new EligibilityResult
            {
                IsEligible = true,
                Flags = new List<string>()
            };

            // 1. Age Validation (21-60 years)
            if (!user.DateOfBirth.HasValue)
            {
                result.IsEligible = false;
                result.RejectionReason = "Date of birth is required for age verification";
                return result;
            }

            int age = CalculateAge(user.DateOfBirth.Value);
            if (age < MIN_AGE || age > MAX_AGE)
            {
                result.IsEligible = false;
                result.RejectionReason = $"Not eligible due to age. Must be between {MIN_AGE}-{MAX_AGE} years.";
                return result;
            }

            // 2. Minimum Monthly Salary Check (₹25,000)
            if (financialDetails.MonthlyIncome < MIN_MONTHLY_SALARY)
            {
                result.IsEligible = false;
                result.RejectionReason = $"Minimum monthly salary ₹{MIN_MONTHLY_SALARY:N0} required";
                return result;
            }

            // 3. Calculate Proposed EMI
            if (!financialDetails.InterestRate.HasValue)
            {
                result.IsEligible = false;
                result.RejectionReason = "Interest rate is required for EMI calculation";
                return result;
            }

            var emiCalculator = new EmiCalculatorService();
            decimal proposedEMI = emiCalculator.CalculateEMI(
                financialDetails.LoanAmount,
                financialDetails.InterestRate.Value,
                financialDetails.TenureMonths);

            // 4. Debt-to-Income Ratio Check (≤50%)
            decimal totalEMI = (financialDetails.ExistingEMI ?? 0) + proposedEMI;
            decimal debtToIncomeRatio = totalEMI / financialDetails.MonthlyIncome;

            if (debtToIncomeRatio > MAX_DEBT_TO_INCOME_RATIO)
            {
                result.IsEligible = false;
                result.Flags.Add($"Your EMI exceeds {MAX_DEBT_TO_INCOME_RATIO * 100}% of income. Consider reducing loan amount.");
                result.RejectionReason = $"Debt-to-income ratio ({debtToIncomeRatio:P1}) exceeds maximum allowed ({MAX_DEBT_TO_INCOME_RATIO:P1})";
                return result;
            }

            // 5. Loan-to-Income Ratio Check (≤20x monthly income)
            decimal loanToIncomeRatio = financialDetails.LoanAmount / financialDetails.MonthlyIncome;
            
            if (loanToIncomeRatio > MAX_LOAN_TO_INCOME_RATIO)
            {
                result.IsEligible = false;
                result.RejectionReason = $"Loan amount cannot exceed {MAX_LOAN_TO_INCOME_RATIO}x monthly income";
                return result;
            }

            // 6. Store calculated EMI in loan record
            loan.FinancialDetails!.InterestRate = financialDetails.InterestRate;
            
            result.ProposedEMI = proposedEMI;
            result.DebtToIncomeRatio = debtToIncomeRatio;
            result.LoanToIncomeRatio = loanToIncomeRatio;

            return result;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;
            
            // Adjust if birthday hasn't occurred yet this year
            if (dateOfBirth.Date > today.AddYears(-age))
                age--;
            
            return age;
        }
    }

    public class EligibilityResult
    {
        public bool IsEligible { get; set; }
        public string? RejectionReason { get; set; }
        public List<string> Flags { get; set; } = new();
        
        public decimal ProposedEMI { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public decimal LoanToIncomeRatio { get; set; }
    }
}
