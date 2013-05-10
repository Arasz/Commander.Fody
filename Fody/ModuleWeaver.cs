﻿using System;
using System.Linq;
using System.Text;
using Mono.Cecil;

public class ModuleWeaver
{
    public ModuleWeaver()
    {
        LogInfo = s => { };
    }

    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }
    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }    

    public void Execute()
    {
        var context = new ModuleWeavingContext(ModuleDefinition, LogInfo);
        Prepare(context);
        AddCommandInitialization(context);
    }

    public void Prepare(ModuleWeavingContext moduleContext)
    {
        foreach (var type in moduleContext.AllTypes)
        {
            Prepare(type, moduleContext);
        }
    }

    public void Prepare(TypeDefinition typeDefinition, ModuleWeavingContext context)
    {
        var onCommandMethods = typeDefinition.FindOnCommandMethods().ToList();
        if (!onCommandMethods.Any())
        {
            return;
        }
        var typeContext = context.GetTypeWeavingContext(typeDefinition);
        context.WeavableTypes.Add(typeContext.Type);
        foreach (var method in onCommandMethods)
        {
            ProcessOnCommandMethod(method, typeContext);
        }
    }

    public void ProcessOnCommandMethod(MethodDefinition method, TypeWeavingContext context)
    {
        var onCommandAttributes = 
            method.CustomAttributes
                .Where(attrib => attrib.Constructor.DeclaringType.Name == "OnCommandAttribute")
                .Where(attrib => attrib.HasConstructorArguments && attrib.ConstructorArguments.First().Type.Name == "String");
        var commandNames = onCommandAttributes.Select(attrib => attrib.ConstructorArguments.First().Value).OfType<string>();
        var type = method.DeclaringType;
        var icommandTypeRef = context.ModuleContext.CommonTypes.ICommand;
        foreach (var commandName in commandNames)
        {
            LogInfo(string.Format("Found OnCommand method {0} for command {1} on type {2}"
                                  , method
                                  , commandName
                                  , type.Name));
            try
            {
                PropertyDefinition propertyDefinition;
                if (context.Type.TypeDefinition.TryAddCommandProperty(icommandTypeRef, commandName, out propertyDefinition))
                {
                    var commandData = new CommandData(commandName) {CommandProperty = propertyDefinition};
                    context.Type.ReferencedCommands.Add(commandData);
                    context.Type.InjectedCommands.Add(commandData);
                }
                else
                {
                    var commandData = new CommandData(commandName) { CommandProperty = propertyDefinition };
                    context.Type.ReferencedCommands.Add(commandData);
                }
            }
            catch (Exception ex)
            {
                LogInfo(string.Format("Error while adding property {0} to {1}: {2}", commandName, type, ex));
            }
        }               
    }

    public void AddCommandInitialization(ModuleWeavingContext context)
    {
        MethodDefinition initializeMethod = null;
        foreach (var typeNode in context.WeavableTypes)
        {
            if (typeNode.ReferencedCommands.Any())
            {                
                typeNode.TypeDefinition.TryAddCommandInitializerMethod(out initializeMethod);
            }
            if (initializeMethod == null)
            {
                // TODO: First log/notify that there is a problem
                continue;
            }
            foreach (var command in typeNode.ReferencedCommands)
            {
                typeNode.TypeDefinition.TryAddCommandPropertyInitialization(initializeMethod, command);
            }
            initializeMethod = null;
        }
    }
}