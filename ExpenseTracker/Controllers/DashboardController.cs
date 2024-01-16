using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;
using System.Globalization;
using ExpenseTracker.Models.ViewModels;
using ExpenseTracker.Models.Chart;

namespace ExpenseTracker.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly ApplicationDBContext _context;

        public DashBoardController(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index()
        {
            //Last 7 days
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransaction = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            TotalCalculateAndPassToView(SelectedTransaction);
            DoughnutChartCalculateAndPassToView(SelectedTransaction);
            SplineChartCalculateAndPassToView(SelectedTransaction, StartDate);

            //recent transaction
            ViewBag.RecentTransaction = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(date => date.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public void TotalCalculateAndPassToView(List<Transaction> selectedTransaction)
        {
            //Total Income
            int TotalIncome = selectedTransaction
                .Where(transaction => transaction.Category.Type == "Income")
                .Sum(income => income.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("c0");
            //Total Expense
            int TotalExpense = selectedTransaction
                .Where(transaction => transaction.Category.Type == "Expense")
                .Sum(expnse => expnse.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("c0");
            //Balance
            var Balance = TotalIncome - TotalExpense;

            //Modify to have minus
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culture, "{0:c0}", Balance);
        }

        public void DoughnutChartCalculateAndPassToView(List<Transaction> selectedTransaction)
        {
            //Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = selectedTransaction
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(x => new DoughnutChart /*foreach to get instance*/
                {
                    CategoryName = x.First().Category.Title,
                    Text = x.Sum(x => x.Amount).ToString("C0"),
                    Data = x.Sum(x => x.Amount)
                })
                .OrderByDescending(dgnut => dgnut.Data)
                .ToList();
        }
        public void SplineChartCalculateAndPassToView(List<Transaction> selectedTransaction, DateTime startDate)
        {

            //Spline chart - Income vs Expense
            //Income
            List<SplineChart> IncomeSummary = selectedTransaction
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChart()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                }).ToList();

            //Income
            List<SplineChart> ExpenseSummary = selectedTransaction
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChart()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                }).ToList();

            //Combine Income & Expense
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(num => startDate.AddDays(num).ToString("dd-MMM"))
                .ToArray();
            ViewBag.SplineChartData = from day in Last7Days //declare range variable 'day' to iterate over Last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined //The results are stored in a new collection named dayIncomeJoined.
                                      from income in dayIncomeJoined.DefaultIfEmpty() //left outer join - DefaultIfEmpty() -> include all items  from left side even the right side is nothing
                                                                                      //-> câu lệnh cuối này có tác dụng bổ nghĩa cho income trong bảng IncomeSummary

                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()

                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense
                                      };
        }
    }
}
