﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zvs.DataModel;
using zvs.Fakes;
using zvs.Processor.Fakes;

namespace zvs.Processor.Tests
{
    [TestClass]
    public class PluginManagerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullArg1Test()
        {
            //arrange 
            //act
            new PluginManager(null, new StubIEntityContextConnection(), new StubIFeedback<LogEntry>());
            //assert - throws exception
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullArg2Test()
        {
            //arrange 
            //act
            new PluginManager(new List<ZvsPlugin>(), null, new StubIFeedback<LogEntry>());
            //assert - throws exception
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullArg3Test()
        {
            //arrange 
            //act
            new PluginManager(new List<ZvsPlugin>(), new StubIEntityContextConnection(), null);
            //assert - throws exception
        }

        [TestMethod]
        public async Task LoadPluginsNameTooLongAsyncTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-LoadPluginsNameTooLongAsyncTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var logEntries = new List<LogEntry>();
            var log = new StubIFeedback<LogEntry>
            {
                ReportAsyncT0CancellationToken = (e, c) =>
                {
                    Console.WriteLine(e.ToString());
                    logEntries.Add(e);
                    return Task.FromResult(0);
                }
            };

            var longNamePlugin = new StubZvsPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque magna diam, pellentesque et orci eget, pellentesque iaculis odio. Ut ultrices est sapien, ac pellentesque odio malesuada a. Etiam in neque ullamcorper massa gravida ullamcorper vel a posuere.",
                DescriptionGet = () => "",
                OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0)
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin>() { longNamePlugin }, dbConnection, log);
            //act
            await pluginManager.InitializePluginsAsync(CancellationToken.None);

