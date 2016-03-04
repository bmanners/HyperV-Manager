using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;
using System.Text;

namespace Hyper_V_Manager
{
	class VMManager
	{
		public List<VM> VMs { get; set; }

		public void UpdateList()
		{
			VMs = GetVMs();
		}

		private List<VM> GetVMs()
		{
			List<VM> vms = new List<VM>();

			string path = ConfigurationSettings.AppSettings["root"];
			ManagementScope manScope = new ManagementScope(path);
			ObjectQuery queryObj = new ObjectQuery("Select * From Msvm_ComputerSystem where Caption=\"Virtual Machine\"");
		
			ManagementObjectSearcher vmSearcher = new ManagementObjectSearcher(manScope, queryObj);
			ManagementObjectCollection vmCollection = vmSearcher.Get();

			foreach (ManagementObject vm in vmCollection)
			{
				vms.Add(new VM(vm));
		}

			return vms;
		}
	}
}
