
using System;  
using System.Collections.Generic;
using System.Linq;

namespace Assignment2_FP
{
    // ==========================================
    // DATA STRUCTURES
    // ==========================================
    public record OrderLine(string ItemName, decimal Price, int Quantity);

    // ==========================================
    // TASK 2: CUSTOM DELEGATE & PRICING MODULE
    // ==========================================
    // Custom delegate type using 'delegate' keyword as required by Task 2
    public delegate decimal DiscountRule(decimal subtotal);

    public static class Pricing
    {
        // Line total: Unit price multiplied by quantity
        public static decimal LineTotal(decimal price, int quantity)
            => price * quantity;

        // Collection subtotal (Preserves original collection without mutation)
        public static decimal Subtotal(IEnumerable<OrderLine> lines)
        {
            if (lines == null || !lines.Any()) return 0m;
            return lines.Sum(line => LineTotal(line.Price, line.Quantity));
        }

        // Standard Discount Rule (Named method implementation for delegate)
        public static decimal StandardDiscountRule(decimal subtotal)
        {
            if (subtotal >= 200m) return 0.10m;
            if (subtotal >= 100m) return 0.05m;
            return 0m;
        }

        // Alternative Rule: No discount lambda
        public static readonly DiscountRule NoDiscountRule = subtotal => 0m;

        // Discount Amount
        public static decimal DiscountAmount(decimal subtotal, DiscountRule discountRule)
        {
            if (discountRule == null) return 0m;
            return subtotal * discountRule(subtotal);
        }

        // Discounted Subtotal
        public static decimal DiscountedSubtotal(decimal subtotal, DiscountRule discountRule)
            => subtotal - DiscountAmount(subtotal, discountRule);

        // Shipping calculation
        public static decimal Shipping(decimal discountedSubtotal, bool isEmpty)
        {
            if (isEmpty) return 0m;
            return discountedSubtotal >= 100m ? 0m : 15m;
        }

        // Total Payable amount calculation
        public static decimal Payable(IEnumerable<OrderLine> lines, DiscountRule discountRule)
        {
            bool isEmpty = lines == null || !lines.Any();
            if (isEmpty) return 0m;

            decimal sub = Subtotal(lines);
            decimal discSub = DiscountedSubtotal(sub, discountRule);
            decimal ship = Shipping(discSub, isEmpty);

            return discSub + ship;
        }

        // Display Helper using Action delegate
        public static void DisplayOrderSummary(string title, IEnumerable<OrderLine> lines, DiscountRule rule, Action<string> printer)
        {
            bool isEmpty = lines == null || !lines.Any();
            decimal sub = Subtotal(lines);
            decimal discAmount = DiscountAmount(sub, rule);
            decimal discSub = DiscountedSubtotal(sub, rule);
            decimal ship = Shipping(discSub, isEmpty);
            decimal payable = Payable(lines, rule);

            printer($"--- {title} ---");
            printer($"Subtotal: {sub:F2}");
            printer($"Discount Amount: {discAmount:F2}");
            printer($"Discounted Subtotal: {discSub:F2}");
            printer($"Shipping: {ship:F2}");
            printer($"Payable Amount: {payable:F2}\n");
        }
    }

    // ==========================================
    // STARTER CODE WITH INTENTIONAL BUGS (TASK 1)
    // ==========================================
    public static class StarterPricing
    {
        public static decimal LineTotal(decimal price, int quantity)
            => price + quantity; // BUG 1: Addition instead of multiplication

        public static decimal DiscountRate(decimal subtotal)
        {
            if (subtotal >= 100m) return 0.05m; // BUG 2: Unreachable 10% condition below
            if (subtotal >= 200m) return 0.10m;
            return 0m;  
        }

        public static decimal Shipping(decimal discounted, bool isEmpty)
            => discounted > 100m ? 0m : 15m; // BUG 3: Strict '>' excludes boundary 100m & ignores isEmpty

        public static decimal Payable(decimal subtotal, bool isEmpty)
            => subtotal - DiscountRate(subtotal) // BUG 4: Subtracting rate (e.g. 0.05) instead of amount (subtotal * rate)
               + Shipping(subtotal, isEmpty);
    }

