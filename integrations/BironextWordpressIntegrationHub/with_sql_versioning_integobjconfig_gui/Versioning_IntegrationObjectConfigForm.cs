using administration_data;
using administration_data.data.structs;
using gui_gen;
using gui_generator;
using gui_generator.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace with_sql_versioning_integobjconfig_gui
{
    public partial class Versioning_IntegrationObjectConfigForm : Form
    {

        IntegrationDao integrationDao;
        IntegrationVersionDao integrationVersionDao;
        ContentDao contentDao;
        ConfigModificationEventDao configModificationEventDao;

        ConfigRenderer configRenderer;


        string original = "";
        int integId;
        IntegrationVersion version;

        public Versioning_IntegrationObjectConfigForm(string connectionString, string nameOfIntegration)
        {
            InitializeComponent();

            integrationDao = new IntegrationDao(connectionString);
            integrationVersionDao = new IntegrationVersionDao(connectionString);
            contentDao = new ContentDao(connectionString);
            configModificationEventDao = new ConfigModificationEventDao(connectionString);
            integId = integrationDao.GetAll().Where(x => x.Name == nameOfIntegration).Single().Id;

            this.Load += InitializeAsync;
        }

        public async void InitializeAsync(object sender, EventArgs e)
        {
            await Rerender();
        }

        ZoomablePanel currentConfigPanel;

        async Task Rerender()
        {
            version = integrationVersionDao.GetByIntegrationId(integId).OrderByDescending(x => x.Id).Take(1).Single(); // get the latest version
            var content = contentDao.Get(version.ContentId);
            var adapter = new LazyIntegrationAdapter();
            var some = adapter.Adapt(JsonConvert.DeserializeObject<CurrentValue>(content.Data), "BIROTOWOO").Get().BuildIntegrationAsync();

            var integration = await some;
            CurrentValue currentValue = adapter.Adapt(integration);
            original = JsonConvert.SerializeObject(currentValue);


            int elementWidth = 1500;

            currentConfigPanel = new ZoomablePanel();
            currentConfigPanel.Width = pnlConfig.Width;
            currentConfigPanel.Height = pnlConfig.Height;
            pnlConfig.Controls.Add(currentConfigPanel);


            configRenderer = new ConfigRenderer(currentConfigPanel, elementWidth);
            configRenderer.Render(currentValue);

            new VersionInfo(pnlVersionData).Render(version);


            Action<string, string> actionHandler = (action, input) =>
            {
                onActionClicked(action, input);
            };

            Actions actionsControl = new Actions(pnlActions, true, version.Actions, actionHandler);
            actionsControl.Render();


            var history = configModificationEventDao.GetByIntegrationId(integId);
            new HistoryComponent(pnlHistory, history).Render();
        }

        void onActionClicked(string action, string inputValue)
        {

            var orig = JsonConvert.DeserializeObject<CurrentValue>(original);
            IntegrationConfigTools.NullImplementationOptions(orig);
            string json = JsonConvert.SerializeObject(orig);

            CurrentValue currVal = configRenderer.CollectCurrentValue();
            string sm = JsonConvert.SerializeObject(currVal);
            var changed = JsonConvert.DeserializeObject<CurrentValue>(sm); // the currVal original should not be mutated (no nulling impl opts). Thus we just copy it.


            IntegrationConfigTools.NullImplementationOptions(changed);
            string json2 = JsonConvert.SerializeObject(changed);


            try
            {
                var adapter = new LazyIntegrationAdapter();
                var some = adapter.Adapt(JsonConvert.DeserializeObject<CurrentValue>(json2), "BIROTOWOO").Get().BuildIntegrationAsync();
            }
            catch (Exception ex) {
                MessageBox.Show("Cannot change integration because the new version does not compile. Please reconfigure so that it is a valid integration object." + ex.InnerException.Message + ex.InnerException.StackTrace.ToString());
                return;
            }

            var resCompare = new ResultCompare(json, json2);
            resCompare.setOnOkClick(async () =>
            {
                version = new SqlIntegrationsManager(integrationVersionDao, contentDao, integrationDao)
                    .CommitVersionAction(version, action, inputValue, json2);
                await Rerender();
                resCompare.Close();
            });
            resCompare.Show();
        }

        private async void btnResetChanges_Click(object sender, EventArgs e)
        {
            await Rerender(); // on whole interface rerender, it will be reset to default!
        }

        private void btnViewChangesSinceLastVersion_Click(object sender, EventArgs e)
        {
            var version = integrationVersionDao.GetByIntegrationId(integId).OrderByDescending(x => x.Id);

            if (version.Count() < 2) {
                MessageBox.Show("This is the first version.");
                return;
            }
            var tmp = version.Take(2).ToList();

            string json = contentDao.Get(tmp[1].ContentId).Data; // old version
            string json2 = contentDao.Get(tmp[0].ContentId).Data; // current version

            new ResultCompare(json, json2).Show();
        }
    }

    class Actions
    {
        private Panel hostPanel;
        private bool renderTextbox;
        private List<string> actions;
        private Action<string, string> onActionSelected;
        private TextBox inputTextBox;

        public Actions(Panel hostPanel, bool renderTextbox, List<string> actions, Action<string, string> onActionSelected)
        {
            this.hostPanel = hostPanel;
            this.renderTextbox = renderTextbox;
            this.actions = actions;
            this.onActionSelected = onActionSelected;
        }

        public void Render()
        {
            hostPanel.Controls.Clear();  // Clear existing controls
            int xPos = 10;  // Starting position for the buttons
            int yPos = 10;  // Starting vertical position

            // Create and position buttons horizontally
            foreach (var action in actions)
            {
                var button = new Button
                {
                    Text = action,
                    Location = new Point(xPos, yPos),
                    Size = new Size(100, 30),  // Adjust size as needed
                };
                button.Click += (sender, e) => ButtonClickHandler(action);
                hostPanel.Controls.Add(button);
                xPos += 110;  // Adjust horizontal spacing and width of each button
                if (xPos + 100 > hostPanel.Width)  // Check if next button fits in current row
                {
                    xPos = 10;  // Reset to first column
                    yPos += 40;  // Move to next row
                }
            }

            int textBoxHeight = hostPanel.Height - yPos - 40;  // Calculate remaining height for text box
            if (renderTextbox && textBoxHeight > 0)
            {
                inputTextBox = new TextBox
                {
                    Location = new Point(10, yPos + 40),  // Position below the last button row
                    Size = new Size(hostPanel.Width - 20, textBoxHeight),  // Use calculated height
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical
                };
                hostPanel.Controls.Add(inputTextBox);
            }

            hostPanel.AutoScroll = true;  // Enable scrolling if content overflows
        }

        private void ButtonClickHandler(string action)
        {
            string inputValue = inputTextBox != null ? inputTextBox.Text : string.Empty;
            onActionSelected(action, inputValue);
            if (inputTextBox != null)
            {
                inputTextBox.Text = "";  // Clear input field after action
            }
        }
    }

    class VersionInfo
    {

        Panel hostPanel;
        public VersionInfo(Panel hostPanel)
        {
            this.hostPanel = hostPanel;
        }

        public void Render(IntegrationVersion version)
        {
            // Clear previous contents
            hostPanel.Controls.Clear();

            // Define base coordinates and offsets
            int startX = 10;
            int startY = 10;
            int verticalSpacing = 25;
            int columnWidth = (hostPanel.Width - 30) / 2;  // Two columns, assuming a margin

            // Create label controls for each piece of information
            Label nameLabel = new Label { Text = "Name: " + version.Id, AutoSize = true, Location = new Point(startX, startY) };
            Label statusLabel = new Label { Text = "Status: " + version.Status, AutoSize = true, Location = new Point(startX, startY + verticalSpacing) };
            Label reviewerLabel = new Label { Text = "Revizor: " + version.Reviewer, AutoSize = true, Location = new Point(startX, startY + 2 * verticalSpacing) };
            Label approvedDateLabel = new Label { Text = "Last change approved date: " + version.ModifiedDate, AutoSize = true, Location = new Point(startX + columnWidth, startY) };
            Label requesteeLabel = new Label { Text = "Last change requested by: " + version.Requestee, AutoSize = true, Location = new Point(startX + columnWidth, startY + verticalSpacing) };
            Label requestedDateLabel = new Label { Text = "Last change requested date: " + version.CreatedDate, AutoSize = true, Location = new Point(startX + columnWidth, startY + 2 * verticalSpacing) };

            // Add labels to the panel
            hostPanel.Controls.Add(nameLabel);
            hostPanel.Controls.Add(statusLabel);
            hostPanel.Controls.Add(reviewerLabel);
            hostPanel.Controls.Add(approvedDateLabel);
            hostPanel.Controls.Add(requesteeLabel);
            hostPanel.Controls.Add(requestedDateLabel);
        }
    }

    class ConfigRenderer
    {
        private Panel hostPanel;
        private CurrentValue currentValue;
        private string original;
        private CurrentValueControl currentValueControl;
        private Point p = new Point(0, 0);

        public const int HEIGHT_DISPLACEMENT = 30;


        int elementWidth;
        public ConfigRenderer(Panel hostPanel, int elementWidth)
        {
            this.hostPanel = hostPanel;
            this.elementWidth = elementWidth;
        }

        public void Render(CurrentValue root)
        {
            hostPanel.Controls.Clear();
            this.currentValue = root;
            original = JsonConvert.SerializeObject(root);
            Rerender();
        }

        public CurrentValue CollectCurrentValue()
        {
            return currentValueControl.CurrentValue;
        }

        private void Rerender()
        {
            p = hostPanel.AutoScrollPosition;
            hostPanel.Controls.Clear();

            if (currentValueControl != null)
            {
                currentValue = currentValueControl.CurrentValue;
            }

            var memos = currentValue.implementationOptions.ToList();
            var vars = currentValue.dependencies.Where(x => x.type != "EntryObject`1").ToArray();
            var dememo = new VariableDememoizerDecorator(new Dememoizer(memos), vars);

            currentValueControl = new RecursiveRenderer(elementWidth, HEIGHT_DISPLACEMENT, () =>
            {
                Rerender();
                return 0;
            }, 0, dememo).recurse(currentValue);

            hostPanel.AutoScroll = true;
            hostPanel.Controls.Add(currentValueControl);
            p.Y = -p.Y;
            hostPanel.AutoScrollPosition = p;
        }
    }

    public class HistoryComponent
    {
        private Panel hostPanel;
        private List<ConfigModificationEvent> integrationConfigModificationEvents;

        public HistoryComponent(Panel hostPanel, List<ConfigModificationEvent> integrationConfigModificationEvents)
        {
            this.hostPanel = hostPanel;
            this.integrationConfigModificationEvents = integrationConfigModificationEvents;
        }

        public void Render()
        {
            hostPanel.Controls.Clear();
            hostPanel.AutoScroll = true;  // Enable scrolling for overflow content

            // Create a title label
            Label titleLabel = new Label
            {
                Text = "History",
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            hostPanel.Controls.Add(titleLabel);

            int yPos = 40;  // Initial y position for the first history element

            // Iterate through each event and create UI components
            foreach (var eventInfo in integrationConfigModificationEvents)
            {
                Panel historyElement = new Panel
                {
                    Size = new Size(hostPanel.Width - 20, 110),
                    Location = new Point(10, yPos),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // First row with BuildIntegrationAsync ID and Version ID
                Label integrationIdLabel = new Label { Text = $"Integration ID: {eventInfo.IntegrationId}", AutoSize = true, Location = new Point(10, 10) };
                Label versionIdLabel = new Label { Text = $"Version ID: {eventInfo.VersionId}", AutoSize = true, Location = new Point(200, 10) };
                historyElement.Controls.Add(integrationIdLabel);
                historyElement.Controls.Add(versionIdLabel);

                // Second row with Event Type and Comment
                Label eventTypeLabel = new Label { Text = $"Event Type: {eventInfo.EventType}", AutoSize = true, Location = new Point(10, 35) };
                Label commentLabel = new Label { Text = $"Comment: {eventInfo.Comment}", AutoSize = true, Location = new Point(200, 35) };
                historyElement.Controls.Add(eventTypeLabel);
                historyElement.Controls.Add(commentLabel);

                // Created Date
                Label createdDateLabel = new Label { Text = $"Created Date: {eventInfo.CreatedDate}", AutoSize = true, Location = new Point(10, 60) };
                historyElement.Controls.Add(createdDateLabel);

                hostPanel.Controls.Add(historyElement);
                yPos += 120;  // Increment y position for the next history element
            }
        }
    }


    public class ZoomablePanel : Panel
    {
        private float zoomFactor = 1.0f;
        private const float zoomStep = 0.1f;
        private const float minZoom = 0.1f;
        private const float maxZoom = 5.0f;

        public ZoomablePanel()
        {
            this.DoubleBuffered = true;
            this.Paint += new PaintEventHandler(this.ZoomablePanel_Paint);
        }

        public void ZoomIn()
        {
            zoomFactor = Math.Min(maxZoom, zoomFactor + zoomStep);
            this.Invalidate(); // Trigger a redraw of the panel
        }

        public void ZoomOut()
        {
            zoomFactor = Math.Max(minZoom, zoomFactor - zoomStep);
            this.Invalidate(); // Trigger a redraw of the panel
        }

        private void ZoomablePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            e.Graphics.ScaleTransform(zoomFactor, zoomFactor);
            foreach (Control control in this.Controls)
            {
                control.Invalidate();
            }
        }
    }




}
