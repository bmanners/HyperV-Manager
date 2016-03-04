using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace Hyper_V_Manager
{
	class VM
	{
		public String Name { get { return _managementObject["ElementName"].ToString(); }  }
		public String Status {get { return ConvertState(Convert.ToInt32(_managementObject["State"])); }}
		private ManagementObject _managementObject;

		public VM(ManagementObject managementObject)
		{
			_managementObject = managementObject;
		}


		private string ConvertState(int statuscode)
		{
			string state;
			switch (statuscode)
			{
				case 2:
					state = "Running";
					break;
				case 3:
					state = "Stopped";
					break;
				case 32768:
					state = "Paused";
					break;
				case 32769:
					state = "Saved";
					break;
				case 32770:
					state = "Starting";
					break;
				case 32773:
					state = "Saving";
					break;
				case 32774:
					state = "Stopping";
					break;
				default:
					state = "Unknown";
					break;
			}
			return state;

		}
	}
}
