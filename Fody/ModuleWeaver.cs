﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public class ModuleWeaver: IFodyLogger
    {
        public ModuleWeaver()
        {
            LogInfo = m => { };
            LogWarning = m => { };
            LogWarningPoint = (m, p) => { };
            LogError = m => { };
            LogErrorPoint = (m, p) => { };
        }

        // Will contain the full element XML from FodyWeavers.xml. OPTIONAL
        public XElement Config { get; set; }
        public ModuleWeaverSettings Settings { get; set; }

        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }
        public Action<string, SequencePoint> LogWarningPoint { get; set; }
        public Action<string> LogError { get; set; }
        public Action<string, SequencePoint> LogErrorPoint { get; set; }

        public ModuleDefinition ModuleDefinition { get; set; }
        public IAssemblyResolver AssemblyResolver { get; set; }
        public Assets Assets { get; private set; }        

        public void Execute()
        {
            Setup();
            var processors = GetProcessors();
            ExecuteProcessors(processors);
        }

        private static void ExecuteProcessors(IEnumerable<ModuleProcessorBase> processors)
        {
            foreach (var processor in processors)
            {
                processor.Execute();
            }
        }

        private IEnumerable<ModuleProcessorBase> GetProcessors()
        {
            var processors = new ModuleProcessorBase[]
            {
                new CommandAttributeScanner(this),
                new ClassInjectionProcessor(this),
                new ModuleTypesProcessor(this)
            };
            return processors;
        }

        private void Setup()
        {
            Settings = new ModuleWeaverSettings(Config);
            Assets = new Assets(this);
        }
    }
}