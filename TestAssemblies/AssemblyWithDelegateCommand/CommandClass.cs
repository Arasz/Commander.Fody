﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

public class CommandClass
{
    public string Name { get; set; }

    public void NotCommandMethod()
    {
    }

    [OnCommand("TestCommand")]
    public void OnTestCommand()
    {
    }

    [OnCommand("TestCommand2")]
    public void OnTestCommand2()
    {
        
    }

    [OnCommandCanExecute("SubmitCommand")]
    public bool CanSubmit()
    {
        return true;
    }

    [OnCommand("SubmitCommand")]
    public void OnSubmitCommand()
    {
        
    }
}

public class ExampleCommandClassWithInitializer
{
    public ExampleCommandClassWithInitializer()
    {
        this.InitializeCommands();
    }

    private void InitializeCommands()
    {
        if (TestCommand == null)
        {
            TestCommand = new DelegateCommand(OnTestCommand);
        }

        if (SubmitCommand == null)
        {
            SubmitCommand = new DelegateCommand(OnSubmit, CanSubmit);
        }

        if (NullCommand == null)
        {
            NullCommand = null;
        }
    }

    public ICommand TestCommand { get; set; }
    public ICommand SubmitCommand { get; set; }
    public ICommand NullCommand { get; set; }
    public ICommand NestedCommand { get; set; }

    public void OnTestCommand()
    {        
    }

    public bool CanSubmit()
    {
        return true;
    }

    public void OnSubmit()
    {        
    }

    public bool CanExecuteNestedCommand()
    {
        return true;
    }

    public void OnExecuteNestedCommand()
    {        
    }

    private class NestedCommandImplementation : ICommand
    {
        private readonly ExampleCommandClassWithInitializer _owner;

        public NestedCommandImplementation(ExampleCommandClassWithInitializer owner)
        {
            _owner = owner;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _owner.OnExecuteNestedCommand();
        }

        public bool CanExecute(object parameter)
        {
            return _owner.CanExecuteNestedCommand();
        }        
    }
}