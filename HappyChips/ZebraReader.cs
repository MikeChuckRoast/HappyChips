using HappyChips.Models;
using Symbol.RFID3;

namespace HappyChips
{
    internal class ZebraReader : IGenericRfidReader
    {

        internal RFIDReader reader;

        private DelegateChipRead _delegateChipRead;


        public ZebraReader(string readerHostname, DelegateChipRead delegateChipRead, out string errorMessage)
        {
            this._delegateChipRead = delegateChipRead;
            reader = new RFIDReader(readerHostname, 5084, 0);
            reader.Connect();
            errorMessage = "";
        }

        ~ZebraReader()
        {
            reader.Disconnect();
        }

        public (bool, string) StartReader(bool setTransmitPower = false, int transmitPower = 30)
        {
            // Set transmit power
            if (setTransmitPower)
            {
                SetTransmitPower(transmitPower);
            }
            else
            {
                // Set to max power
                SetTransmitPower(-1);
            }

            // Define read notify handler
            Events.ReadNotifyHandler readNotifyHandler = new Events.ReadNotifyHandler(OnChipRead);
            reader.Events.ReadNotify += readNotifyHandler;

            try
            {
                reader.Actions.Inventory.Perform();
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string) StopReader()
        {
            try
            {
                reader.Actions.Inventory.Stop();
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private void OnChipRead(object sender, Events.ReadEventArgs readEventArgs)
        {
            var tagData = readEventArgs.ReadEventData.TagData;
            var chipId = tagData.TagID;
            var antennaId = tagData.AntennaID;
            var lastRead = tagData.SeenTime.UTCTime.FirstSeenTimeStamp;
            var tagSeenCount = tagData.TagSeenCount;
            // Create a new ChipReadDetail object
            var chipReadDetail = new ChipReadDetail
            {
                ChipId = chipId,
                LastRead = lastRead,
                TagSeenCount = tagSeenCount,
                AntennaId = antennaId
            };

            // Call the delegate
            _delegateChipRead(chipReadDetail);

        }

        private void SetTransmitPower(int transmitPower)
        {
            // Get power table from reader capability
            int[] powerTable = reader.ReaderCapabilities.TransmitPowerLevelValues;

            // Get current antenna config
            ushort[] antID = reader.Config.Antennas.AvailableAntennas;
            Antennas.Config antConfig = reader.Config.Antennas[antID[0]].GetConfig();

            // Set transmit power
            antConfig.TransmitPowerIndex = (ushort)(powerTable.Length - 1);
            if (transmitPower > -1)
            {
                if ((transmitPower * 100) < powerTable[0])
                {
                    antConfig.TransmitPowerIndex = 0;
                }
                else
                {
                    for (int iPower = 0; iPower < powerTable.Length; iPower++)
                    {
                        if (powerTable[iPower] >= (transmitPower * 100))
                        {
                            antConfig.TransmitPowerIndex = (ushort)iPower;
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < antID.Length; i++)
            {
                reader.Config.Antennas[antID[i]].SetConfig(antConfig);
            }
        }
    }
}
