using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Management;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Hyper_V_Manager
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		/*
         * 
         * Unknown      - 0     - state could not be determined
         * Enabled      - 2     - VM is running
         * Disabled     - 3     - VM is stopped
         * Paused       - 32768 - VM is paused
         * Suspended    - 32769 - VM is in a saved state
         * Starting     - 32770 - VM is starting
         * Saving       - 32773 - VM is saving its state
         * Stopping     - 32774 - VM is turning off
         * Pausing      - 32776 - VM is pausing
         * Resuming     - 32777 - VM is resuming from a paused state
         * 
         */


		private string vmBalloonState = string.Empty;
		private readonly Timer t = new Timer();
		private readonly Dictionary<string, string> changingVMs = new Dictionary<string, string>();

		private VMManager vmManager;

		private void Form1_Load(object sender, EventArgs e)
		{
			t.Elapsed += t_Elapsed;
			t.Interval = 4500;

			vmManager = new VMManager();
			vmManager.UpdateList();

			BuildContextMenu();
		}

		private void t_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!contextMenuStrip1.Visible)
				UpdateBalloontip();
		}


		private void UpdateBalloontip()
		{
			var localVMs = vmManager.GetManagementObjects(); // GetVMs();

			foreach (var vm in localVMs)
			{
				if (changingVMs.ContainsKey(vm["ElementName"].ToString()))
				{
					var vmName = vm["ElementName"].ToString();
					var currentBalloonState = changingVMs[vm["ElementName"].ToString()];
					var initvmBalloonState = changingVMs[vm["ElementName"].ToString()];

					switch (Convert.ToInt32(vm["EnabledState"]))
					{
						case 2:
							currentBalloonState = "Running";
							break;
						case 3:
							currentBalloonState = "Stopped";
							break;
						case 32768:
							currentBalloonState = "Paused";
							break;
						case 32769:
							currentBalloonState = "Saved";
							break;
						case 32770:
							currentBalloonState = "Starting";
							break;
						case 32773:
							currentBalloonState = "Saving";
							break;
						case 32774:
							currentBalloonState = "Stopping";
							break;
						default:
							currentBalloonState = "Unknown";
							break;
					}

					if (initvmBalloonState != currentBalloonState)
					{
						notifyIcon1.ShowBalloonTip(4000, "VM State Changed", vm["ElementName"] + " " + currentBalloonState,
							ToolTipIcon.Info);
						changingVMs[vm["ElementName"].ToString()] = currentBalloonState;
					}
					else if (currentBalloonState == "Running" || currentBalloonState == "Stopped" || currentBalloonState == "Paused" ||
					         currentBalloonState == "Saved")
						changingVMs.Remove(vm["ElementName"].ToString());
					else if (changingVMs.Count <= 0)
						t.Enabled = false;
				}
			}
		}


		//private List<ManagementObject> GetVMs()
		//{
		//	var vms = new List<ManagementObject>();

		//	var path = ConfigurationSettings.AppSettings["root"];
		//	var manScope = new ManagementScope(path);
		//	var queryObj = new ObjectQuery("Select * From Msvm_ComputerSystem where Caption=\"Virtual Machine\"");
		//		// added where clause to only get VM's

		//	var vmSearcher = new ManagementObjectSearcher(manScope, queryObj);
		//	var vmCollection = vmSearcher.Get();

		//	foreach (ManagementObject vm in vmCollection)
		//	{
		//		//if (vm["AssignedNumaNodeList"] != null)  // remove if as no longer needed.
		//		//{
		//		vms.Add(vm);
		//		//}
		//	}

		//	return vms;
		//}

		/*
         * Context menu events
         */

		#region ContextMenuEvents

		private void pauseItem_Click(object sender, EventArgs e)
		{
			ChangeVMState("Pause", ((ToolStripMenuItem) sender).OwnerItem.Name);
		}

		private void saveStateItem_Click(object sender, EventArgs e)
		{
			ChangeVMState("Save State", ((ToolStripMenuItem) sender).OwnerItem.Name);
		}

		private void stopItem_Click(object sender, EventArgs e)
		{
			ChangeVMState("Stop", ((ToolStripMenuItem) sender).OwnerItem.Name);
		}

		private void startItem_Click(object sender, EventArgs e)
		{
			ChangeVMState("Start", ((ToolStripMenuItem) sender).OwnerItem.Name);
		}

		private void exitItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		/*
         * Change the state of the VM based on the state passed in and the VM Name
         */

		private void ChangeVMState(string vmState, string vmName)
		{
			var localVMs = vmManager.GetManagementObjects(); // GetVMs();

			t.Enabled = true;

			foreach (var vm in localVMs)
			{
				if (vm["ElementName"].ToString() == vmName)
				{
					changingVMs.Add(vm["ElementName"].ToString(), "Unknown");

					var inParams = vm.GetMethodParameters("RequestStateChange");

					if (vmState == "Start")
						inParams["RequestedState"] = 2;

					if (vmState == "Stop")
						inParams["RequestedState"] = 3;

					if (vmState == "Pause")
						inParams["RequestedState"] = 32768;

					if (vmState == "Save State")
						inParams["RequestedState"] = 32769;

					vm.InvokeMethod("RequestStateChange", inParams, null);
				}
			}
		}


		/*
         * Build out the context menu items.
         */


		private void BuildContextMenu()
		{
			//Re-get the VM's from Hyper-V
			var localVMs = vmManager.GetManagementObjects();  // GetVMs();

			var vmState = string.Empty;

			//Clear out the menu items
			contextMenuStrip1.Items.Clear();

			//loop through the VMs and rebuild out the context menus
			//Do this to ensure the proper status is displayed and the proper action items are enabled
			foreach (var vm in localVMs)
			{
				vmState = string.Empty;

				var startItem = new ToolStripMenuItem("Start");
				startItem.Click += startItem_Click;
				startItem.DisplayStyle = ToolStripItemDisplayStyle.Text;

				var stopItem = new ToolStripMenuItem("Stop");
				stopItem.Click += stopItem_Click;
				stopItem.DisplayStyle = ToolStripItemDisplayStyle.Text;

				var saveStateItem = new ToolStripMenuItem("Save State");
				saveStateItem.Click += saveStateItem_Click;
				saveStateItem.DisplayStyle = ToolStripItemDisplayStyle.Text;

				var pauseItem = new ToolStripMenuItem("Pause");
				pauseItem.Click += pauseItem_Click;
				pauseItem.DisplayStyle = ToolStripItemDisplayStyle.Text;

				switch (Convert.ToInt32(vm["EnabledState"]))
				{
					case 2:
						vmState = "Running";
						startItem.Enabled = false;
						break;
					case 3:
						vmState = "Stopped";
						stopItem.Enabled = false;
						saveStateItem.Enabled = false;
						pauseItem.Enabled = false;
						break;
					case 32768:
						vmState = "Paused";
						pauseItem.Enabled = false;
						break;
					case 32769:
						vmState = "Saved";
						saveStateItem.Enabled = false;
						stopItem.Enabled = false;
						pauseItem.Enabled = false;
						break;
					case 32770:
						vmState = "Starting";
						break;
					case 32773:
						vmState = "Saving";
						break;
					case 32774:
						vmState = "Stopping";
						break;
					default:
						vmState = "Unknown";
						break;
				}

				var vmStatusText = vm["ElementName"] + " " + vmState;

				var vmItem = new ToolStripMenuItem(vmStatusText);

				vmItem.Name = vm["ElementName"].ToString();

				if (vmState == "Running" || vmState == "Stopped" || vmState == "Saved" || vmState == "Paused")
				{
					vmItem.DropDownItems.Add(startItem);
					vmItem.DropDownItems.Add(stopItem);
					vmItem.DropDownItems.Add(saveStateItem);
					vmItem.DropDownItems.Add(pauseItem);
				}


				contextMenuStrip1.Items.Add(vmItem);
			}

			var exitItem = new ToolStripMenuItem("Exit");
			exitItem.Click += exitItem_Click;
			contextMenuStrip1.Items.Add(exitItem);
			contextMenuStrip1.Refresh();
		}

		/*
         * When the context menu is opened, rebuild the Context Menu Items.  This ensures the status and items are up to date
         */

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			BuildContextMenu();
		}

		/*
         *  When the form is activated, hide it so the only UI is the system tray icon.
         */

		private void Form1_Activated(object sender, EventArgs e)
		{
			Hide();
		}
	}
}
