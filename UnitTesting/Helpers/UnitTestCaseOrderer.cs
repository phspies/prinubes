using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace UnitTesting
{
    public class UnitTestCaseOrderer : ITestCaseOrderer
    {
        public const string TypeName = "UnitTesting.UnitTestCaseOrderer";

        public const string AssembyName = "UnitTesting";

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            string assemblyName = typeof(TestPriorityAttribute).AssemblyQualifiedName!;
            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();
            List<TTestCase> tests = new List<TTestCase>();
            foreach (TTestCase testCase in testCases)
            {
                int priority = testCase.TestMethod.Method.GetCustomAttributes(assemblyName).FirstOrDefault()?.GetNamedArgument<int>(nameof(TestPriorityAttribute.Priority)) ?? 0;
                GetOrCreate(sortedMethods, priority).Add(testCase);
            }

            foreach (TTestCase testCase in sortedMethods.Keys.SelectMany(priority => sortedMethods[priority].OrderBy(testCase => testCase.TestMethod.Method.Name)))
            {
                tests.Add(testCase);
            }

            return tests;
        }

        private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : struct
            where TValue : new() => dictionary.TryGetValue(key, out TValue? result) ? result : (dictionary[key] = new TValue());
    }
}