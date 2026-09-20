using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        // --- Task 2: Демонстрация удаления скрытой зависимости ---
        DisplayResult("=== Task 2: Hidden Dependency Demonstration ===");
        int mutableThreshold = 50;
        int testScore = 55;
        
        // Показываем, что результат зависит только от явно переданных аргументов (Pure Function)
        DisplayResult($"Score 55 with threshold 50: {Classify(testScore, mutableThreshold)}");
        mutableThreshold = 60; // Изменяем внешний порог
        DisplayResult($"Score 55 with threshold 60: {Classify(testScore, mutableThreshold)}");
        DisplayResult($"Score 55 explicitly checked with threshold 50: {Classify(testScore, 50)}\n");

        // --- Task 4: Наборы данных ---
        string[] defaultInputs = { "85", " 70 ", "49", "100", "0", "abc", "", "72.5", "-1", "101" };

        DisplayResult("Choose Mode:");
        DisplayResult("1 - Process Default Dataset");
        DisplayResult("2 - Interactive Console Mode"); 
        Console.Write("Enter choice (1 or 2): ");
        string choice = Console.ReadLine()?.Trim();

        if (choice == "2")
        {
            List<string> userInputs = new List<string>();
            DisplayResult("\nEnter scores one per line (type 'done' to finish):");
            while (true)
            {
                string input = Console.ReadLine();
                if (input?.Trim().ToLower() == "done") break;
                userInputs.Add(input);
            }
            ProcessDataset(userInputs.ToArray());
        }
        else
        {
            DisplayResult("\n=== Processing Default Dataset ===");
            ProcessDataset(defaultInputs);
        }
    }

    // --- Task 4: Обработка массива и вывод результатов ---
    static void ProcessDataset(string[] inputs)
    {
        int validCount = 0;
        int rejectedCount = 0;
        int passedCount = 0;
        List<int> validScores = new List<int>();

        DisplayResult("\n--- Individual Results ---");
        foreach (string input in inputs)
        {
            var (isValid, score, errorMessage) = ValidateScore(input);

            if (!isValid)
            {
                rejectedCount++;
                DisplayResult($"{input}: {errorMessage}");
            }
            else
            {
                validCount++;
                int finalScore = AddBonus(score); // Применяем бонус если > 80
                
                if (finalScore >= 50) passedCount++;
                validScores.Add(finalScore);

                string classification = Classify(finalScore, 50);
                decimal fraction = ToFraction(finalScore);
                DisplayResult($"{finalScore}: {classification} (Fraction: {fraction})");
            }
        }

        // Вычисляем среднее с помощью функции Task 3
        decimal average = CalculateAverage(validScores);

        DisplayResult("\n--- Summary ---");
        DisplayResult($"Valid entries: {validCount}");
        DisplayResult($"Rejected entries: {rejectedCount}");
        DisplayResult($"Passed entries: {passedCount}");
        DisplayResult($"Average of valid scores: {average:F2}");
    }

    // --- Task 1: Чистая валидация (без вывода в консоль) ---
    static (bool IsValid, int Score, string Error) ValidateScore(string text)
    {
        if (!int.TryParse(text?.Trim(), out int score)) 
            return (false, 0, "Invalid integer");
            
        if (score < 0 || score > 100) 
            return (false, 0, "Out of range");

        return (true, score, string.Empty);
    }

    // --- Task 2: Чистая классификация со switch ---
    static string Classify(int score, int passThreshold)
    {
        return score switch
        {
            >= 90 => "Excellent",
            >= 70 => "Good",
            _ when score >= passThreshold => "Satisfactory",
            _ => "Fail"
        };
    }

    // --- Task 3: Конвертация и математические функции ---
    static decimal ToDecimal(int score) => (decimal)score;

    static decimal ToFraction(int score) => ToDecimal(score) / 100m;

    static decimal CalculateAverage(IEnumerable<int> validScores)
    {
        if (validScores == null || !validScores.Any()) return 0m;
        return (decimal)validScores.Sum() / validScores.Count();
    }

    // --- Task 3 & 4: Разделение вывода от расчетов ---
    static void DisplayResult(string message)
    {
        Console.WriteLine(message);
    }

    // --- Task 4: Бонус за оценку выше 80 ---
    static int AddBonus(int score) => score > 80 ? score + 2 : score;
} 

