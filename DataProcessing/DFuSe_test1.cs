using SharpCubeProgrammer;
using SharpCubeProgrammer.Enum;
using SharpCubeProgrammer.Struct;

namespace DFuSe_test1
{
    internal class DFuSe_test1
    {
        static void Main()
        {
            DFuSe_test1 UUT = new DFuSe_test1();
            UUT.FlashSTM32();
        }
        
        public void FlashSTM32()
        {
            //Instantiate the API
            var cubeProgrammerApi = new SharpCubeProgrammer.CubeProgrammerApi();

            //Set up Callbacks (Optional: shows progress in your UI)
            var callbacks = new DisplayCallBacks
            {
                LogMessage = (msg, type) => Console.WriteLine($"LOG: {msg}"),
                LoadBar = (current, total) => Console.WriteLine($"Progress: {current}/{total}")
            };
            cubeProgrammerApi.SetDisplayCallbacks(callbacks);

            try
            {
                //Find USB-C connection
                var usbList = cubeProgrammerApi.GetDfuDeviceList();

                Console.WriteLine(usbList.First());
                if (usbList.Count() == 0)
                {
                    Console.WriteLine("No STM32 device found in DFU mode. Is BOOT0 high?");
                    return;
                }

                //Connect to the first found device
                else
                {
                    var DFULink = usbList.First();
                    var status = cubeProgrammerApi.ConnectDfuBootloader;

                    if (status.Equals(CubeProgrammerError.CubeprogrammerNoError))
                    {
                        var generalInfo = cubeProgrammerApi.GetDeviceGeneralInf();
                        if (generalInfo != null)
                        {
                            Console.WriteLine("INFO: \n" +
                                                "Board: {0} \n" +
                                                "Bootloader Version: {1} \n" +
                                                "Cpu: {2} \n" +
                                                "Description: {3} \n" +
                                                "DeviceId: {4} \n" +
                                                "FlashSize: {5} \n" +
                                                "RevisionId: {6} \n" +
                                                "Name: {7} \n" +
                                                "Series: {8} \n" +
                                                "Type: {9}",
                                generalInfo.Value.Board,
                                generalInfo.Value.BootloaderVersion,
                                generalInfo.Value.Cpu,
                                generalInfo.Value.Description,
                                generalInfo.Value.DeviceId,
                                generalInfo.Value.FlashSize,
                                generalInfo.Value.RevisionId,
                                generalInfo.Value.Name,
                                generalInfo.Value.Series,
                                generalInfo.Value.Type);
                        }
                    }
                }

            }

            finally
            {
                Console.WriteLine("Matteo stinky");
            }
        }
    
    }
}
