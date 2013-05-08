/*==============================================================================
        Copyright (c) 2012-2013 QUALCOMM Austria Research Center GmbH.
        All Rights Reserved.
        Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This event handler contains the logic that handles events from the UserDefinedTargetBehaviour
/// and creates new targets in a dataset from a TrackableSource. 
/// </summary>
public class UserDefinedTargetEventHandler : MonoBehaviour, IUserDefinedTargetEventHandler
{
    #region PRIVATE_MEMBER_VARIABLES

    // UserDefinedTargetBuildingBehaviour reference to avoid lookups
    private UserDefinedTargetBuildingBehaviour mTargetBuildingBehaviour;
    // ImageTracker reference to avoid lookups
    private ImageTracker mImageTracker;
    // member to the user interface class 
    private UserDefinedTargetBuildingUI mUserInterface;
    // logic class that handles drawing the UI in scan mode:
    private ScanModeUILogic mScanModeUILogic;
    // DataSet that newly defined targets are added to
    private DataSet mBuiltDataSet;

    // if the OnInitialized callback has been called
    private bool mOnInitializedCalled = false;
    // counter variable used to name duplicates of the image target template
    private int mTargetCounter;
    // name of the trackable that is/was currently created
    private string mCurrentTargetName;
    // currently observed frame quality
    private ImageTargetBuilder.FrameQuality mFrameQuality = ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE;
    
    private bool mDoShowInstructions = true;
    private bool mDrawInstructionsFlag = false;

    // text displayed when the sample cannot be run in landscape left
    private const string ERROR_TEXT_ORIENTATION = "The User Defined Targets sample must have a landscape left orientation";

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_MEMBERS

    /// <summary>
    /// Can be set in the Unity inspector to reference a ImageTargetBehaviour that is instanciated for augmentations of new user defined targets.
    /// </summary>
    public ImageTargetBehaviour ImageTargetTemplate;

    #endregion // PUBLIC_MEMBERS



    #region PROPERTIES

    /// <summary>
    /// Reference to the UserInterface class
    /// </summary>
    public UserDefinedTargetBuildingUI UserInterFace
    {
        get { return mUserInterface; }
    }

    #endregion // PROPERTIES



    #region IUserDefinedTargetEventHandler_IMPLEMENTATION

    /// <summary>
    /// Called when UserDefinedTargetBuildingBehaviour has been initialized successfully
    /// </summary>
    public void OnInitialized()
    {
        // look up the ImageTracker once and store a reference
        mImageTracker = (ImageTracker)TrackerManager.Instance.GetTracker(
                                        Tracker.Type.IMAGE_TRACKER);

        // instanciate the UserInterface
        mUserInterface = new UserDefinedTargetBuildingUI();
        mScanModeUILogic = new ScanModeUILogic(mUserInterface, BuildNewTarget, mTargetBuildingBehaviour.StopScanning);

        if (mImageTracker != null)
        {
            // create a new dataset
            mBuiltDataSet = mImageTracker.CreateDataSet();
            mImageTracker.ActivateDataSet(mBuiltDataSet);

            // remember that the component has been initialized
            mOnInitializedCalled = true;
        }
    }

    /// <summary>
    /// Updates the current frame quality
    /// </summary>
    public void OnFrameQualityChanged(ImageTargetBuilder.FrameQuality frameQuality)
    {
        mFrameQuality = frameQuality;
    }

    /// <summary>
    /// Takes a new trackable source and adds it to the dataset
    /// </summary>
    public void OnNewTrackableSource(TrackableSource trackableSource)
    {
        // deactivates the dataset first
        mImageTracker.DeactivateDataSet(mBuiltDataSet);
        
        // Destroy the oldest target if the dataset is full or the dataset 
        // already contains five user-defined targets.
        if (mBuiltDataSet.HasReachedTrackableLimit() || mBuiltDataSet.GetTrackables().Count() >= 5)
        {
            IEnumerable<Trackable> trackables = mBuiltDataSet.GetTrackables();
            Trackable oldest = null;
            foreach (Trackable trackable in trackables)
                if (oldest == null || trackable.ID < oldest.ID)
                    oldest = trackable;

            if (oldest != null)
            {
                Debug.Log("Destroying oldest trackable in UDT dataset: " + oldest.Name);
                mBuiltDataSet.Destroy(oldest, true);
            }
        }

        // get predefined trackable and instantiate it
        ImageTargetBehaviour imageTargetCopy = (ImageTargetBehaviour)Instantiate(ImageTargetTemplate);

        // add the duplicated trackable to the data set and activate it
        mBuiltDataSet.CreateTrackable(trackableSource, imageTargetCopy.gameObject);

        mTargetCounter++;

        // activate the dataset again
        mImageTracker.ActivateDataSet(mBuiltDataSet);

    }

    #endregion // IUserDefinedTargetEventHandler_IMPLEMENTATION



    #region UNTIY_MONOBEHAVIOUR_METHODS

    /// <summary>
    /// Registers this component as an event handler at the UserDefinedTargetBuildingBehaviour
    /// </summary>
    void Start()
    {
        // intialize the ErrorMsg class
        ErrorMsg.Init();

        mTargetBuildingBehaviour = GetComponent<UserDefinedTargetBuildingBehaviour>();
        if (mTargetBuildingBehaviour)
        {
            mTargetBuildingBehaviour.RegisterEventHandler(this);
        }
    }
    
    void Update () 
    {
        // Back Key goes back to the About Scene
        if (Input.GetKeyDown(KeyCode.Escape)) 
        { 
            Application.LoadLevel("Vuforia-2-AboutScreen");
        }
    }
    
    
    /// <summary>
    /// Renders the UI, depending on the current state
    /// </summary>
    void OnGUI()
    {
        if (mOnInitializedCalled)
        {
            // BeginOnGUI tells the user interface to reset some values and set the right GUISkin
            mUserInterface.BeginOnGUI();

            // check in which mode we are
            if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE)
            {
                if (QCARRuntimeUtilities.IsLandscapeOrientation)
                {        
                    if(!mDrawInstructionsFlag )
                    {
                        // Draws Landscape UI for ViewFinder Mode & process button actions
                        if (mUserInterface.DrawViewFinderModeLandscapeUI() == UserDefinedTargetBuildingUI.ButtonID.ENTER_SCANNING_MODE_PRESSED)
                        {
                            // Checks if the instructions Screen need to be displayed
                            if(mDoShowInstructions)
                            {
                                // Draws the instructions UI
                                mDrawInstructionsFlag = true;
                            
                            }else
                            {
                                // Enters the scanning mode
                                mTargetBuildingBehaviour.StartScanning();    
                            }
                    
                        }
                    }
                    else
                    {
                        // Draws the Landscape Instructions UI
                        UserDefinedTargetBuildingUI.ButtonID buttonPreessedId = mUserInterface.DrawInstructionsLandscapeUI();
                        
                        // Checks Instructions button pressed
                        if (buttonPreessedId == UserDefinedTargetBuildingUI.ButtonID.INSTRUCTIONS_OK_PRESSED)
                        {
                            mDrawInstructionsFlag = false;
                            mDoShowInstructions = false;
                            mTargetBuildingBehaviour.StartScanning();

                        }
                        else if (buttonPreessedId == UserDefinedTargetBuildingUI.ButtonID.CANCEL_PRESSED)                        
                        {
                            mDrawInstructionsFlag = false;
                        }
                    }
                }
                else
                {
                    if(!mDrawInstructionsFlag )
                    {
                        // Draws Portrait mode for ViewFinder Mode & process button actions
                        if (mUserInterface.DrawViewFinderModePortraitUI() == UserDefinedTargetBuildingUI.ButtonID.ENTER_SCANNING_MODE_PRESSED)
                        {
                    
                            // Checks if the instructions Screen need to be displayed
                            if(mDoShowInstructions)
                            {
                                // Draws the instructions UI
                                mDrawInstructionsFlag = true;
                            
                            }else
                            {
                                // Enters the scanning mode
                                mTargetBuildingBehaviour.StartScanning();
                            }
                        }
                    }else
                    {                    
                        // Draws the portrait Instructions UI
                        UserDefinedTargetBuildingUI.ButtonID buttonPreessedId = mUserInterface.DrawInstructionsPortraitUI();
                        
                        // Checks Instructions button pressed
                        if (buttonPreessedId == UserDefinedTargetBuildingUI.ButtonID.INSTRUCTIONS_OK_PRESSED)
                        {
                            mDrawInstructionsFlag = false;
                            mDoShowInstructions = false;
                            mTargetBuildingBehaviour.StartScanning();
                        }
                        else if (buttonPreessedId == UserDefinedTargetBuildingUI.ButtonID.CANCEL_PRESSED)                        
                        {
                            mDrawInstructionsFlag = false;
                        }
                    }
                        
                }
            }

            // draw scanning mode UI, decides internally what should be rendered
            mScanModeUILogic.DrawUI(mFrameQuality);

        }
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS



    #region PRIVATE_METHODS

    /// <summary>
    /// updates the next target name and triggers a target build
    /// </summary>
    private void BuildNewTarget()
    {
        // create the name of the next target.
        // the TrackableName of the original, linked ImageTargetBehaviour is extended with a continuous number to ensure unique names
        mCurrentTargetName = string.Format("{0}-{1}", ImageTargetTemplate.TrackableName, mTargetCounter);

        // generate a new target name:
        Debug.Log("ImageTargetBuilder.Build with target name " + mCurrentTargetName);

        mTargetBuildingBehaviour.BuildNewTarget(mCurrentTargetName, ImageTargetTemplate.GetSize().x);
    }

    #endregion // PRIVATE_METHODS
}