            //assert 
            Assert.IsTrue(logEntries.Count(o => o.Level == LogEntryLevel.Error) == 1, "Expected 1 error");
        }

         [TestMethod]
        public async Task GetZvsPluginsTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-GetZvsPlugins" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var log = new StubIFeedback<LogEntry>();
            var unitTestingPlugin = new StubUnitTestPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Unit Testing Plugin",
                DescriptionGet = () => "",
                OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0)
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin> { unitTestingPlugin }, dbConnection, log);

            //act
            var result = pluginManager.GetZvsPlugins();

            //assert 
            Assert.IsTrue(result.Count() == 1, "Expected 1 plugin in the list");
        }

        [TestMethod]
        public async Task LoadPluginsAsyncTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-LoadPluginsAsyncTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var logEntries = new List<LogEntry>();
            var log = new StubIFeedback<LogEntry>
            {
                ReportAsyncT0CancellationToken = (e, c) =>
                {
                    Console.WriteLine(e.ToString());
                    logEntries.Add(e);
                    return Task.FromResult(0);
                }
            };

            var unitTestingPlugin = new StubUnitTestPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Unit Testing Plugin",
                DescriptionGet = () => "",
                OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0)
            };

            unitTestingPlugin.OnSettingsCreatingPluginSettingBuilder = async (settingBuilder) =>
            {
                var testSetting = new PluginSetting
                {
                    Name = "Test setting",
                    Value = 360.ToString(CultureInfo.InvariantCulture),
                    ValueType = DataType.INTEGER,
                    Description = "Unit testing only"
                };

                await
                    settingBuilder.Plugin(unitTestingPlugin)
                        .RegisterPluginSettingAsync(testSetting, o => o.PropertyTest);
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin> { unitTestingPlugin }, dbConnection, log);

            //act
            await pluginManager.InitializePluginsAsync(CancellationToken.None);

            //assert 
            Assert.IsTrue(logEntries.Count(o => o.Level == LogEntryLevel.Error) == 0, "Expected 0 errors");
            Assert.IsTrue(unitTestingPlugin.PropertyTest == 360, "Expected TestSetting property to be 360");
        }

        [TestMethod]
        public async Task LoadPluginsAsyncBadPropertyTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-LoadPluginsAsync_BadProperty_Test" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var logEntries = new List<LogEntry>();
            var log = new StubIFeedback<LogEntry>
            {
                ReportAsyncT0CancellationToken = (e, c) =>
                {
                    Console.WriteLine(e.ToString());
                    logEntries.Add(e);
                    return Task.FromResult(0);
                }
            };

            var badSettings = new PluginSetting
            {
                Name = "Test setting2",
                Value = 360.ToString(CultureInfo.InvariantCulture),
                ValueType = DataType.STRING,
                Description = "Unit testing only",
                UniqueIdentifier = "NotExistantPropertyName"

            };

            var plugin = new Plugin()
            {
                PluginGuid = Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
            };
            plugin.Settings.Add(badSettings);
            using (var context = new ZvsContext(dbConnection))
            {
                context.Plugins.Add(plugin);
                await context.SaveChangesAsync(CancellationToken.None);
            }

            var unitTestingPlugin = new StubUnitTestPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Unit Testing Plugin",
                DescriptionGet = () => "",
                OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0)
            };

            unitTestingPlugin.OnSettingsCreatingPluginSettingBuilder = async (settingBuilder) =>
            {
                var testSetting = new PluginSetting
                {
                    Name = "Test setting",
                    Value = 360.ToString(CultureInfo.InvariantCulture),
                    ValueType = DataType.INTEGER,
                    Description = "Unit testing only"
                };

                await
                    settingBuilder.Plugin(unitTestingPlugin)
                        .RegisterPluginSettingAsync(testSetting, o => o.PropertyTest);
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin> { unitTestingPlugin }, dbConnection, log);

            //act
            await pluginManager.InitializePluginsAsync(CancellationToken.None);

            //assert 
            Assert.IsTrue(unitTestingPlugin.PropertyTest == 360, "Expected TestSetting property to be 360");
            Assert.IsTrue(logEntries.Count(o => o.Level == LogEntryLevel.Error && o.Message.Contains("Cannot find property")) == 1, "Expected 1 Cannot find property error");
        }

        [TestMethod]
        public async Task LoadPluginsInvalidCastAsyncTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-LoadPluginsInvalidCastAsyncTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var logEntries = new List<LogEntry>();
            var log = new StubIFeedback<LogEntry>
            {
                ReportAsyncT0CancellationToken = (e, c) =>
                {
                    Console.WriteLine(e.ToString());
                    logEntries.Add(e);
                    return Task.FromResult(0);
                }
            };

            var unitTestingPlugin = new StubUnitTestPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Unit Testing Plugin",
                DescriptionGet = () => "",
                OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0)
            };

            unitTestingPlugin.OnSettingsCreatingPluginSettingBuilder = async (settingBuilder) =>
            {
                var testSetting = new PluginSetting
                {
                    Name = "Test setting",
                    Value = "abc",
                    ValueType = DataType.INTEGER,
                    Description = "Unit testing only"
                };

                await
                    settingBuilder.Plugin(unitTestingPlugin)
                        .RegisterPluginSettingAsync(testSetting, o => o.PropertyTest);
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin> { unitTestingPlugin }, dbConnection, log);

            //act
            await pluginManager.InitializePluginsAsync(CancellationToken.None);

            //assert 
            Assert.IsTrue(logEntries.Count(o => o.Level == LogEntryLevel.Error && o.Message.Contains("Cannot cast value on plugin setting")) == 1, "Expected 1 cannot cast value on plugin setting error");
        }

        [TestMethod]
        public async Task LoadPluginsAsyncAutoStartTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-LoadPluginsAsyncAutoStartTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var log = new StubIFeedback<LogEntry>
            {
                ReportAsyncT0CancellationToken = (e, c) =>
                {
                    Console.WriteLine(e.ToString());
                    return Task.FromResult(0);
                }
            };

            var plugin = new Plugin()
            {
                PluginGuid = Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                IsEnabled = true
            };
            using (var context = new ZvsContext(dbConnection))
            {
                context.Plugins.Add(plugin);
                await context.SaveChangesAsync(CancellationToken.None);
            }
            var isStarted = false;
            var unitTestingPlugin = new StubUnitTestPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Unit Testing Plugin",
                DescriptionGet = () => "",
                 OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0),
                StartAsync01 = () =>
                {
                    isStarted = true;
                    return Task.FromResult(0);
                }
            };
            unitTestingPlugin.OnSettingsCreatingPluginSettingBuilder = async (settingBuilder) =>
            {
                var testSetting = new PluginSetting
                {
                    Name = "Test setting",
                    Value = 360.ToString(CultureInfo.InvariantCulture),
                    ValueType = DataType.INTEGER,
                    Description = "Unit testing only"
                };

                await
                    settingBuilder.Plugin(unitTestingPlugin)
                        .RegisterPluginSettingAsync(testSetting, o => o.PropertyTest);
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin> { unitTestingPlugin }, dbConnection, log);

            //act
            await pluginManager.InitializePluginsAsync(CancellationToken.None);

            //assert 
            Assert.IsTrue(isStarted, "Plugin not started!");
        }

        [TestMethod]
        public async Task EnablePluginAsyncTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-EnablePluginAsyncTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var log = new StubIFeedback<LogEntry>();
            var hasStarted = false;
            var unitTestingPlugin = new StubUnitTestPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Unit Testing Plugin",
                DescriptionGet = () => "",
                OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0),
                StartAsync01 = async () => hasStarted = true
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin> { unitTestingPlugin }, dbConnection, log);

            var plugin = new Plugin
            {
                PluginGuid = Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
            };
            using (var context = new ZvsContext(dbConnection))
            {
                context.Plugins.Add(plugin);
                await context.SaveChangesAsync(CancellationToken.None);
            }

            //act
            var result = await pluginManager.EnablePluginAsync(Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"), CancellationToken.None);

            //assert 
            Assert.IsFalse(result.HasError, result.Message);
            Assert.IsTrue(hasStarted, "Expected plugin startAsync to be called.");
        }

        [TestMethod]
        public async Task EnablePluginAsyncNotFoundTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-EnablePluginAsyncNotFoundTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var log = new StubIFeedback<LogEntry>();
            var pluginManager = new PluginManager(new List<ZvsPlugin>(), dbConnection, log);

            //act
            var result = await pluginManager.EnablePluginAsync(Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"), CancellationToken.None);

            //assert 
            Assert.IsTrue(result.HasError, result.Message);
        }

        [TestMethod]
        public async Task DisablePluginAsyncTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-DisablePluginAsyncTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var log = new StubIFeedback<LogEntry>();
            var hasStopped = false;
            var unitTestingPlugin = new StubUnitTestPlugin
            {
                PluginGuidGet = () => Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
                NameGet = () => "Unit Testing Plugin",
                DescriptionGet = () => "",
                OnSettingsCreatingPluginSettingBuilder = (s) => Task.FromResult(0),
                OnDeviceSettingsCreatingDeviceSettingBuilder = (s) => Task.FromResult(0),
                OnSceneSettingsCreatingSceneSettingBuilder = (s) => Task.FromResult(0),
                StopAsync01 = async () => hasStopped = true
            };

            var pluginManager = new PluginManager(new List<ZvsPlugin> { unitTestingPlugin }, dbConnection, log);

            var plugin = new Plugin
            {
                PluginGuid = Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"),
            };
            using (var context = new ZvsContext(dbConnection))
            {
                context.Plugins.Add(plugin);
                await context.SaveChangesAsync(CancellationToken.None);
            }

            //act
            var result = await pluginManager.DisablePluginAsync(Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"), CancellationToken.None);

            //assert 
            Assert.IsFalse(result.HasError, result.Message);
            Assert.IsTrue(hasStopped, "Expected plugin StopAsync to be called.");
        }

        [TestMethod]
        public async Task DisablePluginAsyncNotFoundTest()
        {
            //Arrange 
            var dbConnection = new StubIEntityContextConnection { NameOrConnectionStringGet = () => "am-DisablePluginAsyncNotFoundTest" };
            Database.SetInitializer(new CreateFreshDbInitializer());

            var log = new StubIFeedback<LogEntry>();
            var pluginManager = new PluginManager(new List<ZvsPlugin> { }, dbConnection, log);

            //act
            var result = await pluginManager.DisablePluginAsync(Guid.Parse("a0f912a6-b8bb-406a-360f-1eb13f50aae4"), CancellationToken.None);

            //assert 
            Assert.IsTrue(result.HasError, result.Message);
        }

        public class StubUnitTestPlugin : StubZvsPlugin
        {
            public int PropertyTest { get; set; }
        }
    }


}
