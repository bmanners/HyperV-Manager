using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;

namespace Hyper_V_Manager
{
	internal class VMManager
	{
		public List<VM> VMs { get; set; }

		public void UpdateList()
		{
			VMs = GetVMs();
		}

		private List<VM> GetVMs()
		{
			var vms = new List<VM>();

			var path = ConfigurationSettings.AppSettings["root"];
			var manScope = new ManagementScope(path);
			var queryObj = new ObjectQuery("Select * From Msvm_ComputerSystem where Caption=\"Virtual Machine\"");

			var vmSearcher = new ManagementObjectSearcher(manScope, queryObj);
			var vmCollection = vmSearcher.Get();

			foreach (ManagementObject vm in vmCollection)
			{
				vms.Add(new VM(vm));
			}

			return vms;
		}

		public List<ManagementObject>  GetManagementObjects()
		{
			return VMs.Select(vm => vm._managementObject).ToList();
		}
	}
}
