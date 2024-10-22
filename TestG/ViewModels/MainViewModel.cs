using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using TestG.Consts;
using TestG.Extensions;
using TestG.Models;
using TestG.Validation;

namespace AutoTester;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartTestCommand))]
    List<TestCase> cases = new();
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    [ObservableProperty]
    private int testCaseCount = 1;
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    [ObservableProperty]
    private double leftLimit = 0;
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    [ObservableProperty]
    private double rightLimit = 0;
    [ObservableProperty]
    private int method = 0;
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    [ObservableProperty]
    private string args = "";
    [ObservableProperty]
    private bool isPositive = false;
    [ObservableProperty]
    private double eps = 0.001;
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    [ObservableProperty]
    private string richBoxString = "";

    [ObservableProperty]
    private bool correct = false;



    private bool selectAll = false;

    public bool SelectAll
    {
        get { return selectAll; }
        set
        {
            if (SetProperty(ref selectAll, value))
            {
                SelectOrDeselectAll();
                OnPropertyChanged(nameof(Cases));

            }
        }
    }

    private void SelectOrDeselectAll()
    {
        Cases = Cases.ToList().Select(c =>
        {
            c.Use = SelectAll ? true : false;
            return c;
        }).ToList(); ;

    }

    private bool CanTest()
    {
        bool f5 = TestCaseCount >= 100;
        bool f6 = TestCaseCount < 1;
        if (TestCaseCount >= 100 || TestCaseCount < 1 || LeftLimit > RightLimit)
        {
            return false;
        }
        try
        {
            _ = GetArgs();
        }
        catch
        {
            return false;
        }
        return true;
    }
    [RelayCommand(CanExecute = nameof(CanTest))]
    private void Test(Window window)
    {
        bool isValid = Validator.IsValid(window);
        if (!isValid)
            return;
        var args = GetArgs();
        GenTestCase(args);
    }
    private bool CanStartTest()
    {
        return Cases.Count() != 0;
    }

    [RelayCommand]
    private void Delete()
    {
        var qwe = Cases.ToList();
        var selected = Cases.Where(c => c.Use == true).ToList();
        foreach (var c in selected)
        {
            qwe.Remove(c);
        }
        Cases = qwe;
    }
    [RelayCommand(CanExecute = nameof(CanStartTest))]
    private void StartTest()
    {
        var useCases = Cases.Where(x => x.Use).OrderBy(x => x.IsPositive).ThenBy(x => x.IsPassed).ToList();
        foreach (var useCase in useCases)
        {
            using Process cmd = new Process();
            cmd.StartInfo.FileName = @"Integral3x.exe";
            cmd.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(1251); ;
            cmd.StartInfo.StandardInputEncoding = Encoding.GetEncoding(1251); ;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Arguments = useCase.ArgumentLine.Replace("X = ", "");
            cmd.Start();

            var factMessage = cmd.StandardOutput.ReadLine();
            cmd.StandardInput.Write(' ');
            cmd.WaitForExit();

            var expectedSplit = useCase.Expected!.Split(" = ");
            useCase.Result = $"YF: {factMessage}";
            if (expectedSplit.Length != 1)
            {
                try
                {
                    var res = double.Parse(factMessage!.Split(" = ")[1]);
                    var expectedValue = double.Parse(expectedSplit[1]);
                    var diff = Math.Abs(expectedValue - res);
                    useCase.Diff = $"|SYE - SYF| = {diff} > EPS";
                    useCase.IsPassed = diff < Eps;
                }
                catch
                {
                    useCase.IsPassed = false;
                }
            }
            else
            {
                useCase.IsPassed = useCase.Expected.Replace("YE: ", "") == factMessage;
            }
        }
        useCases = useCases.Where(x => x.Use).OrderBy(x => x.IsPositive).ThenBy(x => x.IsPassed).ToList();
        var stringBuilder = new StringBuilder();
        foreach (var useCase in useCases)
        {
            stringBuilder.Append(useCase.ToString());
        }
        RichBoxString = stringBuilder.ToString();
    }
    private bool CanExport() => !string.IsNullOrEmpty(RichBoxString);
    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Export()
    {
        File.WriteAllText("result.txt", RichBoxString);
        MessageBox.Show("Результат сохранен  'result.txt'", "@", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GenTestCase(double[] args)
    {
        var message = "";
        var errors = false;
        var rnd = new Random();
        for (int i = 0; i < TestCaseCount; i++)
        {
            double step = 0;
            if (IsPositive || !IsPositive && Correct)
            {
                step = rnd.NextDoubleLogarithmic(0.0005, 0.001);
            }
            else
            {
                step = rnd.NextDoubleLogarithmic(-0.01, 0.0000001);
            }
            List<double> paramList = [LeftLimit, RightLimit, step, Method + 1];
            foreach (var arg in args)
            {
                paramList.Add(arg);
            }
            string eps = $"EPS = {Eps}";
            string argLine = $"X = {paramList.GetString()}";
            string expectedMessage = "";
            double expectedValue = 0;
            bool isPos = IsPositive;
            if (leftLimit >= rightLimit)
            {
                expectedMessage = ErrorMessages.LeftLimitError;
                isPos = false;
                errors = true;
            }
            if (step <= 0.000001 || step >= 0.5)
            {
                expectedMessage = ErrorMessages.StepError;


            }
            if (paramList.Count < 5)
            {
                expectedMessage = ErrorMessages.ParamNumberError;
                isPos = false;
                errors = true;


            }
            if (Method > 2 || Method < 0)
            {
                expectedMessage = ErrorMessages.MethodError;
                isPos = false;
                errors = false;

            }
            if (string.IsNullOrWhiteSpace(expectedMessage))
            {
                expectedValue = GetPolynomIntegral(args);
                expectedMessage = $"S = {expectedValue.ToString("e")}";
                isPos = true;
            }
            expectedMessage = $"YE: {expectedMessage}";
            Cases.Add(new TestCase()
            {
                Expected = expectedMessage,
                ArgumentLine = argLine,
                Eps = eps,
                IsPositive = isPos,
                Use = false,
            });
            errors = !isPos;
            message = expectedMessage;
        }

        if (IsPositive && errors)
        {

            MessageBox.Show($"{message}.Тип тестов автоматически изменён на негативный!");
        }
        if (!IsPositive && !errors)
        {
            MessageBox.Show($"Тип тестов автоматически изменён на позитивный!");

        }

        Cases = new(Cases.OrderBy(x => x.IsPositive == false).ToList());
    }

    private double GetPolynomIntegral(double[] args)
    {
        double result = 0;
        for (int i = 0; i < args.Count(); i++)
            result += args[i] * ((Math.Pow(RightLimit, i + 1) / (i + 1)) - (Math.Pow(LeftLimit, i + 1) / (i + 1)));
        return result;
    }

    private double[] GetArgs()
    {
        var strArgs = Args.Trim().Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var values = new List<double>();
        foreach (var arg in strArgs)
        {
            values.Add(double.Parse(arg));
        }
        return values.ToArray();
    }

}
