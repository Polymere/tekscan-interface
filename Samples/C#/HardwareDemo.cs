using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TekAPI; // Add TekAPI as a reference in project and add this line to import functions

/* Hardware_Demo.cs
 * Tekscan, Inc.
 * Copyright October, 2013
 *  
 * Rev 1.2
 * July 30, 2014
 * Updated function calls for SDK 1.2 compatibility (TekClaimSensor, TekStartRecording, TekStopRecording)
 * 
 * This sample code demonstrates how the you can read and record data using
 * Tekscan hardware using the OEM Toolkit. Tekscan hardware must be connected
 * and all Tekscan software should be closed before using OEM Toolkit functions. 
 * Please see product documentation for full descriptions of all available functions. 
 * 
 * All file paths included below assume that the files reside in the
 * default installation directory. If the files are moved, or different
 * .cal, .equ, or .fsx files are used, the paths must be changed
 * accordingly.
 * 
 * Many functions return error codes as defined in the documentation. We
 * recommend using these error codes to verify that operations complete
 * successfully. The TekEnableLogging and TekGetLastError functions can be
 * used for debugging.
 */

namespace TekAPI_Sample_Code
{
    class HardwareDemo
    {
        static void Main(string[] args)
        {
            string tekApiUserPath = @"D:\EPFL\thesis\Tekscan\myClient\TekAPI";

            /* Load a calibration/equilibration (optional)
             * Return types are CTekEquilibration and CTekCalibration objects,
             * respectively. These objects provide a function allowing for calibration
             * and equilibration of arrays of captured frame data and can also be passed
             * as parameters to functions that save recordings or apply calibrations and
             * equilibrations to .fsx files. 
             */
            string mapFileDirectory = tekApiUserPath+@"\Samples"; // must set map file directory to load a calibration
			CTekAPI.TekSetMapFileDirectory(mapFileDirectory);
            string equilibrationFilePath = tekApiUserPath + @"\Samples\SampleEquil.equ";
            string calibrationFilePath = tekApiUserPath +@"\Samples\SampleCal.cal";
            CTekEquilibration equilibration = CTekAPI.TekLoadEquilibration(equilibrationFilePath);
            CTekCalibration calibration = CTekAPI.TekLoadCalibration(calibrationFilePath);

            // Find and claim available hardware
            // Required as first step before any hardware will be recognized.
            CTekAPI.TekInitializeHardware();

            // Get list of available serial numbers, returned as System.String[].
            String[] availableSerialNumbers;
            int errorCode = CTekAPI.TekEnumerateHandles(out availableSerialNumbers);

            // Get lowest serial number to use as an identifier for future calls.
            string serialNumber = availableSerialNumbers[0]; // sensor with lowest serial number

            // The map file used here should match the type of sensor you are using.
            string mapFilePath = tekApiUserPath +@"\Samples\null.mp";
            CTekAPI.TekClaimSensor(ref availableSerialNumbers, mapFilePath);

            // Set up the selected sensor
            long framePeriod = 10000; // controls the period in microseconds of data collection (1/frequency)
            CTekAPI.TekInitializeSensor(serialNumber, framePeriod); // framePeriod in microseconds: 10000 microseconds = 100 Hz

            // Sensitivity can  be set using an integer 1-40 (matching IScan
            // slider levels) 
            CTekAPI.TekSetSensitivityLevel(serialNumber, 20);
            /* OR
             * Sensitivity can also be set to match a calibration file's setting. This
             * method is highly recommended if you plan to apply calibrations to collected
             * data. While these operations will complete even if the sensitivity of the
             * recording and calibration do not match, an error code will be produced
             * when saving recordings with calibration or applying calibrations to recordings.
             */
            //CTekAPI.TekSetCalibratedSensitivity(serialNumber, calibration);

            // Get details about the sensor (optional)
            int rows, columns;
            double rowSpacing, columnSpacing;
            errorCode = CTekAPI.TekGetSensorRows(serialNumber, out rows);
            errorCode = CTekAPI.TekGetSensorColumns(serialNumber, out columns);
            rowSpacing = CTekAPI.TekGetSensorRowSpacing(serialNumber, out rowSpacing);
            columnSpacing = CTekAPI.TekGetSensorColumnSpacing(serialNumber, out columnSpacing);

            // Get frame data in real time
            int timeOut = 100; // time-out in milliseconds
            byte[] frameData;
            errorCode = CTekAPI.TekCaptureDataFrame(serialNumber, timeOut, out frameData);

            // Print frame to console in space-delimited format
            // The array returned when capturing a frame contains columns from the sensor appended in sequence
            for (int i = 0; i < frameData.Length; i++)
            {
                // Write data to console 
                Console.Write(frameData[i] + " ");

                // If next element is the start of a new column, start a new line
                if ((i + 1) % columns == 0)
                {
                    Console.WriteLine();
                }
            }

            // Apply calibration and equilibration to frame data (optional)
            equilibration.TekEquilibrate(frameData); // passed by reference, data is now equilibrated

            double[] frameDataCalibrated;
            errorCode = calibration.TekCalibrate(frameData, out frameDataCalibrated);

            // Take a recording
            CTekAPI.TekStartRecording(5); // 5 second recording
            Console.WriteLine("Recording...");

            // Total number frames that will be collected
            int framesToRecord;
            errorCode = CTekAPI.TekGetFramesToRecord(serialNumber, out framesToRecord);

            // Wait until the recording is complete
            int framesRecorded;
            while (CTekAPI.TekIsRecording( ) == 0)
            {
                // Can also get the number of frames recorded so far
                errorCode = CTekAPI.TekGetFramesRecorded(serialNumber, out framesRecorded);
                System.Threading.Thread.Sleep(100);
            }
            Console.WriteLine("Recording complete.");

            // Or, can manually stop the recording at any point
            CTekAPI.TekStopRecording();

            // Save the recording
            string recordingPath = tekApiUserPath +@"\Samples\MySampleRecording.fsx";
            CTekAPI.TekSaveRecording(serialNumber, ref recordingPath);
            Console.WriteLine("Recording saved.");
            // OR
            // Recordings can also be saved with calibrations and equilibrations
            // CTekAPI.TekSaveRecording(serialNumber, ref recordingPath, equilibration);
            // CTekAPI.TekSaveRecording(serialNumber, ref recordingPath, calibration);
            // CTekAPI.TekSaveRecording(serialNumber, ref recordingPath, equilibration, calibration);

            /* IMPORTANT: Release hardware resources
             * Failure to do this could leave hardware in an unusable state, requiring
             * the handle/hub to be disconnected and reconnected to cycle power. Placing
             * this code in the catch block of a try-catch can ensure that errors in
             * other parts of the program do not prevent these statements from executing.
             */
            CTekAPI.TekReleaseSensor(serialNumber);
            CTekAPI.TekDeinitializeHardware();
        }
    }
}
