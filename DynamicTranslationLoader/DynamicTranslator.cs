﻿using BepInEx;
using System;
using System.ComponentModel;
using System.IO;
using BepInEx.Logging;
using DynamicTranslationLoader.Image;
using DynamicTranslationLoader.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace DynamicTranslationLoader
{
    [BepInPlugin(GUID: "com.bepis.bepinex.dynamictranslator", Name: "Dynamic Translator", Version: "3.0")]
    public class DynamicTranslator : BaseUnityPlugin
    {
        public static event Func<object, string, string> OnUnableToTranslateUGUI;
        public static event Func<object, string, string> OnUnableToTranslateTextMeshPro;

        // Settings
        public static SavedKeyboardShortcut ReloadTranslations { get; set; }
        public static SavedKeyboardShortcut DumpUntranslatedText { get; set; }

        [DisplayName("!Enable image dumping")]
        [Description("Extract and save all in-game UI images to BepInEx\\translation\\Images\nWarning: Very slow, disable when not needed")]
        [Advanced(true)]
        public static ConfigWrapper<bool> IsDumpingEnabled { get; set; }
        [DisplayName("Dump all images to global folder")]
        [Advanced(true)]
        public static ConfigWrapper<bool> DumpingAllToGlobal { get; set; }
        
        public DynamicTranslator()
        {
            IsDumpingEnabled = new ConfigWrapper<bool>("dumping", this);
            DumpingAllToGlobal = new ConfigWrapper<bool>("dump-to-global", this);
            ReloadTranslations = new SavedKeyboardShortcut("Reload translations", this, new KeyboardShortcut(KeyCode.F10));
            DumpUntranslatedText = new SavedKeyboardShortcut("Dump untranslated text", this, new KeyboardShortcut(KeyCode.F10, KeyCode.LeftShift));
        }

        public void Awake()
        {
            var dirTranslation = Path.Combine(Paths.PluginPath, "translation");
            SetupTextTl(dirTranslation);
            SetupImageTl(dirTranslation);
        }
        
        public void Update()
        {
            if (Event.current == null) return;
            if (ReloadTranslations.IsDown())
            {
                TextTranslator.RetranslateText();
                Logger.Log(LogLevel.Message, "Translation reloaded.");
            }
            else if (DumpUntranslatedText.IsDown())
            {
                TextTranslator.DumpText();
                Logger.Log(LogLevel.Message, $"Text dumped to \"{Path.GetFullPath("dumped-tl.txt")}\"");
            }
        }

	    public void OnEnable()
	    {
		    SceneManager.sceneLoaded += TextTranslator.TranslateScene;
	    }
		
	    public void OnDisable()
	    {
		    SceneManager.sceneLoaded -= TextTranslator.TranslateScene;
	    }

        private static void SetupTextTl(string dirTranslation)
        {
            TextTranslator.LoadTextTranslations(dirTranslation);

            TextHooks.InstallHooks();

            ResourceRedirector.ResourceRedirector.AssetResolvers.Add(TextTranslator.RedirectHook);

            TextTranslator.TranslateTextAll();
        }

        private static void SetupImageTl(string dirTranslation)
        {
            ImageTranslator.LoadImageTranslations(dirTranslation);
            
            ImageHooks.InstallHooks();
        }

        internal static string OnOnUnableToTranslateUgui(object arg1, string arg2)
        {
            return OnUnableToTranslateUGUI?.Invoke(arg1, arg2);
        }

        internal static string OnOnUnableToTranslateTextMeshPro(object arg1, string arg2)
        {
            return OnUnableToTranslateTextMeshPro?.Invoke(arg1, arg2);
        }
    }
}