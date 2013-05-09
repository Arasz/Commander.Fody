﻿using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class CommandPropertyInjector
{
    public static PropertyDefinition AddProperty(TypeReference propertyType, TypeDefinition targetType, string commandName)
    {
        var propertyDefinition = targetType.Properties.FirstOrDefault(x => x.Name == commandName);
        if (propertyDefinition != null)
        {
            propertyDefinition.ValidateIsOfType(propertyType);
            return propertyDefinition;
        }        
        propertyDefinition = new PropertyDefinition(commandName, PropertyAttributes.HasDefault, propertyType);
        targetType.Properties.Add(propertyDefinition);
        AddPropertyGetter(propertyDefinition);
        return propertyDefinition;
    }

    public static MethodDefinition AddPropertyGetter(
        PropertyDefinition property
        , MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual
        , FieldDefinition backingField = null)
    {
        if (backingField == null)
        {
            // TODO: Try and find existing friendly named backingFields first.
            var fieldName = string.Format("<{0}>k__BackingField", property.Name);
            backingField = property.DeclaringType.AddField(property.PropertyType, fieldName);
        }

        var methodName = "get_" + property.Name;
        var getter = new MethodDefinition(methodName, methodAttributes, property.PropertyType)
        {
            IsGetter = true,
            Body = {InitLocals = true},
        };

        getter.Body.Variables.Add(new VariableDefinition(property.PropertyType));

        var returnStart = Instruction.Create(OpCodes.Ldloc_0);
        getter.Body.Instructions.Append(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, backingField),
            Instruction.Create(OpCodes.Stloc_0),
            Instruction.Create(OpCodes.Br_S, returnStart),
            returnStart,
            Instruction.Create(OpCodes.Ret)
        );        
                
        property.GetMethod = getter;
        property.DeclaringType.Methods.Add(getter);
        return getter;
    }
}