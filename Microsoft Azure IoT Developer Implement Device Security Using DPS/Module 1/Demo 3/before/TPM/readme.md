#TPM Attestation



## How to run the sample

1. Run the sample from the command line to retrieve the Endorsment Key: Dotnet run --GetEndorsementKey
1. Create a new Individual enrollment in DPS using the endorsement key
1. Run the solution as an Administrator
1. Update the solution with global endpoint, Id scope and Registration ID
1. Run the solution as an Administrator


## Notes

To make things easier, this sample uses a TPM simulator (SecurityProviderTpmSimulator).  Sadly this does is not supported on Linux or OSX.

To run against a real TPM2.0 device, replace this with `SecurityProviderTpmHsm`.
