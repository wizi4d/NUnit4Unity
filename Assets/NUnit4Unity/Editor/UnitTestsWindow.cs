using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

namespace NUnit4Unity
{
	public class UnitTestsWindow : EditorWindow
	{
		private UnityTestRunner testRunner;
		private bool runTests;
		private string selectedFixtureName;
		private string selectedTestName;
		private ITestResult selectedTestResult;

		private IDictionary<TestFixture, bool> foldoutTestFixtures;
		
		private Texture2D backgroundTexture;
		private Vector2 testAreaScrollPosition = Vector2.zero;
		private Vector2 testMessageScrollPosition = Vector2.zero;

		public UnitTestsWindow()
		{
			testRunner = new UnityTestRunner();
			testRunner.FindTests();

			foldoutTestFixtures = new Dictionary<TestFixture, bool>();

			backgroundTexture = new Texture2D(1, 1);
			backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
		}

		[MenuItem("Window/Unit Tests")]
		public static void InitializeWindow()
		{
			UnitTestsWindow window = (UnitTestsWindow)EditorWindow.GetWindow(typeof(UnitTestsWindow));
			window.title = "Unit Tests";
		}

		private void OnGUI()
		{
			HandleTestMode();

			RenderTopMenuButtons();
			RenderTestArea(testRunner.testSuites);
		}

		private void HandleTestMode()
		{
			if (runTests && EditorApplication.isPlaying)
			{
				if (String.IsNullOrEmpty(selectedTestName))
					testRunner.RunAllTests();
				else
				{
					testRunner.RunTestByFullName(selectedFixtureName, selectedTestName);
					selectedFixtureName = String.Empty;
					selectedTestName = String.Empty;
				}
				SetTestModeState(false);
			}
		}

		private bool IsTestModeActive()
		{
			return runTests && EditorApplication.isPlaying;
		}

		private void RunAllTests()
		{
			selectedTestName = String.Empty;
			SetTestModeState(true);
		}

		private void SetTestModeState(bool active)
		{
			EditorApplication.isPlaying = active;
			runTests = active;
		}

		private void RunSingleTest(string testName)
		{
			selectedTestName = testName;
			SetTestModeState(true);
		}

		private void RenderTopMenuButtons()
		{
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Run all tests", GUILayout.Width(100)))
			{
				RunAllTests();
			}

			/*if (GUILayout.Button("Cancel"))
			{
				
			}*/

			GUILayout.EndHorizontal();

			GUILayout.Space(5);
		}

		private void RenderTestArea(IList<TestSuite> testSuites)
		{
			testAreaScrollPosition = EditorGUILayout.BeginScrollView(testAreaScrollPosition);

			foreach (TestSuite testSuite in testSuites)
				RenderTestSuite(testSuite);

			GUILayout.Space(5);
			EditorGUILayout.EndScrollView();

			testMessageScrollPosition = EditorGUILayout.BeginScrollView(testMessageScrollPosition);
			if (this.selectedTestResult != null && this.selectedTestResult.FailCount > 0)
			{
				GUILayout.Label(selectedTestResult.FullName);
				GUILayout.Label(selectedTestResult.Message);
				GUILayout.Label(selectedTestResult.StackTrace);
			}
			else
			{
				GUILayout.Label(String.Empty);
				GUILayout.Label(String.Empty);
				GUILayout.Label(String.Empty);
			}
			EditorGUILayout.EndScrollView();
		}

		private void RenderTestSuite(TestSuite testSuite)
		{
			foreach (TestFixture testFixture in testSuite.Tests)
				RenderTestFixture(testFixture);
		}

		private void RenderTestFixture(TestFixture testFixture)
		{
			if (!foldoutTestFixtures.ContainsKey(testFixture))
				foldoutTestFixtures.Add(testFixture, true);

			DrawSeparator();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Run", GUILayout.Width(35)))
				RunSingleTest(testFixture.FullName);
			bool expanded = foldoutTestFixtures[testFixture];
			ITestResult testFixtureResult;
			testRunner.testResults.TryGetValue(testFixture.FullName, out testFixtureResult);
			string fixturePresentation = FixturePresentation(testFixture, testFixtureResult);
			expanded = EditorGUILayout.Foldout(expanded, fixturePresentation);
			Rect fildoutRect = GUILayoutUtility.GetLastRect();
			foldoutTestFixtures[testFixture] = expanded;
			GUILayout.EndHorizontal();

			if (testFixtureResult != null)
				HighlightTestResult(testFixtureResult.SkipCount, testFixtureResult.FailCount, fildoutRect.x, 0.4f);

			if (expanded)
				RenderTests(testFixture, testFixtureResult);
		}

