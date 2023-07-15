# Arduino Camera Image Collector

An app to read in JPEG image data from the provided serial port and baud rate, and to write them into the same directory as this executable. Images will be stored in a folder with the format MM_dd_YY, and image names will be of the form HH_mm_ss.

# Notes
1. The port, whether default or user-specified, needs to be opened for reading prior to starting this program, e.g. run 'sudo chmod a+r <port-name>
2. This application is intended to be used with the Arduino Camera, running the sketch located @ . The baud rate for the Xbee radio is set at 38400.

# Usage
 ArduinoCameraImageCollector <Serial_Port> <Baud_Rate>   : run program with provided serial port.
 ArduinoCameraImageCollector                             : run program with serial port /dev/ttyACM0 and a baud rate of 921600
