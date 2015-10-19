# Make
Integration project for Windows 10, Cortana, and Windows IoT on the Raspbarry Pi 2 Model B.

The desktop app has a commands file that references raspberry pi 2 pins and states of those pins.
Using Cortana, you can say "Make LED Pin 5 turn on".  Cortana will pass the command to the desktop app, which
parses the command and sends it to the Raspberry Pi app.  The Raspberry Pi app will change the state of the pin
based on the incomming command.  The transfer of data is done using Microsoft's StreamSocket and StreamSocketListener.
