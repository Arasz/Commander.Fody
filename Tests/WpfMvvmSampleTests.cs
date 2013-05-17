﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Commander.Fody;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class WpfMvvmSampleTests : BaseTaskTests
    {
        public WpfMvvmSampleTests()
            : base(@"TestAssemblies\WpfMvvmSample\WpfMvvmSample.csproj", Console.WriteLine)
        {
            
        }

        [Test]
        public void DelegateCommand_Class_Should_Be_Injected()
        {
            var delegateCommandType = (
                from type in Assembly.GetTypes()
                where type.IsClass
                    && type.Name == "<Commander_Fody>__DelegateCommand"
                select type).FirstOrDefault();

            Assert.That(delegateCommandType, Is.Not.Null);
        }

        [Test]
        public void Injected_DelegateCommand_Should_Implement_ICommand()
        {
            var delegateCommandType = (
                from type in Assembly.GetTypes()
                where type.IsClass
                    && type.Name == "<Commander_Fody>__DelegateCommand"
                select type).FirstOrDefault();

            var iface = delegateCommandType.GetInterface("ICommand");
            Assert.That(iface, Is.Not.Null);
        }

        [Test]
        public void SurveyViewModel_Should_Have_CommandInitialization_Injected()
        {
            var instance = Assembly.GetInstance("WpfMvvmSample.SurveyViewModel");
            var objectInstance = (object)instance;
            var type = DefinitionFinder.FindType(objectInstance.GetType());
            var method = type.FindMethod("<Commander_Fody>InitializeCommands");
            method.Should().NotBeNull();
        }

        [Test]
        public void SurveyViewModel_Should_Have_CommandInitialization_Injected_With_SubmitCommand_Set()
        {
            var ilVisitor = Substitute.For<IILVisitor>();
            ilVisitor.OnInlineMethod(Arg.Is(OpCodes.Call), Arg.Any<MethodReference>());
            var instance = Assembly.GetInstance("WpfMvvmSample.SurveyViewModel");
            var objectInstance = (object)instance;
            var type = DefinitionFinder.FindType(objectInstance.GetType());
            var method = type.FindMethod("<Commander_Fody>InitializeCommands");
            ILParser.Parse(method, ilVisitor);
            ilVisitor.Received().OnInlineMethod(
                Arg.Is(OpCodes.Call)
                , Arg.Is<MethodReference>(x => x.Name == "set_SubmitCommand" && x.DeclaringType.Name == "SurveyViewModel")
            );
        }

        [Test]
        public void TestCommand_Should_Be_Injected()
        {
            var instance = Assembly.GetInstance("WpfMvvmSample.SurveyViewModel");
            var submitCommand = instance.SubmitCommand;
            object submitCommandObject = submitCommand;
            submitCommandObject.Should().NotBeNull("SubmitCommand should be initialized.");
            var type = (submitCommandObject).GetType();
            //type.Name.Should().Be("<Commander_Fody>__DelegateCommand");
            type.Name.Should().Be("<>__NestedCommandImplementationForSubmitCommand");
        }
    }
}
