﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;

namespace BuildOnSave
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	// why is this needed?
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuidString)]

	// This package needs to be loaded _before_ the user interacts with its UI.
	[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
	public sealed class BuildOnSavePackage : Package
	{
		const string PackageGuidString = "ce5fb4cb-f9c4-469e-ac59-647eb754148c";
		BuildOnSave _buildOnSave_;

		const string SettingsVersionCode = "1";
		const string KeySolutionSettings = "BuildOnSave_" + SettingsVersionCode;

		public BuildOnSavePackage()
		{
			AddOptionKey(KeySolutionSettings);
		}

		protected override void OnLoadOptions(string key, Stream stream)
		{
			if (key != KeySolutionSettings)
				return;
			if (_buildOnSave_ == null)
			{
				Log.E("can not handle OnLoadOptions, we are not initialized yet");
				return;
			}

			try
			{
				var serialized = streamToString(stream);
				Log.D("deserializing and applying solution options {options}", serialized);
				var options = JsonConvert.DeserializeObject<SolutionOptions>(serialized);
				_buildOnSave_.SolutionOptions = options;
			}
			catch (Exception e)
			{
				Log.E("failed to deserialize options: {exception}", e);
			}
		}

		protected override void OnSaveOptions(string key, Stream stream)
		{
			if (key != KeySolutionSettings)
				return;
			if (_buildOnSave_ == null)
			{
				Log.E("can not handle OnSaveOptions, we are not initialized yet");
				return;
			}

			try
			{
				var options = _buildOnSave_.SolutionOptions;
				var serialized = JsonConvert.SerializeObject(options);
				Log.D("serialized and saving solution options: {options}", serialized);
				var bytes = Encoding.UTF8.GetBytes(serialized);
				stream.Write(bytes, 0, bytes.Length);
			}
			catch (Exception e)
			{
				Log.E("failed to serialize options: {exception}", e);
			}
		}

		static string streamToString(Stream stream)
		{
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
			_buildOnSave_ = new BuildOnSave(this);
		}
	}
}