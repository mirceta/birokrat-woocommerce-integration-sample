using administration_data;
using administration_data.data.structs;
using gui_generator;
using gui_generator.api;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using tests.composition.fixed_task.common;
using with_sql_versioning_integobjconfig_gui;

namespace tests_gui.integ_loading
{
    public interface IIntegrationSourceHandler
    {
        Task<List<string>> getIntegrationNames();
        void editIntegrationClick(string nameOfIntegration);
    }

    public class IntegrationSourceHandlerFacade
    {
        private readonly MultiplexIntegrationSourceHandler _multiplexMatters;

        public IntegrationSourceHandlerFacade(string selectedType, IntegrationFactoryBuilder factoryBuilder)
        {
            string connectionString = factoryBuilder.getSqlServer();

            // Create instances of PreMatters and JsonMatters
            var preMatters = new PreIntegrationSourceHandler(factoryBuilder, new administration_data.IntegrationDao(connectionString), connectionString);


            // ADD WHEN READY!
            var jsonMatters = new JsonIntegrationSourceHandler(new administration_data.IntegrationDao(connectionString), connectionString);

            // Create the mapping dictionary
            var integrationObjectsMap = new Dictionary<string, IIntegrationSourceHandler>
            {
                { "PRE", preMatters },
                { "JSON", jsonMatters }
            };

            // Initialize MultiplexMatters with the provided type
            _multiplexMatters = new MultiplexIntegrationSourceHandler(integrationObjectsMap, selectedType);
        }

        public IIntegrationSourceHandler Get()
        {
            return _multiplexMatters;
        }
    }

    internal class VoidPreIntegrationSourceHandler : IIntegrationSourceHandler
    {
        IntegrationFactoryBuilder factoryBuilder;
        IntegrationDao integrationDao;
        string cs;
        public VoidPreIntegrationSourceHandler(IntegrationFactoryBuilder factoryBuilder)
        {
            this.factoryBuilder = factoryBuilder;
        }
        public async Task<List<string>> getIntegrationNames()
        {
            var factory = factoryBuilder.build("http://localhost:19000/api", "");
            var lazies = factory.GetAllLazy();
            var result = await lazies;
            var names = result.Select(x => x.Name).ToList();

            return names;
        }
        public void editIntegrationClick(string nameOfIntegration)
        {
        }
    }

    internal class PreIntegrationSourceHandler : IIntegrationSourceHandler
    {
        /*
         FOR PRE USE THIS ONE WHEN YOU HAVE SETUP JSON SQL DATABASE!!!!
         */


        IntegrationFactoryBuilder factoryBuilder;
        IntegrationDao integrationDao;
        string cs;
        public PreIntegrationSourceHandler(IntegrationFactoryBuilder factoryBuilder, IntegrationDao integrationDao, string connectionString)
        {
            this.factoryBuilder = factoryBuilder;
            this.integrationDao = integrationDao;
            this.cs = connectionString;
        }
        public async Task<List<string>> getIntegrationNames()
        {
            var factory = factoryBuilder.build("http://localhost:19000/api", "");
            var lazies = factory.GetAllLazy();
            var result = await lazies;
            var names = result.Select(x => x.Name).ToList();
            return names;
        }
        public async void editIntegrationClick(string nameOfIntegration)
        {
            string disclaimer = "Integracij v načinu dela PRE (programska tovarna integracij) ni mogoče spreminajti.";
            disclaimer += "Ali želite izbrano integracijo prenesti v obliko JSON ki je spremenljiva?";
            DialogResult result = MessageBox.Show(disclaimer, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                var tmp = factoryBuilder.build("http://localhost:19000", "");
                var lazyIntegration = await tmp.GetLazyByName(nameOfIntegration);
                var integration = await lazyIntegration.BuildIntegrationAsync();

                var curval = new LazyIntegrationAdapter().Adapt(integration);

                CurrentValueProcessor.RemoveUnnecessaryElementsAndNormalize(curval);
                string json = JsonConvert.SerializeObject(curval);

                var contentDao = new ContentDao(cs);
                var integVerDao = new IntegrationVersionDao(cs);

                var mng = new SqlIntegrationsManager(
                    integVerDao,
                    contentDao,
                    integrationDao); // !!!!! CANNOT BE FIXED TO BIROTOWOO!


                string integrationType = "";
                if (integration.Name.Contains("BIROTOWOO") && integration.Name.Contains("WOOTOBIRO"))
                {
                    throw new Exception($"{integration.Name} is both BIROTOWOO AND WOOTOBIRO. It must be exactly one of those or the transfer is not possible.");
                }
                else if (integration.Name.Contains("BIROTOWOO"))
                {
                    integrationType = "BIROTOWOO";
                }
                else if (integration.Name.Contains("WOOTOBIRO"))
                {
                    integrationType = "WOOTOBIRO";
                }
                else {
                    throw new Exception($"{integration.Name} is neither BIROTOWOO nor WOOTOBIRO. It must be exactly one of those or the transfer is not possible.");
                }

                int integId = mng.NewIntegration(nameOfIntegration, json, integrationType);
                MessageBox.Show("Če želite novonastalo integracijo uporabiti potem zaprite program" +
                    " in ga ponovno odprite. Za izvor integracijskih objektov zdaj izberite obliko JSON." +
                    " Nova integracija se bo pojavila na levem seznamu.");
            }
        }
    }

