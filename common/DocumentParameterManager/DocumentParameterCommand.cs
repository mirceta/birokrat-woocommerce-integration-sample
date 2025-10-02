using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    public class DocumentParameterCommand
    {

        public class Builder
        {


            public IDocumentParameterCommandCondition condition = null;
            public ParameterOperation operation;
            public string fieldName;
            public IDocumentParameterCommandValue value;
            public string replaceWith;

            public bool requiresRefresh;

            public Builder()
            {

            }

            public Builder SetValue(IDocumentParameterCommandValue value)
            {
                this.value = value;
                return this;
            }

            public Builder SetCondition(IDocumentParameterCommandCondition condition)
            {
                this.condition = condition;
                return this;
            }

            public Builder SetOperation(ParameterOperation operation)
            {
                this.operation = operation;
                return this;
            }

            public Builder SetFieldName(string fieldName)
            {
                this.fieldName = fieldName;
                return this;
            }

            public Builder SetReplaceWith(string value)
            {
                this.replaceWith = value;
                return this;
            }

            public DocumentParameterCommand Build()
            {

                // condition
                if (condition == null)
                {
                    condition = new Tautology();
                }

                // operation & replaceWith
                if (operation == ParameterOperation.REPLACE && replaceWith == null)
                {
                    throw new Exception("");
                }



                // fieldName
                if (string.IsNullOrEmpty(fieldName))
                {
                    throw new Exception("");
                }

                // value

                if (fieldName == "cmbVrstaProdaje")
                {
                    requiresRefresh = true;
                }

                return new DocumentParameterCommand(this);
            }

        }

        public DocumentParameterCommand(
            IDocumentParameterCommandCondition condition,
            ParameterOperation operation,
            string fieldName,
            IDocumentParameterCommandValue value,
            string replaceWith = null,
            bool requiresRefresh = false)
        {
            // condition
            this.condition = condition ?? new Tautology();

            // operation & replaceWith validation
            if (operation == ParameterOperation.REPLACE && replaceWith == null)
            {
                throw new ArgumentException("ReplaceWith cannot be null when operation is REPLACE.");
            }

            // fieldName validation
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("FieldName cannot be null or empty.");
            }

            // value validation and other initializations
            this.operation = operation;
            this.fieldName = fieldName;
            this.value = value ?? throw new ArgumentException("Value function cannot be null.");
            this.replaceWith = replaceWith;

            // Specific field logic
            this.requiresRefresh = fieldName == "cmbVrstaProdaje" || requiresRefresh;
        }

        private DocumentParameterCommand(Builder builder)
        {
            condition = builder.condition;
            operation = builder.operation;
            fieldName = builder.fieldName;
            value = builder.value;
            replaceWith = builder.replaceWith;

            requiresRefresh = builder.requiresRefresh;
        }

        bool requiresRefresh;
        IDocumentParameterCommandCondition condition;
        ParameterOperation operation;
        string fieldName;
        IDocumentParameterCommandValue value;
        string replaceWith;
        public bool RequiresRefresh { get => requiresRefresh; }
        public IDocumentParameterCommandCondition Condition { get => condition; }
        public ParameterOperation Operation { get => operation; }
        public string FieldName { get => fieldName; }
        public IDocumentParameterCommandValue Value { get => value; }
        public string ReplaceWith { get => replaceWith; }
    }

    public enum ParameterOperation
    {
        APPEND = 1,
        SET = 2,
        REPLACE = 3
    }

    public class Tautology : IDocumentParameterCommandCondition
    {
        public bool Is(object order, Dictionary<string, object> data)
        {
            return true;
        }
    }

    public class Const : IDocumentParameterCommandValue
    {

        string value;
        public Const(string value)
        {
            this.value = value;
        }

        public string Get(object order, Dictionary<string, object> data)
        {
            return value;
        }
    }
}
