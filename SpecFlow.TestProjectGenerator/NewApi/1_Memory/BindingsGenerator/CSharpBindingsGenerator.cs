﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi.Driver;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public class CSharpBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
using System;
using System.IO;
using System.Xml;
using TechTalk.SpecFlow;

[Binding]
public class {0}
{{
    {1}
}}";

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            return new ProjectFile($"BindingsClass_{Guid.NewGuid():N}.cs", "Compile", content);
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string randomClassName = $"BindingsClass_{Guid.NewGuid():N}";
            string fileContent = string.Format(BindingsClassTemplate, randomClassName, method);
            return new ProjectFile($"{randomClassName}.cs", "Compile", fileContent);
        }

        protected override string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"object {argumentName}";
                        break;
                    case ParameterType.Table:
                        parameter = $"Table {argumentName}";
                        break;
                    case ParameterType.DocString:
                        parameter = $"string {argumentName}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }
            
            return $@"[{attributeName}(@""{regex}"")] public void {methodName}({parameter}) 
                                {{
                                    {methodImplementation}
                                }}";
        }

        protected override string GetHookBindingClass(
            string hookType,
            string name,
            string code = "",
            int? order = null,
            IList<string> hookTypeAttributeTags = null,
            IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null)
        {
            string ToScopeTags(IList<string> scopeTags) => scopeTags is null || !scopeTags.Any() ? null : $"[{string.Join(", ", scopeTags.Select(t => $@"Scope(Tag=""{t}"")"))}]";

            bool isStatic = IsStaticEvent(hookType);

            string hookTags = hookTypeAttributeTags?.Select(t => $@"""{t}""").JoinToString(", ");

            var hookAttributeConstructorProperties = new[]
            {
                hookTypeAttributeTags is null || !hookTypeAttributeTags.Any() ? null : $"tags: new string[] {{{hookTags}}}",
                order is null ? null : $"Order = {order}"
            }.Where(p => p.IsNotNullOrWhiteSpace());

            string hookTypeAttributeTagsString = string.Join(", ", hookAttributeConstructorProperties);

            string scopeClassAttributes = ToScopeTags(classScopeAttributeTags);
            string scopeMethodAttributes = ToScopeTags(methodScopeAttributeTags);
            string staticKeyword = isStatic ? "static" : string.Empty;

            return $@"
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TechTalk.SpecFlow;

[Binding]
{scopeClassAttributes}
public class {$"HooksClass_{Guid.NewGuid():N}"}
{{
    [{hookType}({hookTypeAttributeTagsString})]
    {scopeMethodAttributes}
    public {staticKeyword} void {name}()
    {{
        Console.WriteLine(""-> hook: {name}"");
        {code}
    }}   
}}
";
        }
    }
}
