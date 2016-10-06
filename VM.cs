using System;
using System.Management;

namespace Hyper_V_Manager
{
	public enum RequestedState
	{
		Start = 2,
		Stop = 3,
		Pause = 32768,
		SaveState = 32769
	}

	internal class VM
	{
		public ManagementObject _managementObject;

		public VM(ManagementObject managementObject)
		{
			_managementObject = managementObject;
			
		}

		public string Name
		{
			get { return _managementObject["ElementName"].ToString(); }
		}

		public string Status
		{
			get
			{
				var test = _managementObject["State"].ToString();
				return ConvertState(Convert.ToInt32(_managementObject["State"]));
			}
		}


		public void ChangeState(RequestedState requestedState)
		{
			var inParams = _managementObject.GetMethodParameters("RequestStateChange");

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