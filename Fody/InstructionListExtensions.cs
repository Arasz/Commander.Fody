﻿using System;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public static class InstructionListExtensions
{
    public static void Prepend(this Collection<Instruction> collection, params Instruction[] instructions)
    {
        for (var index = 0; index < instructions.Length; index++)
        {
            var instruction = instructions[index];
            collection.Insert(index, instruction);
        }
    }
    public static void BeforeLast(this Collection<Instruction> collection, params Instruction[] instructions)
    {
        var index = collection.Count - 1;
        foreach (var instruction in instructions)
        {
            collection.Insert(index, instruction);
            index++;
        }
    }
    public static void Append(this Collection<Instruction> collection, params Instruction[] instructions)
    {
        for (var index = 0; index < instructions.Length; index++)
        {
            collection.Insert(index, instructions[index]);
        }
    }

    public static Instruction GetLastInstructionWhere(this Collection<Instruction> collection, Func<Instruction, bool> predicate)
    {
        for (int idx = collection.Count - 1; idx >= 0; idx--)
        {
            var instruction = collection[idx];
            if (predicate(instruction))
            {
                return instruction;
            }            
        }

        return null;
    }
}