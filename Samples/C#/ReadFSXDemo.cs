using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TekAPI; // Add TekAPI as a reference in project and add this line to import functions

/*  ReadFSXDemo.cs
 * Tekscan, Inc.
 * Copyright October, 2013
 * 
 * Rev 1.0
 * October 25, 2013
 * Initial Release
 * 
 * This sample code demonstrates how the you can read and modify .FSX files
 * using the OEM Toolkit. Please see product documentation for full 
 * descriptions of all available functions.
 * 
 * All file paths included below assume that the files reside in the
 * default installation directory. If the files are moved, or different
 * .cal, .equ, or .fsx files are used, the paths must be changed accordingly.
 * 
 * Many functions return error codes as defined in the documentation. We
 * recommend using these error codes to verify that operations complete
 * successfully. The TekEnableLogging and TekGetLastError functions can be
 * used for debugging.
 */

namespace Read_FSX_Demo
{
    class ReadFSXDemo
    {
        static void Main(string[] args)
        {
            // Open .FSX file
            // Set the location to look for .mp files
            string mapFileDirectory = @"C:\Tekscan\TekAPI\Samples";
            CTekAPI.TekSetMapFileDirectory(mapFileDirectory);

            string recordingPath = @"C:\Tekscan\TekAPI\Samples\SampleRecording.fsx";
            CTekFile recording = CTekAPI.TekLoadRecording(recordingPath);

            // Get data from file
            int frameNumber = 0; // 0-based frame counting (first frame of the recording)
            byte[] recordingData, peakFrameData;
            int errorCode = recording.TekGetRawFrameData(out recordingData, frameNumber);
            errorCode = recording.TekGetPeakFrameData(out peakFrameData);

            /* Will only return calibrated data if recording has a calibration applied.
             * Both raw and calibrated data will be equilibrated if an equilibration is
             * applied to the recording.
             */
            double[] calibratedRecordingData;
            errorCode = recording.TekGetCalibratedFrameData(out calibratedRecordingData, frameNumber);

            // Get details about the recording
            int rows = recording.TekGetRows();
            int columns = recording.TekGetColumns();
            int numberOfFrames = recording.TekGetFrameCount();
            double rowSpacing = recording.TekGetRowSpacing();
            double columnSpacing = recording.TekGetColumnSpacing();

            // Print frame to console in space-delimited format
            // The array returned from the recording contains columns from the sensor appended in sequence
            for (int i = 0; i < recordingData.Length; i++)
            {
                // Write data to console 
                Console.Write(recordingData[i] + " ");

                // If next element is the start of a new column, start a new line
                if ((i + 1) % columns == 0)
                {
                    Console.WriteLine();
                }
            }
            
            /* Load a calibration/equilibration (optional)
            * Return types are CTekEquilibration and CTekCalibration objects,
            * respectively. These objects provide a function allowing for calibration
            * and equilibration of arrays of captured frame data and can also be passed
            * as parameters to functions that save recordings or apply calibrations and
            * equilibrations to .fsx files.
            */
            string equilibrationFilePath = @"C:\Tekscan\TekAPI\Samples\SampleEquil.equ";
            string calibrationFilePath = @"C:\Tekscan\TekAPI\Samples\SampleCal.cal";
            CTekEquilibration equilibration = CTekAPI.TekLoadEquilibration(equilibrationFilePath);
            CTekCalibration calibration = CTekAPI.TekLoadCalibration(calibrationFilePath);

            /* Apply calibrations and equilibrations to files
             * While these operations will complete even if the sensitivity of the
             * recording and calibration do not match (not recommended), an error code
             * will be produced when saving recordings or applying calibrations to recordings.
             */
            //recording.TekApplyCalibration(calibration);
            //recording.TekApplyEquilibration(equilibration);

            // Undo calibrations and equilibrations in files
            recording.TekClearCalibration();
            recording.TekClearEquilibration();
        }
    }
}
