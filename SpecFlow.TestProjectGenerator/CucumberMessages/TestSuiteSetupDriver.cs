using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages.InlineObjects;
using TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages.RowObjects;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class TestSuiteSetupDriver
    {
        private readonly ProjectsDriver _projectsDriver;
        private readonly TestSuiteInitializationDriver _testSuiteInitializationDriver;
        private readonly JsonConfigurationLoaderDriver _jsonConfigurationLoaderDriver;
        private readonly ConfigurationDriver _configurationDriver;
        private bool _isProjectCreated;

        public TestSuiteSetupDriver(ProjectsDriver projectsDriver, TestSuiteInitializationDriver testSuiteInitializationDriver, JsonConfigurationLoaderDriver jsonConfigurationLoaderDriver,
            ConfigurationDriver configurationDriver)
        {
            _projectsDriver = projectsDriver;
            _testSuiteInitializationDriver = testSuiteInitializationDriver;
            _jsonConfigurationLoaderDriver = jsonConfigurationLoaderDriver;
            _configurationDriver = configurationDriver;
        }

        public void AddGenericWhenStepBinding()
        {
            _projectsDriver.AddStepBinding("When", ".*", "//pass", "'pass");
        }

        public void AddFeatureFiles(int count)
        {
            if (count <= 0 && !_isProjectCreated)
            {
                _projectsDriver.CreateProject("C#");
                _isProjectCreated = true;
                return;
            }

            for (int n = 0; n < count; n++)
            {
                string featureTitle = $"Feature{n}";
                var featureBuilder = new StringBuilder();
                featureBuilder.AppendLine($"Feature: {featureTitle}");

                foreach (string scenario in Enumerable.Range(0, 1).Select(i => $"Scenario: passing scenario nr {i}\r\nWhen the step pass in {featureTitle}"))
                {
                    featureBuilder.AppendLine(scenario);
                    featureBuilder.AppendLine();
                }

                _projectsDriver.AddFeatureFile(featureBuilder.ToString());
                AddGenericWhenStepBinding();
            }

            _isProjectCreated = true;
        }

        public void AddScenarios(int scenariosCount)
        {
            if (scenariosCount <= 0 && !_isProjectCreated)
            {
                _projectsDriver.CreateProject("C#");
                _isProjectCreated = true;
                return;
            }

            const string featureTitle = "Feature1";
            var featureBuilder = new StringBuilder();
            featureBuilder.AppendLine($"Feature: {featureTitle}");

            foreach (string scenario in Enumerable.Range(0, scenariosCount).Select(i => $"Scenario: passing scenario nr {i}\r\nWhen the step pass in {featureTitle}"))
            {
                featureBuilder.AppendLine(scenario);
                featureBuilder.AppendLine();
            }

            _projectsDriver.AddFeatureFile(featureBuilder.ToString());

            _isProjectCreated = true;
        }

        public void AddScenario(Guid pickleId)
        {
            AddScenarios(1);
            _testSuiteInitializationDriver.OverrideTestCaseStartedPickleId = pickleId;
            _testSuiteInitializationDriver.OverrideTestCaseFinishedPickleId = pickleId;
        }

        public void AddDuplicateStepDefinition(string scenarioBlock, string stepRegex)
        {
            _projectsDriver.AddStepBinding(scenarioBlock, stepRegex, "//pass", "'pass");
            _projectsDriver.AddStepBinding(scenarioBlock, stepRegex, "//pass", "'pass");
        }

        public void AddNotMatchingStepDefinition()
        {
            _projectsDriver.AddStepBinding("When", "the step does not pass in .*", "//pass", "'pass");
        }

        public void EnsureAProjectIsCreated()
        {
            if (_isProjectCreated)
            {
                return;
            }

            AddFeatureFiles(1);
        }

        public void AddSpecFlowJsonFromString(string specFlowJson)
        {
            EnsureAProjectIsCreated();
            _jsonConfigurationLoaderDriver.AddSpecFlowJson(specFlowJson);
        }

        public void AddScenarioWithGivenStep(string step, string tags = "")
        {
            if (!_isProjectCreated)
            {
                _projectsDriver.CreateProject("C#");
                _isProjectCreated = true;
            }

            const string featureTitle = "Feature1";
            var featureBuilder = new StringBuilder();
            featureBuilder.AppendLine($"Feature: {featureTitle}");

            featureBuilder.AppendLine(tags);
            featureBuilder.AppendLine("Scenario: scenario");
            featureBuilder.AppendLine($"Given {step}");
            featureBuilder.AppendLine();

            _projectsDriver.AddFeatureFile(featureBuilder.ToString());

            _isProjectCreated = true;
        }

        public void AddStepDefinitionsFromStringList(string stepDefinitionOrder)
        {
            if (!_isProjectCreated)
            {
                _projectsDriver.CreateProject("C#");
                _isProjectCreated = true;
            }

            var order = ParseOrderFromString(stepDefinitionOrder);
            foreach (var stepDefinitionRow in order.StepDefinitionRows)
            {
                (string csharp, string vbnet) = GetStepDefinitionCodeForExecution(stepDefinitionRow.Execution);
                _projectsDriver.AddStepBinding("Given", stepDefinitionRow.Name, csharp, vbnet);
            }
        }

        public StepDefinitionOrder ParseOrderFromString(string stepDefinitionOrder)
        {
            var regex = new Regex(@"(?<BindingName>[A-Za-z0-9_]+) \((?<Result>pass|fail|pending)\)");
            var matches = regex.Matches(stepDefinitionOrder).Cast<Match>();
            var stepDefinitionRows = from m in matches
                                     let bindingName = m.Groups["BindingName"].Value
                                     let name = bindingName.EndsWith("Binding") ? bindingName.Substring(0, bindingName.Length - "Binding".Length) : bindingName
                                     let resultString = m.Groups["Result"].Value
                                     let result = (StepDefinitionRowExecution)Enum.Parse(typeof(StepDefinitionRowExecution), resultString, true)
                                     select new StepDefinitionRow { Name = name, Execution = result };
            return new StepDefinitionOrder
            {
                StepDefinitionRows = stepDefinitionRows.ToList()
            };
        }

        public (string csharp, string vbnet) GetStepDefinitionCodeForExecution(StepDefinitionRowExecution execution)
        {
            switch (execution)
            {
                case StepDefinitionRowExecution.Pass: return ("//pass", "'pass");
                case StepDefinitionRowExecution.Fail: return (@"throw new global::System.Exception(""Expected failure"");", @"Throw New System.Exception(""Expected failure"")");
                case StepDefinitionRowExecution.Pending: return (@"_scenarioContext.Pending();", @"_scenarioContext.Pending()");
                default: throw new NotSupportedException($"Not supported {nameof(StepDefinitionRowExecution)}: {execution}");
            }
        }

        public void AddScenarios(CreateScenarioWithResultRow createScenarioWithResultRows)
        {
            if (createScenarioWithResultRows.Successful >= 0)
            {
                _projectsDriver.AddStepBinding("Given", "a successful step", "", "");

                for (int i = 0; i < createScenarioWithResultRows.Successful; i++)
                {
                    _projectsDriver.AddFeatureFile(
                        $@"Feature: Feature {Guid.NewGuid()}
Scenario: Scenario{i}
Given a successful step
                ");
                }
            }

            if (createScenarioWithResultRows.Ambiguous >= 1)
            {
                AddDuplicateStepDefinition("Given", "an ambiguous step");

                for (int i = 0; i < createScenarioWithResultRows.Ambiguous; i++)
                {
                    _projectsDriver.AddFeatureFile(
                        $@"Feature: Feature {Guid.NewGuid()}
Scenario: Scenario{i}
Given an ambiguous step
                ");
                }
            }

            if (createScenarioWithResultRows.Failing >= 1)
            {
                _projectsDriver.AddFailingStepBinding("Given", "a failing step");

                for (int i = 0; i < createScenarioWithResultRows.Failing; i++)
                {
                    _projectsDriver.AddFeatureFile(
                        $@"Feature: Feature {Guid.NewGuid()}
Scenario: Scenario{i}
Given a failing step
                ");
                }
            }

            _isProjectCreated = true;
        }

        public void AddAppConfigFromString(string appConfigContent)
        {
            _configurationDriver.SetConfigurationFormat(ConfigurationFormat.None);
            _projectsDriver.AddFile("app.config", appConfigContent);
        }
    }
}
