using Org.LLRP.LTK.LLRPV1;
using DataType = Org.LLRP.LTK.LLRPV1.DataType;

namespace HappyChips
{
    internal class RfidReader
    {
        private LLRPClient _client;

        private int _timeout = 2000;

        private uint _roSpecId = 123;

        public RfidReader(string readerHostname, out ENUM_ConnectionAttemptStatusType status)
        {

            _client = new LLRPClient();
            _client.Open(readerHostname, _timeout, out status);

        }

        public (bool, string) ConfigureReader(delegateRoAccessReport reportDelegate)
        {
            DeleteActiveROSpecs();

            // Define report delegate
            _client.OnRoAccessReportReceived += reportDelegate;

            // Add ROSpec
            (DataType.Message response, MSG_ERROR_MESSAGE errorMessage) = Add_RoSpec();
            if (!CheckResponse(response, errorMessage))
            {
                return (false, "Error adding RO spec: " + errorMessage);
            }

            // Enable ROSpec
            (response, errorMessage) = Enable_RoSpec();
            if (!CheckResponse(response, errorMessage))
            {
                return (false, "Error enabling RO spec: " + errorMessage);
            }

            return (true, "Reader Configured");
        }

        public (bool, string) StopReading()
        {
            // Disable ROSpec
            MSG_DISABLE_ROSPEC msg = new MSG_DISABLE_ROSPEC();
            msg.ROSpecID = _roSpecId;
            MSG_ERROR_MESSAGE msg_err;
            MSG_DISABLE_ROSPEC_RESPONSE rsp = _client.DISABLE_ROSPEC(msg, out msg_err, _timeout);

            _client.Close();
            _client.Dispose();

            if (rsp != null && rsp.LLRPStatus.StatusCode == ENUM_StatusCode.M_Success)
            {
                return (true, "Reader Stopped");
            }
            else
            {
                return (false, "Error stopping reader: " + msg_err);
            }

        }

        private bool CheckResponse(DataType.Message response, MSG_ERROR_MESSAGE errorMessage)
        {
            if (response != null)
            {
                // Success
                Console.WriteLine(response.ToString());
                return true;
            }
            else if (errorMessage != null)
            {
                // Error
                Console.WriteLine(errorMessage.ToString());
                return false;
            }
            else
            {
                // Timeout
                Console.WriteLine("Timeout Error.");
                return false;
            }
        }

        private (MSG_DELETE_ROSPEC_RESPONSE, MSG_ERROR_MESSAGE) Delete_RoSpec(uint id)
        {
            MSG_DELETE_ROSPEC msg = new MSG_DELETE_ROSPEC();
            msg.ROSpecID = id;
            MSG_ERROR_MESSAGE msg_err;

            MSG_DELETE_ROSPEC_RESPONSE rsp = _client.DELETE_ROSPEC(msg, out msg_err, _timeout);

            return (rsp, msg_err);
        }

        private (MSG_ADD_ROSPEC_RESPONSE, MSG_ERROR_MESSAGE) Add_RoSpec()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ROSPEC msg = new MSG_ADD_ROSPEC();

            // Reader Operation Spec (ROSpec)
            msg.ROSpec = new PARAM_ROSpec();
            // ROSpec must be disabled by default
            msg.ROSpec.CurrentState = ENUM_ROSpecState.Disabled;
            // The ROSpec ID can be set to any number
            // You must use the same ID when enabling this ROSpec
            msg.ROSpec.ROSpecID = _roSpecId;

            // ROBoundarySpec
            // Specifies the start and stop triggers for the ROSpec
            msg.ROSpec.ROBoundarySpec = new PARAM_ROBoundarySpec();
            // Immediate start trigger
            // The reader will start reading tags as soon as the ROSpec is enabled
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger = new PARAM_ROSpecStartTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger.ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Immediate;

            // No stop trigger. Keep reading tags until the ROSpec is disabled.
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger = new PARAM_ROSpecStopTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger.ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null;

