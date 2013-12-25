using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using UnityEngine;
using NUnit.Framework.Internal.WorkItems;
using NUnit.Framework.Internal.Filters;

namespace NUnit4Unity
{
	public class UnityTestRunner
	{
		private static readonly Assembly[] assembliesToTest;

		public IList<TestSuite> testSuites;
		public IDictionary<string, ITestResult> testResults;
		private TestExecutionContext executionContext;

		static UnityTestRunner()
		{
			assembliesToTest = new Assembly[] { Assembly.GetAssembly(typeof(UnityTestRunner)) };
		}

		public UnityTestRunner()
		{
			testSuites = new List<TestSuite>();
			testResults = new Dictionary<string, ITestResult>();
			executionContext = new TestExecutionContext();
		}

		public IList<TestSuite> FindTests()
		{
			testSuites.Clear();
			ITestAssemblyBuilder testSuiteBuilder = new NUnitLiteTestAssemblyBuilder();
			TestSuite testSuite;
			foreach (Assembly assembly in assembliesToTest)
			{
				testSuite = testSuiteBuilder.Build(assembly, new Hashtable());
				if (testSuite != null)
					testSuites.Add(testSuite);
			}

			return testSuites;
		}

		public void RunAllTests()
		{
			testResults.Clear();
			foreach (TestSuite testSuite in testSuites)
			{
				ITestResult testSuiteResult = RunSuite(testSuite, TestFilter.Empty);
				foreach(ITestResult testFixtureResult in testSuiteResult.Children)
					testResults.Add(testFixtureResult.FullName, testFixtureResult);
			}
		}

		public void RunTestByFullName(string testFixtureName, string testName)
		{
			testResults.Clear();
			foreach (TestSuite testSuite in testSuites)
			{
				ITestResult testSuiteResult = RunSuite(testSuite, new SimpleNameFilter(testName));
				foreach(ITestResult testFixtureResult in testSuiteResult.Children)
					testResults.Add(testFixtureResult.FullName, testFixtureResult);
			}
		}

		private ITestResult RunSuite(TestSuite testSuite, ITestFilter filter)
		{
			WorkItem runner = testSuite.CreateWorkItem(filter);
			runner.Execute(executionContext);
			ITestResult result = runner.Result;

			return result;
		}
	}
}
