using System;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
namespace ArduinoCameraImageCollector
{
    class Program
    {
        public  static Queue<int> Queue = new Queue<int>();
        public static List<byte> OutToFile = new List<byte>();
        public static bool ListOccupied = false;
        public static bool ShouldExit = false;
        public static SerialPort comPort;
        const int ReadBufferIterationMax = 200;
        public static int CurrentReadBufferIterationCount = 0 ;
        // Main entry point. 
        static void Main(string[] args)
        {
            
            if(args.Length >= 1){
                if(args[0].Trim() == "--help" ){
                    Console.Out.WriteLine("Arduino Camera Listener");
                    Console.Out.WriteLine("An app to read in JPEG image data from the provided serial port, and to write them into the same directory as " +
                        "this executable. Images will be stored in a folder with the format MM_dd_YY, and image names will be of the form HH_mm_ss");
                    Console.Out.WriteLine("This application is intended to be used with the Arduino Camera, running the sketch located @ ");
                    Console.Out.WriteLine("no args: begin listening session on /dev/ttyACM0");
                    Console.Out.WriteLine("arg 1 : the port attached to the camera.");
                    Console.Out.WriteLine("arg 2: ");
                    Console.Out.WriteLine("Note: the port, whether default or user-specified, needs to be opened for reading prior to starting this program, e.g. run 'sudo chmod a+r <port-name>");
                    return;
                }else{
                    // the first arg should be port.
                    // open port.
                    //ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = "sudo chmod a+rw " + args[0].Trim(), }; 
                    //Process proc = new Process() { StartInfo = startInfo, };
                    //proc.Start();
                    Console.Out.WriteLine("Opening port " + args[0].Trim() + " ...");
                    comPort = new SerialPort(args[0].Trim(), 38400);
                    comPort.NewLine = "\r";
                    comPort.ReadTimeout = 5000;
                   // comPort.StopBits = StopBits.Two;
                }
            }else{
                // assume port is /dev/ttyUSB0
                //ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = "sudo chmod a+rw /dev/ttyACM0" , }; 
                //Process proc = new Process() { StartInfo = startInfo, };
                //proc.Start();
                Console.Out.WriteLine("Opening port /dev/ttyUSB1 ..." );
                comPort = new SerialPort("/dev/ttyUSB1", 921600);
                comPort.NewLine = "\r";
                comPort.ReadTimeout = 5000;
            }
            while(true){
                try{
                comPort.Open();
                break;
            }catch(Exception e){
                Console.Out.WriteLine("Opening port failed. Message: " + e.Message + " Will wait thirty seconds and try again.");
                Thread.Sleep(30000);
            }
            }
            
            
            byte[] writeArray = new byte[1];
            writeArray[0] = 122;
            comPort.DiscardInBuffer();
            Thread.Sleep(100);
            comPort.DiscardOutBuffer();
            Thread.Sleep(100);
            comPort.DataReceived += DataReceived_Handler;
            
            // Enter an infinite loop, just listening for data.
            Console.Out.WriteLine("Entering the listening loop. All images will be written to the directory of the executable." );
            while(true){

                
                while(!ShouldExit){
                    Thread.Sleep(250);
                }

                //Set ShouldExit back to false.
                ShouldExit = false;
                ListOccupied = false;
                OutToFile = new List<byte>();
                CurrentReadBufferIterationCount = 0;
            }
            
  
            //comPort.Close();
        


        }
        static public void DataReceived_Handler(object sender, SerialDataReceivedEventArgs e){
           try{
                    // take place on queue.
                int myInt = Queue.Count + 1;
                Queue.Enqueue(myInt);
                // Check if list is busy. If so, wait.
                // while(ListOccupied){
                //     Thread.Sleep(15);
                // }
                int waitCounter = 0;
                while(Queue.Peek() != myInt){
                    Thread.Sleep(20);
                    waitCounter++;
                    if(waitCounter > 1000){
                        //Flush queue and quit.
                        Console.Out.WriteLine("Quitting the queue.");
                        int result;
                        while(Queue.TryDequeue(out result)){
                            Queue.Dequeue();

                            return;
                        }
                    }
                    
                }
                // dequeue it.
                Queue.Dequeue();
                if(CurrentReadBufferIterationCount < ReadBufferIterationMax){
                    ListOccupied = true;
                    byte[] readBuffer;
                    int bufferSize = comPort.BytesToRead;
                    readBuffer = new byte[bufferSize];
                    if(bufferSize > 0){
                        comPort.Read(readBuffer, 0, bufferSize);
                        //System.Console.WriteLine(readBuffer[bufferSize - 1].ToString() + " " + readBuffer[bufferSize - 2].ToString());
                        bool isThisLastRead = false;
                        // Check the end of buffer: FF D9
                        if(bufferSize > 1){
                            if(readBuffer[bufferSize - 1] == 0xD9 && readBuffer[bufferSize - 2] == 0xFF){
                            isThisLastRead = true;
                            }
                        }
                        
                        for( int i = 0; i < bufferSize; i++){
                            OutToFile.Add(readBuffer[i]);
                        }
                        if(isThisLastRead){
                            // Data gathering is done! Write out to file.
                            // First, check if a folder for this day, month, and year exists.
                            string dateFormat = "MM_dd_yyyy";
                            string timeFormat = "HH_mm_ss";
                            if(!Directory.Exists(DateTime.Now.ToString(dateFormat))){
                                Directory.CreateDirectory(DateTime.Now.ToString(dateFormat));
                            }
                            string path =   DateTime.Now.ToString(dateFormat) + "//" + DateTime.Now.ToString(timeFormat);
                            using(FileStream fs = File.Create(path)){
                                fs.Write(OutToFile.ToArray(), 0, OutToFile.Count);
                            }
                            ShouldExit = true;
                        }else{ListOccupied = false;}
                    }
                }else{ShouldExit = true;}
           }catch(Exception ex){
                // If an exception occurs, we want to fail silently. 
                // Simply print an error out serial and continue.
                Console.Out.WriteLine("Program.DataReceived_Handler - Exception thrown: " + ex.Message);
           } 
           
            
           

            
        }
    }
}

