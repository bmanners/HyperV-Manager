using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace Hyper_V_Manager
{

		public enum RequestedState {Start = 2,Stop = 3, Pause = 32768, SaveState = 32769 };

	class VM
	{
		public String Name { get { return _managementObject["ElementName"].ToString(); }  }
		public String Status {get { return ConvertState(Convert.ToInt32(_managementObject["State"])); }}
		public ManagementObject _managementObject;

		public VM(ManagementObject managementObject)
		{
			_managementObject = managementObject;
		}



		public void ChangeState(RequestedState requestedState)
		{
			ManagementBaseObject inParams = _managementObject.GetMethodParameters("RequestStateChange");

			inParams["RequestedState"] = requestedState;

			_managementObject.InvokeMethod("RequestStateChange",
				inParams,
				null);
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