            // Antenna Inventory Spec (AISpec)
            // Specifies which antennas and protocol to use
            msg.ROSpec.SpecParameter = new UNION_SpecParameter();
            PARAM_AISpec aiSpec = new PARAM_AISpec();
            aiSpec.AntennaIDs = new DataType.UInt16Array();
            // Enable all antennas
            aiSpec.AntennaIDs.Add(0);
            // No AISpec stop trigger. It stops when the ROSpec stops.
            aiSpec.AISpecStopTrigger = new PARAM_AISpecStopTrigger();
            aiSpec.AISpecStopTrigger.AISpecStopTriggerType = ENUM_AISpecStopTriggerType.Null;
            aiSpec.InventoryParameterSpec = new PARAM_InventoryParameterSpec[1];
            aiSpec.InventoryParameterSpec[0] = new PARAM_InventoryParameterSpec();
            aiSpec.InventoryParameterSpec[0].InventoryParameterSpecID = 1234;
            aiSpec.InventoryParameterSpec[0].ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2;
            msg.ROSpec.SpecParameter.Add(aiSpec);

            // Report Spec
            msg.ROSpec.ROReportSpec = new PARAM_ROReportSpec();
            // Send a report for every tag read
            msg.ROSpec.ROReportSpec.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;
            msg.ROSpec.ROReportSpec.N = 1;
            msg.ROSpec.ROReportSpec.TagReportContentSelector = new PARAM_TagReportContentSelector();

            MSG_ADD_ROSPEC_RESPONSE rsp = _client.ADD_ROSPEC(msg, out msg_err, _timeout);

            return (rsp, msg_err);
        }

        private (MSG_ENABLE_ROSPEC_RESPONSE, MSG_ERROR_MESSAGE) Enable_RoSpec()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_ENABLE_ROSPEC msg = new MSG_ENABLE_ROSPEC();
            msg.ROSpecID = _roSpecId;
            MSG_ENABLE_ROSPEC_RESPONSE rsp = _client.ENABLE_ROSPEC(msg, out msg_err, _timeout);

            return (rsp, msg_err);

        }

        private void DeleteActiveROSpecs()
        {
            MSG_GET_ROSPECS getROSpecs = new MSG_GET_ROSPECS();
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_ROSPECS_RESPONSE response = _client.GET_ROSPECS(getROSpecs, out msg_err, _timeout);

            if (response != null && response.LLRPStatus.StatusCode == ENUM_StatusCode.M_Success && response.ROSpec != null)
            {
                foreach (var rospec in response.ROSpec)
                {
                    if (rospec.CurrentState == ENUM_ROSpecState.Active)
                    {
                        Delete_RoSpec(rospec.ROSpecID);
                    }
                }
            }
        }

        public static string ConnectionAttemptStatusEnumToString (ENUM_ConnectionAttemptStatusType status) {
            switch (status)
            {
                case ENUM_ConnectionAttemptStatusType.Success:
                    return "Success";
                case ENUM_ConnectionAttemptStatusType.Failed_A_Reader_Initiated_Connection_Already_Exists:
                    return "Failed: A Reader Initiated Connection Already Exists";
                case ENUM_ConnectionAttemptStatusType.Failed_A_Client_Initiated_Connection_Already_Exists:
                    return "Failed: A Client Initiated Connection Already Exists";
                case ENUM_ConnectionAttemptStatusType.Failed_Reason_Other_Than_A_Connection_Already_Exists:
                    return "Failed: Reason Other Than A Connection Already Exists";
                case ENUM_ConnectionAttemptStatusType.Another_Connection_Attempted:
                    return "Another Connection Attempted";
                default:
                    return "Unknown";
            }
        }

        public static string GetEpcHexString(UNION_EPCParameter epcParameter)
        {
            string epc;
            // Two possible types of EPC: 96-bit and 128-bit
            if (epcParameter[0].GetType() == typeof(PARAM_EPC_96))
            {
                epc = ((PARAM_EPC_96)(epcParameter[0])).EPC.ToHexString();
            }
            else
            {
                epc = ((PARAM_EPCData)(epcParameter[0])).EPC.ToHexString();
            }
            if (epcParameter.Count > 1)
            {
                epc = "Multiple?";
            }
            return epc;
        }
    }
}
