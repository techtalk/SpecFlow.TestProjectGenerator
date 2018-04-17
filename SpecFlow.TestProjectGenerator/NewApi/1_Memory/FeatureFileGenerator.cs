﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class FeatureFileGenerator
    {
        public ProjectFile Generate(string featureFileContent, string featureFileName = null)
        {
            featureFileName = featureFileName ?? $"FeatureFile{Guid.NewGuid():N}.feature";
            return new ProjectFile(featureFileName, "None", featureFileContent);
        }
    }
}
