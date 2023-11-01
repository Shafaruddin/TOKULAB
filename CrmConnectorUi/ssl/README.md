# Generate a Trusted SSL Certificate

This folder contains a script that will generate a trusted ssl certificate which can be used for local software development.

cd to this directory and run the following on git console or linux terminal
```
bash generate.sh
```

## Configuration

You can adjust the `[dn]` part of the `openssl-custom.cnf` file to whatever you prefer.

```
[dn]
C = <COUNTRY>
ST = <STATE>
L = <LOCALITY / CITY>
O = <ORGANIZATION>
OU = <ORGANIZATION_UNIT>
emailAddress = <EMAIL_ADDRESS>
CN = <HOSTNAME / IP_ADDRESS>
```

## Installing the generated certificate
Double click on the certificate (server.crt)
Click on the button “Install Certificate …”
Select whether you want to store it on user level or on machine level
Click “Next”
Select “Place all certificates in the following store”
Click “Browse”
Select “Trusted Root Certification Authorities”
Click “Ok”
Click “Next”
Click “Finish”
If you get a prompt, click “Yes”

## Enable self signed certificates for localhost on Chrome
open chrome, go to this url chrome://flags/#allow-insecure-localhost and enable it and relaunch chrome

## Launch the angular app with new certificate
ng serve --hmr --ssl true --ssl-key ssl\server.key --ssl-cert ssl\server.crt

Credits go to https://github.com/RubenVermeulen/generate-trusted-ssl-certificate