    internal class JsonIntegrationSourceHandler : IIntegrationSourceHandler
    {
        IntegrationDao integrationDao;
        string connectionString;
        public JsonIntegrationSourceHandler(IntegrationDao integrationDao, string connectionString)
        {
            this.integrationDao = integrationDao;
            this.connectionString = connectionString;
        }
        public async Task<List<string>> getIntegrationNames()
        {
            return integrationDao.GetAll().Select(x => x.Name).ToList();
        }
        public void editIntegrationClick(string nameOfIntegration)
        {
            var form = new Versioning_IntegrationObjectConfigForm(connectionString, nameOfIntegration);
            form.Show();
        }
    }

    internal class MultiplexIntegrationSourceHandler : IIntegrationSourceHandler
    {
        private readonly Dictionary<string, IIntegrationSourceHandler> _integrationObjectsMap;
        private readonly string _selectedType;

        public MultiplexIntegrationSourceHandler(Dictionary<string, IIntegrationSourceHandler> integrationObjectsMap, string selectedType)
        {
            _integrationObjectsMap = integrationObjectsMap;
            _selectedType = selectedType;
        }

        public async Task<List<string>> getIntegrationNames()
        {
            if (_integrationObjectsMap.ContainsKey(_selectedType))
            {
                var result = await _integrationObjectsMap[_selectedType].getIntegrationNames();
                return result;
            }
            else
            {
                throw new Exception("Unrecognized type!");
            }
        }

        public void editIntegrationClick(string nameOfIntegration)
        {
            if (_integrationObjectsMap.ContainsKey(_selectedType))
            {
                _integrationObjectsMap[_selectedType].editIntegrationClick(nameOfIntegration);
            }
            else
            {
                throw new Exception("Unrecognized integration factory type");
            }
        }
    }

    public class CurrentValueProcessor
    {
        public static void RemoveUnnecessaryElementsAndNormalize(CurrentValue o)
        {
            if (o == null) return;

            var properties = o.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var value = (string)property.GetValue(o);
                    if (value == "null" || value == "")
                    {
                        property.SetValue(o, null);
                    }
                    else if (value == "True")
                    {
                        property.SetValue(o, "True");
                    }
                    else if (value == "False")
                    {
                        property.SetValue(o, "False");
                    }
                }
                else if (property.PropertyType == typeof(bool))
                {
                    var value = (bool)property.GetValue(o);
                    property.SetValue(o, value ? "True" : "False");
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                    var enumerable = (IEnumerable)property.GetValue(o);
                    if (enumerable != null) // Assuming we need to normalize elements within the collection
                    {
                        foreach (var element in enumerable)
                        {
                            if (element is CurrentValue)
                            {
                                RemoveUnnecessaryElementsAndNormalize((CurrentValue)element);
                            }
                        }
                    }
                }
            }

            // Specific property handling
            o.implementationOptions = null;
            if (o.typeCategory == "enum")
            {
                o.elements = null;
            }

            // Assuming Dependencies and Elements are collections of CurrentValue
            if (o.dependencies != null)
            {
                foreach (var dependency in o.dependencies)
                {
                    RemoveUnnecessaryElementsAndNormalize(dependency);
                }
            }

            if (o.elements != null)
            {
                foreach (var element in o.elements)
                {
                    RemoveUnnecessaryElementsAndNormalize(element);
                }
            }
        }
    }
}
