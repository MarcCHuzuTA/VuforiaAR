/*==============================================================================
            Copyright (c) 2012-2013 QUALCOMM Austria Research Center GmbH.
            All Rights Reserved.
            Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// User defined target menu.
/// 
/// Draws a menu on double tap which allows the user to enable/disable
/// autofocus.
/// 
/// Also includes the Touch to Focus functionality
/// </summary>
public class UserDefinedTargetMenu : MonoBehaviour, ITrackerEventHandler
{
    #region PRIVATE_MEMBER_VARIABLES

    // If the menu is currently open
    private bool mMenuOpen = false;

    // Contains if the device supports continous autofocus
    private bool mContinousAFSupported = true;

    // Contains the currently set auto focus mode.
    private CameraDevice.FocusMode mFocusMode =
        CameraDevice.FocusMode.FOCUS_MODE_NORMAL;

    // this is used to distinguish single and double taps
    private bool mWaitingForSecondTap;
    private Vector3 mFirstTapPosition;
    private DateTime mFirstTapTime;
    // the maximum distance that is allowed between two taps to make them count as a double tap
    // (relative to the screen size)
    private const float MAX_TAP_DISTANCE_SCREEN_SPACE = 0.1f;
    private const int MAX_TAP_MILLISEC = 500;

    // reference to the UserDefinedTargetEventHandler behaviour
    private UserDefinedTargetEventHandler mUserDefinedTargetEventHandler;
    
    // Unity GUI Skin containing settings for font and custom image buttons
    private GUISkin mUISkin;
    
    // dictionary to hold gui styles, fetching them each time a button is drawn is slow
    private Dictionary<string, GUIStyle> mButtonGUIStyles;

    private const string AUTOFOCUS_ON = "Autofocus On";
    private const string AUTOFOCUS_OFF = "Autofocus Off";
    private string mAutoFocusText = "";
    
    #endregion // PRIVATE_MEMBER_VARIABLES



    #region UNTIY_MONOBEHAVIOUR_METHODS

    public void Start()
    {
        // register for the OnInitialized event at the QCARBehaviour
        QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackerEventHandler(this);
        }

        // look up the UserDefinedTargetEventHandler
        mUserDefinedTargetEventHandler = (UserDefinedTargetEventHandler)FindObjectOfType(typeof(UserDefinedTargetEventHandler));
        
        // load and set gui style
        mUISkin = Resources.Load("UserInterface/ButtonSkins") as GUISkin;
        
        // remember all custom styles in gui skin to avoid constant lookups
        mButtonGUIStyles = new Dictionary<string,GUIStyle>();
        foreach (GUIStyle customStyle in mUISkin.customStyles) mButtonGUIStyles.Add(customStyle.name, customStyle);
    }
    

    public void Update()
    {
        // If the touch event results from a button press it is ignored.
        if (!mUserDefinedTargetEventHandler.UserInterFace.ButtonPressed)
        {
            if (mMenuOpen)
            {
                // If finger is removed from screen.
                if (Input.GetMouseButtonUp(0))
                {
                    HandleSingleTap();
                }
            }
            else
            {
                // check if it is a doulbe tap
                if (Input.GetMouseButtonUp(0))
                {
                    if (mWaitingForSecondTap)
                    {
                        // check if time and position match:
                        int smallerScreenDimension = Screen.width < Screen.height ? Screen.width : Screen.height;
                        if (DateTime.Now - mFirstTapTime < TimeSpan.FromMilliseconds(MAX_TAP_MILLISEC) &&
                            Vector4.Distance(Input.mousePosition, mFirstTapPosition) < smallerScreenDimension*MAX_TAP_DISTANCE_SCREEN_SPACE)
                        {
                            // it's a double tap
                            HandleDoubleTap();
                        }
                        else
                        {
                            // too late/far to be a double tap, treat it as first tap:
                            mFirstTapPosition = Input.mousePosition;
                            mFirstTapTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        // it's the first tap
                        mWaitingForSecondTap = true;
                        mFirstTapPosition = Input.mousePosition;
                        mFirstTapTime = DateTime.Now;
                    }
                }
                else
                {
                    // time window for second tap has passed, trigger single tap
                    if (mWaitingForSecondTap && DateTime.Now - mFirstTapTime > TimeSpan.FromMilliseconds(MAX_TAP_MILLISEC))
                    {
                        HandleSingleTap();
                    }
                }
            }
        }
        else
        {
            mUserDefinedTargetEventHandler.UserInterFace.ButtonPressed = false;
        }
    }


    // Draw menus.
    public void OnGUI()
    {
        if (mMenuOpen)
        {
            // scale the menu buttons
            // because of this scaling, hardcoded values can be used
            int smallerScreenDimension = Screen.width < Screen.height ? Screen.width : Screen.height;
            float deviceDependentScale = smallerScreenDimension / 480f;

            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            GUIUtility.ScaleAroundPivot(new Vector2(deviceDependentScale, deviceDependentScale), screenCenter);
            
            GUIStyle itemButtonStyle;
            if (mButtonGUIStyles.TryGetValue("ItemMenu", out itemButtonStyle))
            {
                if (!mContinousAFSupported)
                {
                    mAutoFocusText = "Cont. Auto Focus not supported ";
                }
                
                if(GUI.Button(new Rect(0, Screen.height/2 - 100, Screen.width,114),mAutoFocusText, itemButtonStyle))
                {
                    if (mFocusMode != CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO)
                    {
                        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
                        {
                              mFocusMode = CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO;
                            mAutoFocusText = AUTOFOCUS_OFF;
                        }
                        
                        mMenuOpen = false;
                        mUserDefinedTargetEventHandler.UserInterFace.ButtonPressed = true;
                            
                    }else
                    {
                        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL))
                        {
                              mFocusMode = CameraDevice.FocusMode.FOCUS_MODE_NORMAL;
                            mAutoFocusText = AUTOFOCUS_ON;
                        }
                            
                        mMenuOpen = false;
                        mUserDefinedTargetEventHandler.UserInterFace.ButtonPressed = true;
                    }
                }
                
                if(GUI.Button(new Rect(0, Screen.height/2 + 20, Screen.width,114),"Cancel", itemButtonStyle))
                {
                    mMenuOpen = false;
                    mUserDefinedTargetEventHandler.UserInterFace.ButtonPressed = true;
                }
            }
            
            // reset scale after drawing
            GUIUtility.ScaleAroundPivot(Vector2.one, screenCenter);
        }
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS



    #region ITrackerEventHandler_IMPLEMENTATION

    /// <summary>
    /// This method is called when QCAR has finished initializing
    /// </summary>
    public void OnInitialized()
    {
        // try to set continous auto focus as default
        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
        {
            mFocusMode = CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO;
            mAutoFocusText = AUTOFOCUS_OFF;
        }
        else
        {
            Debug.LogError("could not switch to continuous autofocus");
            mContinousAFSupported = false;
            mAutoFocusText = "Cont. Auto Focus not supported";
        }
    }

    public void OnTrackablesUpdated()
    {
        // not used
    }

    #endregion //ITrackerEventHandler_IMPLEMENTATION



    #region PRIVATE_METHODS

    private void HandleSingleTap()
    {
        mWaitingForSecondTap = false;

        if (mMenuOpen)
            mMenuOpen = false;
        else
        {
            // trigger focus once
            if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO))
            {
                mFocusMode = CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO;
                mAutoFocusText = AUTOFOCUS_ON;
            }
        }
    }

    private void HandleDoubleTap()
    {
        mWaitingForSecondTap = false;
        mMenuOpen = true;
    }

    #endregion // PRIVATE_METHODS
}
