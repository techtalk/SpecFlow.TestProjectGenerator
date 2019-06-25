﻿using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Io.Cucumber.Messages;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class CucumberMessagesDriver
    {
        private readonly TestProjectFolders _testProjectFolders;

        public CucumberMessagesDriver(TestProjectFolders testProjectFolders)
        {
            _testProjectFolders = testProjectFolders;
        }

        public IMessage UnpackWrapper(Wrapper wrapper)
        {
            switch (wrapper.MessageCase)
            {
                case Wrapper.MessageOneofCase.TestRunStarted: return wrapper.TestRunStarted;
                case Wrapper.MessageOneofCase.TestCaseStarted: return wrapper.TestCaseStarted;
                case Wrapper.MessageOneofCase.TestCaseFinished: return wrapper.TestCaseFinished;
                default: throw new InvalidOperationException($"(Currently) unsupported message type: {wrapper.MessageCase}");
            }
        }

        public IEnumerable<IMessage> LoadMessageQueue()
        {
            string pathToCucumberMessagesFile = Path.Combine(_testProjectFolders.ProjectBinOutputPath, "CucumberMessageQueue", "messages");
            if (!File.Exists(pathToCucumberMessagesFile))
            {
                yield break;
            }

            using (var fileStream = File.Open(pathToCucumberMessagesFile, FileMode.Open, System.IO.FileAccess.Read))
            {
                while (fileStream.CanSeek && fileStream.Position < fileStream.Length)
                {
                    var messageParser = Wrapper.Parser;
                    var message = messageParser.ParseDelimitedFrom(fileStream);
                    yield return UnpackWrapper(message);
                }
            }
        }
    }
}
