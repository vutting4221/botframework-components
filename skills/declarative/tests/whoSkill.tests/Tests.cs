// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1118 // Parameter should not span multiple lines

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WhoSkill.Tests
{
    [TestClass]
    public class Tests
    {
        private static string WhoBotFolder = Path.Combine(TestUtils.GetProjectPath(), @"..\..\whoskill");
        private static string TestScriptFolder = Path.Combine(TestUtils.GetProjectPath(), @"TestScripts");
        private static string HttpFolder = Path.Combine(TestUtils.GetProjectPath(), @"cachedHttp");

        private static string Appsettings = Path.Join(WhoBotFolder, "settings", "appsettings.json");
        private static string Settings;

        private static string LuisKey = "00000000-0000-0000-0000-000000000000";
        private static string Endpoint = "https://westus.api.cognitive.microsoft.com";

        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var di = new DirectoryInfo(Path.Join(WhoBotFolder, "generated"));
            foreach (var file in di.GetFiles($"luis.settings.*", SearchOption.AllDirectories))
            {
                Settings = file.FullName;
                break;
            }

            ResourceExplorer = new ResourceExplorer()
                .AddFolder(WhoBotFolder, monitorChanges: false)
                .AddFolder(TestScriptFolder, monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.DeclarativeType, typeof(MockLuisRecognizer), new MockLuisLoader())
                .RegisterType(HttpRequest.DeclarativeType, typeof(MockHttpRequest), new MockHttpRequestLoader());
        }

        [TestMethod]
        public async Task Greeting()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(Appsettings, optional: false)
                .AddJsonFile(Settings, optional: false)

                // Should we use .UseMockLuisSettings() after get all luisCache?
                .AddInMemoryCollection(new Dictionary<string, string>
                       {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                       })
                .UseMockHttpRequestSettings(HttpFolder)
                .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Help()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Whois()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Whois_MultiMatches()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task JobTitle()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Department()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Location()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task PhoneNumber()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task EmailFind()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Manager()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task DirectReports()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Peers()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task EmailAbout()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task MeetAbout()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Me()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task MyManager()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(Appsettings, optional: false)
              .AddJsonFile(Settings, optional: false)
              .AddInMemoryCollection(new Dictionary<string, string>
                     {
                           { "luis:endpoint", Endpoint },
                           { "luis:resources",  TestUtils.GetProjectPath()},
                           { "luis:endpointKey", LuisKey }
                     })
              .UseMockHttpRequestSettings(HttpFolder)
              .Build();
            HostContext.Current.Set<IConfiguration>(config);
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