		private string FixturePresentation(TestFixture testFixture, ITestResult testFixtureResult)
		{
			string result = "";
			if (testFixtureResult == null)
				result += string.Format("{0} (Test count: {1})", testFixture.Name, testFixture.Tests.Count);
			else
			{
				result += string.Format("{0} (Test count: {1}, Pass: {2}, Skip: {3}, Fail: {4})", testFixtureResult.Name, testFixture.Tests.Count, testFixtureResult.PassCount, testFixtureResult.SkipCount, testFixtureResult.FailCount);
			}

			return result;
		}

		private void RenderTests(TestFixture testFixture, ITestResult testFixtureResult)
		{
			foreach (ITest test in testFixture.Tests)
			{
				ITestResult testMethodResult = FindTestMethodResultByFullName(testFixtureResult, test.FullName);

				GUILayout.BeginHorizontal();

				GUILayout.Space(20);
				if (GUILayout.Button("Run", GUILayout.Width(35)))
					RunSingleTest(test.FullName);
				GUILayout.Label(test.Name);
				Rect testNameLabelRect = GUILayoutUtility.GetLastRect();
				string durationText = "";
				if (testMethodResult != null)
				{
					durationText = DurationPresentation(testMethodResult.Duration);
					if (Event.current.isMouse && Event.current.button == 0 && testNameLabelRect.Contains(Event.current.mousePosition))
					    this.selectedTestResult = testMethodResult;
				}
				GUILayout.Label(durationText, new GUIStyle() { alignment = TextAnchor.MiddleRight }, GUILayout.Width(60));
				GUILayout.EndHorizontal();

				if (testMethodResult != null)
					HighlightTestResult(testMethodResult.SkipCount, testMethodResult.FailCount, testNameLabelRect.x, 0.2f);
			}
		}

		private ITestResult FindTestMethodResultByFullName(ITestResult testFixtureResult, string fullName)
		{
			if (testFixtureResult == null)
				return null;

			foreach (ITestResult methodResult in testFixtureResult.Children)
				if (methodResult.FullName == fullName)
					return methodResult;

			return null;
		}

		private string DurationPresentation(TimeSpan duration)
		{
			string result = "";
			if (duration.Minutes > 0)
				result += string.Format("{0} min ", duration.Minutes);
			if (duration.Seconds > 0)
				result += string.Format("{0} sec ", duration.Seconds);
			result += string.Format("{0} ms", duration.Milliseconds);

			return result;
		}

		private void DrawSeparator()
		{
			GUILayout.Space(5);
			Rect rect = GUILayoutUtility.GetLastRect();
			Color lastColor = GUI.color;
			GUI.color = new Color(0f, 0f, 0f, 1f);
			GUI.DrawTexture(new Rect(0f, rect.yMax, Screen.width, 2f), backgroundTexture);
			GUI.color = lastColor;
			GUILayout.Space(5);
		}

		private void HighlightTestResult(int skipCount, int failCount, float indent, float alfaCoefficient)
		{
			if (failCount > 0)
				HighlightLine(Color.red, indent, alfaCoefficient);
			else if (skipCount > 0)
				HighlightLine(Color.yellow, indent, alfaCoefficient);
			else
				HighlightLine(Color.green, indent, alfaCoefficient);
		}

		private void HighlightLine(Color highlightColour, float indent, float alfaCoefficient)
		{
			Rect rect = GUILayoutUtility.GetLastRect();
			rect.x = indent;
			rect.width = Screen.width;
			highlightColour.a *= alfaCoefficient;
			Color lastColor = GUI.color;
			GUI.color = highlightColour;
			GUI.DrawTexture(rect, backgroundTexture);
			GUI.color = lastColor;
		}
	}
}
