﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using zvs.Processor.Triggers;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using zvs.Context;
using zvs.Entities;

namespace zvs.Processor
{
    public class Core
    {
        public PluginManager pluginManager;
        private TriggerManager triggerManager;
        private ScheduledTaskManager scheduledTaskManager;
        public Logging.ILog log;
        public Dispatcher Dispatcher;



        public Core(Dispatcher Dispatcher)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Utils.AppDataPath);
            this.Dispatcher = Dispatcher;

            //Create a instance of the logger
            log = Logging.LogManager.GetLogger<Core>();

            log.InfoFormat("Starting Core Processor:{0}", Utils.ApplicationName);

            #region Install Base Commands and Properties
            using (zvsContext context = new zvsContext())
            {
                BuiltinCommand.AddOrEdit(new BuiltinCommand
                {
                    UniqueIdentifier = "REPOLL_ME",
                    Name = "Re-poll Device",
                    ArgumentType = DataType.INTEGER,
                    Description = "This will force a re-poll on an object."
                }, context);

                BuiltinCommand.AddOrEdit(new BuiltinCommand
                {
                    UniqueIdentifier = "REPOLL_ALL",
                    Name = "Re-poll all Devices",
                    ArgumentType = DataType.NONE,
                    Description = "This will force a re-poll on all objects."
                }, context);

                BuiltinCommand.AddOrEdit(new BuiltinCommand
                {
                    UniqueIdentifier = "GROUP_ON",
                    Name = "Turn Group On",
                    ArgumentType = DataType.STRING,
                    Description = "Activates a group."
                }, context);

                BuiltinCommand.AddOrEdit(new BuiltinCommand
                {
                    UniqueIdentifier = "GROUP_OFF",
                    Name = "Turn Group Off",
                    ArgumentType = DataType.STRING,
                    Description = "Deactivates a group."
                }, context);

                BuiltinCommand.AddOrEdit(new BuiltinCommand
                {
                    UniqueIdentifier = "TIMEDELAY",
                    Name = "Scene Time Delay (sec)",
                    ArgumentType = DataType.INTEGER,
                    Description = "Pauses a scene execution for x seconds."
                }, context);

                BuiltinCommand.AddOrEdit(new BuiltinCommand
                {
                    UniqueIdentifier = "RUN_SCENE",
                    Name = "Run Scene",
                    ArgumentType = DataType.INTEGER,
                    Description = "Argument = SceneId"
                }, context);

                DeviceProperty.AddOrEdit(new DeviceProperty
                {
                    UniqueIdentifier = "ENABLEPOLLING",
                    Name = "Enable polling for this device.",
                    Value = "false", //default value
                    ValueType = DataType.BOOL,
                    Description = "Toggles automatic polling for a device."
                }, context);
            }
            #endregion

            BackgroundWorker PluginBW = new BackgroundWorker();
            PluginBW.DoWork += (sender, args) =>
            {
                pluginManager = new PluginManager(this);
            };

            PluginBW.RunWorkerCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        if (args.Error is System.Reflection.ReflectionTypeLoadException)
                        {
                            System.Reflection.ReflectionTypeLoadException reflectionEx = (System.Reflection.ReflectionTypeLoadException)args.Error;
                            string error = "Cannot load one or more plug-ins.";
                            Exception ex = reflectionEx.LoaderExceptions.FirstOrDefault();
                            if (ex != null)
                            {
                                string errorMsg = !string.IsNullOrEmpty(ex.StackTrace)? ex.StackTrace.ToString() : string.Empty;
                                if (string.IsNullOrEmpty(errorMsg) && !string.IsNullOrEmpty(ex.Message))
                                    errorMsg = ex.Message;

                                error = errorMsg +
                                    Environment.NewLine +
                                    Environment.NewLine +
                                    string.Format("This plug-in might not be compatible with {0}. Try removing the plug-in and re-launching the application. ", Utils.ApplicationNameAndVersion);
                            }

                            Core.ProgramHasToClosePrompt(error);
                        }
                        else
                        {
                            Core.ProgramHasToClosePrompt(args.Error.Message);
                        }
                    }
                };
            PluginBW.RunWorkerAsync();


            triggerManager = new TriggerManager(this);
            scheduledTaskManager = new ScheduledTaskManager(this);

            //Install Program Options
            using (zvsContext context = new zvsContext())
            {
                if (ProgramOption.GetProgramOption(context, "LOGDIRECTION") == null)
                {
                    ProgramOption.AddOrEdit(context, new ProgramOption()
                    {
                        UniqueIdentifier = "LOGDIRECTION",
                        Value = "Descending"
                    });
                }
            }
        }


        public static void ProgramHasToClosePrompt(string reason)
        {
            Window WpfBugWindow = new Window()
            {
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                WindowStyle = WindowStyle.None,
                Top = 0,
                Left = 0,
                Width = 1,
                Height = 1,
                ShowInTaskbar = false
            };
            WpfBugWindow.Show();
            if (MessageBox.Show(reason, Utils.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
            {
                WpfBugWindow.Close();
                Environment.Exit(1);
            }
        }
    }
}
