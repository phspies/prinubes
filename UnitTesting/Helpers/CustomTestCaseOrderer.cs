﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace UnitTesting
{
    public class CustomTestCaseOrderer : ITestCaseOrderer
    {
        public const string TypeName = "UnitTesting.CustomTestCaseOrderer";

        public const string AssembyName = "UnitTesting";

        public static readonly ConcurrentDictionary<string, ConcurrentQueue<string>>
            QueuedTests = new ConcurrentDictionary<string, ConcurrentQueue<string>>();

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var test = testCases.OrderBy(GetOrder);
            return testCases.OrderBy(GetOrder);
        }

        private static int GetOrder<TTestCase>(TTestCase testCase) where TTestCase : ITestCase
        {
            // Enqueue the test name.
            QueuedTests.GetOrAdd(
                    testCase.TestMethod.TestClass.Class.Name,
                    key => new ConcurrentQueue<string>())
                .Enqueue(testCase.TestMethod.Method.Name);

            // Order the test based on the attribute.
            var attr = testCase.TestMethod.Method.ToRuntimeMethod().GetCustomAttribute<OrderAttribute>();
            return attr?.I ?? 0;
        }
    }
}
