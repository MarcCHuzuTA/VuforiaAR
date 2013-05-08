/*==============================================================================
        Copyright (c) 2012-2013 QUALCOMM Austria Research Center GmbH.
        All Rights Reserved.
        Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles drawing the UI during scan mode.
/// The viewfinder is drawn with colors depending on the current frame quality.
/// </summary>
public class ScanModeUILogic
{
    #region PRIVATE_MEMBERS

    // reference to the user interface class
    private readonly UserDefinedTargetBuildingUI mUserInterface;
    // this callback can be set from outside and is invoked when the "New Target" button is pressed
    private readonly Action mStartBuildingCallback;
    // this callback is invoked when the "Cancel" button is pressed
    private readonly Action mStopScanningCallback;

    #endregion // PRIVATE_MEMBERS



    #region CONSTRUCTION

    public ScanModeUILogic(UserDefinedTargetBuildingUI userInterface,
                           Action startBuildingCallback, Action stopScanningCallback)
    {
        mUserInterface = userInterface;
       
        mStartBuildingCallback = startBuildingCallback;
        mStopScanningCallback = stopScanningCallback;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    /// <summary>
    /// Draws the UI when in scan mode.
    /// Automatically shows Help dialog when starting and after timeout.
    /// </summary>
    public void DrawUI(ImageTargetBuilder.FrameQuality frameQuality)
    {
        if (frameQuality != ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE)
        {
            // draw the view finder UI...
            if (frameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_MEDIUM || frameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_HIGH)
            {
                 // draw the view finder with green marks
                 mUserInterface.DrawViewFinder(Color.green);
            }
            else
            {
                 // draw the basic viewfinder with white marks and show the scanning message
                 mUserInterface.DrawViewFinder(Color.white);
            }

            UserDefinedTargetBuildingUI.ButtonID buttonId = UserDefinedTargetBuildingUI.ButtonID.NONE_PRESSED;
            
            if(QCARRuntimeUtilities.IsLandscapeOrientation)
            {
                // Draws the Landscape UI for the Scanning Mode
                buttonId = mUserInterface.DrawScanningModeLandscapeUI();
            }
            else
            {
                // Draws the Portrait UI for the scanning mode
                buttonId = mUserInterface.DrawScanningModePortraitUI();
            }
            
            // Checks Button actions
            if (buttonId == UserDefinedTargetBuildingUI.ButtonID.CANCEL_PRESSED)
            {
                mStopScanningCallback();
            }
            else if (buttonId == UserDefinedTargetBuildingUI.ButtonID.BUILD_TARGET_PRESSED)
            {
                // check if the current frame has too low quality to create a good target
                if(frameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_LOW )
                {
                    ErrorMsg.New("Low Quality Image", "The image has very little detail, please try another.",
                                () => mUserInterface.ButtonPressed = true);
                }
                mStartBuildingCallback();
            }
        }
        
        // Draws possible error messages
        ErrorMsg.Draw();
    }

    #endregion // PUBLIC_METHODS
}