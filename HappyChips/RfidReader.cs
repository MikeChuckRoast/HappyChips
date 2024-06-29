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

        ~RfidReader()
        {
            // Safely attempt to close and dispose of _client
            if (_client != null)
            {
                try
                {
                    if (_client.IsConnected)
                    {
                        _client.Close();
                    }
                    _client.Dispose();
                }
                catch
                {
                    // Optionally log the error or handle it as needed
                }
            }
        }

        public (bool, string) ConfigureReader(delegateRoAccessReport reportDelegate, bool setTransmitPower = false, ushort transmitPower = 200)
        {
            var (deleteSuccess, deleteMessage) = DeleteActiveROSpecs();

            // Set transmit power
            if (setTransmitPower)
            {
                var (success, message) = SetTransmitPower(transmitPower);
                if (!success)
                {
                    return (false, "Error setting transmit power: " + message);
                }
            }

            // Define report delegate
            _client.OnRoAccessReportReceived += reportDelegate;

            // Add ROSpec
            (MSG_ADD_ROSPEC_RESPONSE addResponse, MSG_ERROR_MESSAGE errorMessage) = Add_RoSpec();
            var (addSuccess, addMessage) = CheckResponse(addResponse?.LLRPStatus, errorMessage);
            if (!addSuccess)
            {
                return (false, "Error adding RO spec: " + addMessage);
            }

            // Enable ROSpec
            (MSG_ENABLE_ROSPEC_RESPONSE enableResponse, errorMessage) = Enable_RoSpec();
            var (enableSuccess, enableMessage) = CheckResponse(enableResponse?.LLRPStatus, errorMessage);
            if (!enableSuccess)
            {
                return (false, "Error enabling RO spec: " + enableMessage);
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

            var (success, message) = CheckResponse(rsp?.LLRPStatus, msg_err);

            if (success)
            {
                return (true, "Reader Stopped");
            }
            else
            {
                return (false, "Error stopping reader: " + message);
            }

        }

        private (bool, string) CheckResponse(PARAM_LLRPStatus? response, MSG_ERROR_MESSAGE errorMessage)
        {
            if (response != null)
            {
                if (response.StatusCode != ENUM_StatusCode.M_Success)
                {
                    // Error
                    return (false, response.ErrorDescription);
                }
                else
                {
                    // Success
                    return (true, "Success");
                }
            }
            else if (errorMessage != null)
            {
                // Error
                return (false, errorMessage.ToString());
            }
            else
            {
                // Timeout
                return (false, "Timeout");
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

        private (bool, string) DeleteActiveROSpecs()
        {
            MSG_GET_ROSPECS getROSpecs = new MSG_GET_ROSPECS();
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_ROSPECS_RESPONSE response = _client.GET_ROSPECS(getROSpecs, out msg_err, _timeout);
            var (getSuccess, getMessage) = CheckResponse(response?.LLRPStatus, msg_err);

            if (getSuccess)
            {
                if (response != null && response.ROSpec != null)
                {
                    foreach (var rospec in response.ROSpec)
                    {
                        (var deleteResponse, msg_err) = Delete_RoSpec(rospec.ROSpecID);
                        var (deleteSuccess, deleteMessage) = CheckResponse(deleteResponse?.LLRPStatus, msg_err);
                        if (!deleteSuccess)
                        {
                            return (false, deleteMessage);
                        }
                    }
                }
            } 
            else
            {
                return (false, "Error getting ROSpecs: " + getMessage);
            }
            return (true, "Success");
        }

        private void Get_Reader_Config()
        {
            MSG_GET_READER_CONFIG msg = new MSG_GET_READER_CONFIG();
            MSG_ERROR_MESSAGE msg_err;
            msg.AntennaID = 0;
            msg.GPIPortNum = 0;
            MSG_GET_READER_CONFIG_RESPONSE rsp = _client.GET_READER_CONFIG(msg, out msg_err, 3000);

            //rsp.
            if (rsp != null)
            {
                rsp.ToString();
            }

        }

        private void Get_Reader_Capability()
        {
            MSG_GET_READER_CAPABILITIES msg = new MSG_GET_READER_CAPABILITIES();
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CAPABILITIES_RESPONSE rsp = _client.GET_READER_CAPABILITIES(msg, out msg_err, 3000);
            if (rsp != null)
            {
                rsp.ToString();
            }
        }

        private (bool, string) SetTransmitPower(ushort transmitPower)
        {
            MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();
            MSG_ERROR_MESSAGE msg_err;
            msg.AccessReportSpec = new PARAM_AccessReportSpec();
            msg.AccessReportSpec.AccessReportTrigger = ENUM_AccessReportTriggerType.End_Of_AccessSpec;

            msg.AntennaConfiguration = new PARAM_AntennaConfiguration[1];
            msg.AntennaConfiguration[0] = new PARAM_AntennaConfiguration();
            msg.AntennaConfiguration[0].AntennaID = 0;
            msg.AntennaConfiguration[0].RFTransmitter = new PARAM_RFTransmitter();
            msg.AntennaConfiguration[0].RFTransmitter.ChannelIndex = 1;
            msg.AntennaConfiguration[0].RFTransmitter.HopTableID = 1;
            msg.AntennaConfiguration[0].RFTransmitter.TransmitPower = transmitPower;

            MSG_SET_READER_CONFIG_RESPONSE rsp = _client.SET_READER_CONFIG(msg, out msg_err, _timeout);

            var (success, message) = CheckResponse(rsp?.LLRPStatus, msg_err);
            if (!success)
            {
                return (false, "Error setting transmit power: " + message);
            }
            return (true, $"Power set to {transmitPower}");

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
