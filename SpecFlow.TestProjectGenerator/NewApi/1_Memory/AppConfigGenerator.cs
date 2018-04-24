﻿using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class AppConfigGenerator : XmlFileGeneratorBase
    {
        private readonly ProjectFileFactory _projectFileFactory = new ProjectFileFactory();

        public ProjectFile Generate(string unitTestProvider, AppConfigSection[] appConfigSections = null, StepAssembly[] stepAssemblies = null, SpecFlowPlugin[] plugins = null, CultureInfo featureLanguage = null)
        {
            featureLanguage = featureLanguage ?? CultureInfo.GetCultureInfo("en-US");
            appConfigSections = appConfigSections ?? new[] { new AppConfigSection(name: "specFlow", type: "TechTalk.SpecFlow.Configuration.ConfigurationSectionHandler, TechTalk.SpecFlow") };

            using (var ms = new MemoryStream())
            {
                using (var writer = GenerateDefaultXmlWriter(ms))
                {
                    writer.WriteStartElement("configuration");

                    WriteSpecFlow(writer, unitTestProvider, stepAssemblies, plugins, featureLanguage);

                    writer.WriteEndElement();
                    writer.Flush();

                    return _projectFileFactory.FromStream(ms, "app.config", "None", Encoding.UTF8);
                }
            }
        }

        private void WriteSpecFlow(XmlWriter writer, string unitTestProvider, StepAssembly[] stepAssemblies = null, SpecFlowPlugin[] plugins = null, CultureInfo featureLanguage = null)
        {
            writer.WriteStartElement("specFlow");

            WriteUnitTestProvider(writer, unitTestProvider);
            WriteLanguage(writer, featureLanguage);
            WriteStepAssemblies(writer, stepAssemblies);
            WritePlugins(writer, plugins);

            writer.WriteEndElement();
        }

        private void WriteUnitTestProvider(XmlWriter writer, string unitTestProvider)
        {
            writer.WriteStartElement("unitTestProvider");
            writer.WriteAttributeString("name", unitTestProvider);
            writer.WriteEndElement();
        }

        private void WriteLanguage(XmlWriter writer, CultureInfo featureLanguage)
        {
            writer.WriteStartElement("language");
            writer.WriteAttributeString("feature", featureLanguage.Name);
            writer.WriteEndElement();
        }

        private void WriteStepAssemblies(XmlWriter writer, StepAssembly[] stepAssemblies)
        {
            if (stepAssemblies is null) return;
            writer.WriteStartElement("stepAssemblies");
            foreach (var stepAssembly in stepAssemblies)
            {
                WriteStepAssembly(writer, stepAssembly);
            }

            writer.WriteEndElement();
        }

        private void WriteStepAssembly(XmlWriter writer, StepAssembly stepAssembly)
        {
            writer.WriteStartElement("stepAssembly");
            writer.WriteAttributeString("assembly", stepAssembly.Assembly);
            writer.WriteEndElement();
        }

        private void WritePlugins(XmlWriter writer, SpecFlowPlugin[] plugins)
        {
            if (plugins is null) return;
            writer.WriteStartElement("plugins");
            foreach (var plugin in plugins)
            {
                WritePlugin(writer, plugin);
            }

            writer.WriteEndElement();
        }

        private void WritePlugin(XmlWriter writer, SpecFlowPlugin plugin)
        {
            writer.WriteStartElement("add");
            writer.WriteAttributeString("name", plugin.Name);

            if (!string.IsNullOrEmpty(plugin.Path))
                writer.WriteAttributeString("path", plugin.Path);

            if (plugin.Type != (SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime))
                writer.WriteAttributeString("type", plugin.Type.ToPluginTypeString());

            writer.WriteEndElement();
        }
    }
}
