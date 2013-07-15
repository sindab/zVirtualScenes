﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using System.Linq;
using System.Threading;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Windows.Threading;
using zvs.Entities;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace zvs.Processor
{
    public class AdapterManager
    {
        private const int Verbose = 10;
        private const string Name = "Adapter Manager";

        public Core Core { get; private set; }

#pragma warning disable 649
        [ImportMany]
        private IEnumerable<zvsAdapter> _Adapters;
#pragma warning restore 649

        private Dictionary<Guid, zvsAdapter> AdapterLookup = new Dictionary<Guid, zvsAdapter>();

        public async Task LoadPluginsAsync(Core core)
        {
            Core = core;
            SafeDirectoryCatalog catalog = new SafeDirectoryCatalog("adapters");
            CompositionContainer compositionContainer = new CompositionContainer(catalog);
            compositionContainer.ComposeParts(this);

            if (catalog.LoadExceptionTypeNames.Count > 0)
            {
                Core.log.WarnFormat(@"The following adapters could not be loaded because they are incompatible: {0}. To resolve this issue, update or uninstall the listed plug-in's.",
                    string.Join(", ", catalog.LoadExceptionTypeNames));
            }

            using (zvsContext context = new zvsContext())
            {
                // Iterate the adapters found in dlls
                foreach (var adapter in _Adapters)
                {
                    //keeps this adapter in scope 
                    var zvsAdapter = adapter;

                    if (!AdapterLookup.ContainsKey(zvsAdapter.AdapterGuid))
                        AdapterLookup.Add(zvsAdapter.AdapterGuid, zvsAdapter);

                    //Check Database for this adapter
                    var dbAdapter = await context.Adapters.FirstOrDefaultAsync(p => p.AdapterGuid == zvsAdapter.AdapterGuid);
                    if (dbAdapter == null)
                    {
                        dbAdapter = new Adapter();
                        dbAdapter.AdapterGuid = zvsAdapter.AdapterGuid;
                        context.Adapters.Add(dbAdapter);
                    }

                    //Update Name and Description
                    dbAdapter.Name = zvsAdapter.Name;
                    dbAdapter.Description = zvsAdapter.Description;

                    //TODO: DO SOMETHING ELSE HERE
                    var result = await context.TrySaveChangesAsync();
                    if (result.HasError)
                        core.log.Error(result.Message);

                    string msg = string.Format("Initializing '{0}'", zvsAdapter.Name);
                    Core.log.Info(msg);

                    //Plug-in need access to the core in order to use the Logger
                    await zvsAdapter.Initialize(Core);

                    if (dbAdapter.IsEnabled)
                        await zvsAdapter.StartAsync();
                }
            }
        }

        public ReadOnlyDictionary<Guid, zvsAdapter> AdapterGuidToAdapterDictionary
        {
            get { return new ReadOnlyDictionary<Guid, zvsAdapter>(AdapterLookup); }
        }

        public async void EnableAdapterAsync(Guid adapterGuid)
        {
            if (AdapterLookup.ContainsKey(adapterGuid))
            {
                AdapterLookup[adapterGuid].IsEnabled = true;
                await AdapterLookup[adapterGuid].StartAsync();
            }

            //Save Database Value
            using (zvsContext context = new zvsContext())
            {
                var a = await context.Adapters.FirstOrDefaultAsync(o => o.AdapterGuid == adapterGuid);
                if (a != null)
                    a.IsEnabled = true;

                await context.TrySaveChangesAsync();
            }
        }

        public async void DisableAdapterAsync(Guid adapterGuid)
        {
            if (AdapterLookup.ContainsKey(adapterGuid))
            {
                AdapterLookup[adapterGuid].IsEnabled = false;
                await AdapterLookup[adapterGuid].StopAsync();
            }

            //Save Database Value
            using (zvsContext context = new zvsContext())
            {
                var a = await context.Adapters.FirstOrDefaultAsync(o => o.AdapterGuid == adapterGuid);
                if (a != null)
                    a.IsEnabled = false;

                await context.TrySaveChangesAsync();
            }
        }

        public void NotifyAdapterSettingsChanged(AdapterSetting adapterSetting)
        {
            if (AdapterLookup.ContainsKey(adapterSetting.Adapter.AdapterGuid))
            {
                AdapterLookup[adapterSetting.Adapter.AdapterGuid].SettingChangedAsync(adapterSetting.UniqueIdentifier, adapterSetting.Value);
            }
        }
    }
}
