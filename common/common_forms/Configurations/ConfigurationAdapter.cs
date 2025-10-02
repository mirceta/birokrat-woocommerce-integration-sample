using common_forms.Configurations;
using common_forms.Configurations.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace common_forms.Configurations
{
    /// <summary>
    /// naming convention prefixes: tb - textbox, rtb - RichTextBox, chb - CheckBox, cbb - ComboBox, rbtn - RadioButton
    /// Example: rtbTextBox will come out from GetControlNameWithoutPrefix as TextBox
    /// </summary>
    public class ConfigurationAdapter
    {
        public bool SavedByAdapter { get; private set; } = false;
        private readonly List<IControlHandler> _acceptedHandlers;
        private readonly AdapterHelper _adapterHelper;

        /// <summary>
        /// <inheritdoc cref="ConfigurationAdapter"/>
        /// </summary>
        public ConfigurationAdapter()
        {
            _adapterHelper = new AdapterHelper();
            _acceptedHandlers = new List<IControlHandler>();
        }

        public ConfigurationAdapter ConfigureHandler<T>() where T : IControlHandler, new()
        {
            if (!_acceptedHandlers.Any(h => h.GetType() == typeof(T)))
            {
                _acceptedHandlers.Add(new T());
            }
            return this;
        }

        public ConfigurationAdapter ConfigureToIgnoreNamingConvention()
        {
            ControlCheckHandler.Shared.RespectNamingConvention = false;
            return this;
        }

        /// <summary>
        /// Packs control values from the given parent control into a dictionary.
        /// </summary>
        public Dictionary<string, string> PackControls(Control parent)
        {
            return _adapterHelper.PackControls(parent, _acceptedHandlers);
        }

        /// <summary>
        /// Unpacks and assigns values from the dictionary to controls within the given parent control.
        /// </summary>
        public void UnpackControls(Control parent, Dictionary<string, string> config)
        {
            if (config == null)
                return;

            SavedByAdapter = true;
            _adapterHelper.UnpackControls(parent, _acceptedHandlers, config);
            SavedByAdapter = false;
        }
    }
}
