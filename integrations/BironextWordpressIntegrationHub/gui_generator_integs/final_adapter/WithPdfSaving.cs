using BiroWooHub.logic.integration;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System;
using gui_generator;
using System.Threading.Tasks;

namespace gui_generator_integs.final_adapter
{
    public class WithPdfSaving : ILazyIntegrationAdapter
    {

        int taskId;
        string connectionString;
        string datafolder;
        string desiredName;
        ILazyIntegrationAdapter next;
        public WithPdfSaving(int taskId, string connectionString, string datafolder, string desiredName, ILazyIntegrationAdapter next)
        {
            this.taskId = taskId;
            this.connectionString = connectionString;
            this.datafolder = datafolder;
            this.desiredName = desiredName;
            this.next = next;
        }
        public Task<IIntegration> AdaptFinal(CurrentValue content, string integrationType)
        {
            if (taskId != -1)
            {
                modify(content, Path.Combine(datafolder, desiredName));
            }
            return next.AdaptFinal(content, integrationType);
        }

        void modify(CurrentValue some, string datafolder)
        {
            Func<CurrentValue, bool> condition = (value) =>
                                                    value.type.Contains("IOrderOperationCR") &&
                                                    value.dependencies.Where(x => x.variable == "next").Single().dependencies == null;
            Func<CurrentValue, CurrentValue> action = (value) =>
            {
                value.type = "IOrderOperationCR.SaveDocumentOrderOperationCR";
                value.dependencies = new CurrentValue[] {
                    new CurrentValue() {
                        value = "@@IApiClientV2",
                        variable = "client"
                    },
                    new CurrentValue()
                    {
                        variable = "next",
                        typeCategory = "class",
                        type = "IOrderOperationCR.SavePdfOrderOperationCr",
                        dependencies = new CurrentValue[] {
                            new CurrentValue()
                            {
                                value = "@@IApiClientV2",
                                variable = "client"
                            },
                            new CurrentValue()
                            {
                                variable = "connectionString",
                                typeCategory = "primitive",
                                type = "String",
                                value = connectionString
                            },
                            new CurrentValue()
                            {
                                variable = "taskId",
                                typeCategory = "primitive",
                                type = "Int32",
                                value = taskId + ""
                            },
                            new CurrentValue()
                            {
                                variable = "next",
                                typeCategory = "class",
                                type = "IOrderOperationCR"
                            }
                        }
                    },
                    new CurrentValue()
                    {
                        variable = "filepath",
                        typeCategory = "primitive",
                        type = "String",
                        value = datafolder
                    }
                };
                return value;
            };


            recurse(some, condition, action);
        }

        void recurse(CurrentValue value, Func<CurrentValue, bool> condition, Func<CurrentValue, CurrentValue> action)
        {
            if (value == null || value.dependencies == null && value.elements == null)
                return;
            if (condition(value))
            {
                value = action(value);
            }
            else
            {
                if (value.dependencies != null)
                {
                    foreach (var dependency in value.dependencies)
                    {
                        recurse(dependency, condition, action);
                    }
                }
                if (value.elements != null)
                {
                    foreach (var el in value.elements)
                    {
                        recurse(el, condition, action);
                    }
                }
            }
        }
    }




}