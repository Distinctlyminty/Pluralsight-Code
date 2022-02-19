# Demo 1 - Creating X.509 Certificates in Azure Cloud Shell
The script files are available from this URL:
[https://github.com/Azure/azure-iot-sdk-c/blob/master/tools/CACertificates/CACertificateOverview.md]

## ensure the current directory is the user's home directory
 cd ~

## make a directory named "certificates"
 mkdir certificates

## change directory to the "certificates" directory
 cd certificates

## Download the helper scripts
 curl https://raw.githubusercontent.com/Azure/azure-iot-sdk-c/master/tools/CACertificates/certGen.sh --output certGen.sh
 curl https://raw.githubusercontent.com/Azure/azure-iot-sdk-c/master/tools/CACertificates/openssl_device_intermediate_ca.cnf --output openssl_device_intermediate_ca.cnf
 curl https://raw.githubusercontent.com/Azure/azure-iot-sdk-c/master/tools/CACertificates/openssl_root_ca.cnf --output openssl_root_ca.cnf

## update script permissions so user can read, write, and execute it
 chmod 700 certGen.sh

##  Create root and intermediate certificates
./certGen.sh create_root_and_intermediate_

## Download certificates
download ~/certificates/certs/azure-iot-test-only.root.ca.cert.pem

download ~/certificates/certs/azure-iot-test-only.intermediate.cert.pem

## Create a device certificate
./certGen.sh create_device_certificate_from_intermediate <Device ID>