    // ==========================================
    // TASK 3: AUTOMATED TEST RUNNER
    // ==========================================
    public static class TestRunner
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static void AssertEqual<T>(T expected, T actual, string testName)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                _passed++;
                Console.WriteLine($"[PASS] {testName}");
            }
            else
            {
                _failed++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName} | Expected: {expected}, Actual: {actual}");
                Console.ResetColor();
            }
        }

        public static void RunAllTests()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("RUNNING AUTOMATED TEST SUITE (TASK 3)");
            Console.WriteLine("========================================\n");

            _passed = 0;
            _failed = 0;

            // 1. Line Total Tests (Regression Bug 1)
            AssertEqual(120m, Pricing.LineTotal(40m, 3), "Test 1: LineTotal multiplication (40 * 3)");
            AssertEqual(0m, Pricing.LineTotal(0m, 5), "Test 2: LineTotal zero price");

            // 2. Collection Subtotal & Input Preservation
            var lines = new List<OrderLine> { new("Item1", 40m, 2), new("Item2", 30m, 1) };
            AssertEqual(110m, Pricing.Subtotal(lines), "Test 3: Collection Subtotal (40*2 + 30*1)");
            AssertEqual(2, lines.Count, "Test 4: Input preservation (Collection not mutated)");

            // 3. Discount Thresholds (Regression Bug 2)
            AssertEqual(0m, Pricing.StandardDiscountRule(99.99m), "Test 5: Discount below 100 (0%)");
            AssertEqual(0.05m, Pricing.StandardDiscountRule(100m), "Test 6: Discount exactly 100 (5%)");
            AssertEqual(0.05m, Pricing.StandardDiscountRule(199.99m), "Test 7: Discount below 200 (5%)");
            AssertEqual(0.10m, Pricing.StandardDiscountRule(200m), "Test 8: Discount exactly 200 (10%)");

            // 4. Shipping Thresholds & Empty Orders (Regression Bug 3)
            AssertEqual(0m, Pricing.Shipping(100m, false), "Test 9: Shipping boundary exactly 100m (Free)");
            AssertEqual(15m, Pricing.Shipping(99.99m, false), "Test 10: Shipping under 100m ($15)");
            AssertEqual(0m, Pricing.Shipping(50m, true), "Test 11: Shipping empty order (Free)");

            // 5. Custom Delegate Rules & Payable Calculation (Regression Bug 4)
            AssertEqual(104.50m, Pricing.Payable(lines, Pricing.StandardDiscountRule), "Test 12: Payable with Standard Rule");
            AssertEqual(110.00m, Pricing.Payable(lines, Pricing.NoDiscountRule), "Test 13: Payable with No-Discount Rule");

            // 6. Consistency Check
            AssertEqual(Pricing.Payable(lines, Pricing.StandardDiscountRule), Pricing.Payable(lines, Pricing.StandardDiscountRule), "Test 14: Repeated calls produce identical results");

            Console.WriteLine($"\nTest Results: {_passed} Passed, {_failed} Failed.\n");
        }
    }

    // ==========================================
    // MAIN ENTRY POINT (TASK 4)
    // ==========================================
    internal class Program
    {
        static void Main(string[] args)
{
    
    TestRunner.RunAllTests(); 
            Console.WriteLine("========================================");
            Console.WriteLine("DEMONSTRATION (TASK 4)");
            Console.WriteLine("========================================\n");

            // Example 1: Required Assignment Example
            var example1Lines = new List<OrderLine>
            {
                new("Item 1", 40m, 2),
                new("Item 2", 30m, 1)
            };

            Pricing.DisplayOrderSummary("Example 1: Standard Rule (Subtotal 110)", example1Lines, Pricing.StandardDiscountRule, Console.WriteLine);
            Pricing.DisplayOrderSummary("Example 1: No Discount Rule (Subtotal 110)", example1Lines, Pricing.NoDiscountRule, Console.WriteLine);

            // Example 2: Boundary/Branch Example (Subtotal under 100, subject to shipping fee)
            var example2Lines = new List<OrderLine>
            {
                new("Low Value Item", 25m, 2) // Subtotal = 50
            };

            Pricing.DisplayOrderSummary("Example 2: Standard Rule (Subtotal 50 - Under Free Shipping Threshold)", example2Lines, Pricing.StandardDiscountRule, Console.WriteLine);
        }
    }
} 