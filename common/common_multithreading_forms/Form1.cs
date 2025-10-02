using common_multithreading_tests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace common_multithreading_forms
{

    public partial class Form1 : Form
    {
        private SimplifiedMultithreaded simplifiedMultithreaded;
        private List<Button> taskButtons = new List<Button>();
        private FlowLayoutPanel flowLayoutPanelTasks;
        private int[] taskEnterCounts;
        private int[] taskWaitCounts;
        private const int MaxConcurrentTasks = 2; // Example max concurrent tasks
        private int tasksInCriticalSection;

        public Form1()
        {
            this.Load += new System.EventHandler(this.Form1_Load);
            InitializeComponent();
            InitializeFlowLayoutPanel();
            InitializeTaskButtons();
        }

        private void InitializeFlowLayoutPanel()
        {
            flowLayoutPanelTasks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            Controls.Add(flowLayoutPanelTasks);
        }

        private void InitializeTaskButtons()
        {
            int N = 40;

            var random = new Random();
            List<Func<Task>> tasks = new List<Func<Task>>();
            taskEnterCounts = new int[N];
            taskWaitCounts = new int[N];
            cnts = new Some[N];
            for (int i = 0; i < cnts.Length; i++)
                cnts[i] = new Some();

            for (int i = 0; i < N; i++)
            {
                int taskIndex = i;
                Button taskButton = new Button
                {
                    Text = $"Task {taskIndex + 1}: 0",
                    BackColor = Color.Gray,
                    Width = 100,
                    Height = 50,
                    Margin = new Padding(10)
                };
                taskButtons.Add(taskButton);
                flowLayoutPanelTasks.Controls.Add(taskButton);

                tasks.Add(async () =>
                {
                    await Task.Delay(random.Next(1000, 5000)); // Simulate varying task durations
                });
            }

            simplifiedMultithreaded = new SimplifiedMultithreaded(tasks, UpdateTaskStatus, true);
        }


        class Some { public string status; public int count = 0; }
        Some[] cnts = null;
        object lck = new object();
        private void UpdateTaskStatus(int taskIndex, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateTaskStatus(taskIndex, status)));
            }
            else
            {
                var button = taskButtons[taskIndex];
                switch (status)
                {
                    case "entering":
                        button.BackColor = Color.Green;
                        button.Text = $"";
                        lock (lck) {
                            for (int i = 0; i < cnts.Length; i++)
                            {
                                if (cnts[i].status == "waiting")
                                    cnts[i].count++;
                            }
                        }
                        break;
                    case "waiting":
                        button.BackColor = Color.Orange;
                        lock (lck) {
                            cnts[taskIndex].status = "waiting";
                        }
                        button.Text = $"Task {taskIndex + 1} waiting {cnts[taskIndex].count} times";
                        break;
                    case "completed":
                        lock (lck) {
                            cnts[taskIndex].count = 0;
                        }
                        button.BackColor = Color.Gray;
                        break;
                }
            }
        }

        private void Log(string message)
        {
            // Log the message (optional)
            Console.WriteLine(message);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await simplifiedMultithreaded.Run();
        }
    }



}
