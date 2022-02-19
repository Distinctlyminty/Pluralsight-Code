using CommandLine;
using Microsoft.Azure.Devices;

namespace TPM
{
    internal class Parameters
    {
        [Option(
            'e',
            "GetTpmEndorsementKey",
            HelpText = "Gets the TPM endorsement key. Use this option by itself to get the EK needed to create a DPS individual enrollment.", Default = false)]
        public bool GetTpmEndorsementKey { get; set; }

    }
}