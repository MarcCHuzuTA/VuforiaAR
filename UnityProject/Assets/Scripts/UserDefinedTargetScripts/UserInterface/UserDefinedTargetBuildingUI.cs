/*==============================================================================
        Copyright (c) 2012-2013 QUALCOMM Austria Research Center GmbH.
        All Rights Reserved.
        Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// User defined target building UI.
/// 
/// This class handles the UI Drawing for the different 
/// states of the Application and available screen orientations.
/// 
/// All UI is scaled and sking for different screen sizes
/// and dpis.
/// </summary>
public class UserDefinedTargetBuildingUI
{
    #region NESTED

    /// <summary>
    /// Button Identifiers
    /// </summary>
    public enum ButtonID    
    { 
        NONE_PRESSED,
        ENTER_SCANNING_MODE_PRESSED,
        BUILD_TARGET_PRESSED,
        CANCEL_PRESSED,
        INSTRUCTIONS_OK_PRESSED
    }

    /// <summary>
    /// enum of all textures used in the user defined targets sample project
    /// </summary>
    public enum TextureName
    {
        TEXTURE_INSTRUCTIONS_BACKGROUND,
        TEXTURE_INSTRUCTIONS_ICON1,
        TEXTURE_INSTRUCTIONS_ICON2,
        TEXTURE_INSTRUCTIONS_ICON3,
        TEXTURE_VIEWFINDER_MARKS_PORTRAIT,
        TEXTURE_VIEWFINDER_MARKS_LANDSCAPE,
        TEXTURE_NAV_BACKGROUND,
        TEXTURE_ICON_PLUS,
        TEXTURE_ICON_CAMERA,
        TEXTURE_ICON_CAMERA_LANDSCAPE,
        TEXTURE_COUNT
    };

    public enum HorizontalPosition
    {
        TOP,
        CENTERED,
        BOTTOM
    }

    #endregion // NESTED



    #region PRIVATE_MEMBER_VARIABLES

    private const float INSTRUCTIONS_MARGIN = 50.0f;
    // Defines UI Navigation bar size
    private const float NAVIGATION_BAR_SIZE = 80.0f;

    // used to determine first time that BeginOnGUI() is called
    private bool mInitialized;
    
    //  array that all textures are loaded into on startup
    private readonly Texture[] mTextures;

    // Unity GUI Skin containing settings for font and custom image buttons
    private readonly GUISkin mUISkin;
    
    // dictionary to hold gui styles, fetching them each time a button is drawn is slow
    private readonly Dictionary<string, GUIStyle> mButtonGUIStyles;

    // array of all texture names
    private static readonly string[] sTextureNames =
    {
        "semiTransparentTexture",
        "icon_01",
        "icon_02",
        "icon_03",
        "viewfinder_crop_marks_portrait",
        "viewfinder_crop_marks_landscape",
        "grayTexture",
        "icon_plus",
        "icon_camera",
        "icon_camera_landscape",
        
    };

    // used to interpolate viewfinder color over time
    private Color mLastViewFinderColor = Color.white;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    /// <summary>
    /// if a button was pressed in this frame (to disinguish button presses from other touch events)
    /// </summary>
    public bool ButtonPressed { get; set; }

    /// <summary>
    /// public getter for GUI skin
    /// </summary>
    public GUISkin Skin
    {
        get { return mUISkin; }
    }

    /// <summary>
    /// this scaling factor is used to scale all textures that are drawn in pixel coordinates to make them
    /// appear the same size on all devices. The de-facto standard height of 480 of android (in landscape mode) is taken as the basis
    /// </summary>
    private static float DeviceDependentScale
    {
        get
        {
            if (QCARRuntimeUtilities.IsLandscapeOrientation) 
                return Screen.height / 480f;
            else 
                return Screen.width / 480f; 
        }
    }

    #endregion // PROPERTIES



    #region CONSTRUCTION

    public UserDefinedTargetBuildingUI()
    {
        // load and remember all used textures:
        int textureCount = (int)TextureName.TEXTURE_COUNT;
        mTextures = new Texture[textureCount];
        for (int i=0; i<textureCount; i++)
        {
            mTextures[i] = Resources.Load("UserInterface/" + sTextureNames[i]) as Texture; 
        }
        
        // Loads the skin for different screen sizes
        if(Screen.dpi > 260 )
        {
             // load and set gui style
            mUISkin = Resources.Load("UserInterface/ButtonSkinsXHDPI") as GUISkin;
            
        }else
        {
             // load and set gui style
            mUISkin = Resources.Load("UserInterface/ButtonSkins") as GUISkin;
        }
        
        #if UNITY_IPHONE
        if(Screen.height > 1500)
        {
            // Loads the XHDPI sources for the iPAd 3
            mUISkin = Resources.Load("UserInterface/ButtonSkinsiPad3") as GUISkin;
        }

        #endif

        // remember all custom styles in gui skin to avoid constant lookups
        mButtonGUIStyles = new Dictionary<string,GUIStyle>();
        foreach (GUIStyle customStyle in mUISkin.customStyles) mButtonGUIStyles.Add(customStyle.name, customStyle);
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    /// <summary>
    /// Resets the GUI state
    /// </summary>
    public void BeginOnGUI()
    {
        // when called first, set GUI skin and remember
        if (!mInitialized)
        {
            GUI.skin = mUISkin;
            mInitialized = true;
        }
    }

    /// <summary>
    /// Draws the view finder textures with correct position and scale
    /// </summary>
    /// <param name="color"></param>
    public void DrawViewFinder(Color color)
    {       
        // has viewfinder color changed?
        bool viewFinderColorChanged = !color.Equals(mLastViewFinderColor);
        // switch color to draw marks if necessary
        bool changeColor = !color.Equals(GUI.color) || viewFinderColorChanged;

        Color oldColor = GUI.color;
        // interpolate continously to new color:
        if (changeColor) GUI.color =  viewFinderColorChanged ? LerpColorContinuously(mLastViewFinderColor, color) : color;
        mLastViewFinderColor = GUI.color;

        if (QCARRuntimeUtilities.IsLandscapeOrientation)
        {
            DrawTextureCenteredScaledFromScreenSize(mTextures[(int)TextureName.TEXTURE_VIEWFINDER_MARKS_LANDSCAPE], 1.0f);
        }else
        {
            DrawTextureCenteredScaledFromScreenSize(mTextures[(int)TextureName.TEXTURE_VIEWFINDER_MARKS_PORTRAIT], 1.0f);
        }

        // reset original color
        if (changeColor) GUI.color = oldColor;
    }
    
    /// <summary>
    /// Draws the navigation Bar background
    /// </summary>
    public void DrawNavigationBarBackground(float scale=1.0f, float height = 90, bool isVertical = true){
    
        if( isVertical )
        {
            height *= DeviceDependentScale;

            if (QCARRuntimeUtilities.IsLandscapeOrientation)
            {
                GUI.DrawTexture(new Rect(Screen.width - height, 0, height, Screen.height),mTextures[(int)TextureName.TEXTURE_NAV_BACKGROUND]);
            }
            else
            {
                GUI.DrawTexture(new Rect(0, Screen.height - height, Screen.width, height),mTextures[(int)TextureName.TEXTURE_NAV_BACKGROUND]);
            }
        }else
        {
            height *= DeviceDependentScale;
    
            GUI.DrawTexture(new Rect(0, Screen.height - height, Screen.width, height),mTextures[(int)TextureName.TEXTURE_NAV_BACKGROUND]);
            
        }
        
    }
    
    /// <summary>
    /// Draws the help bar at the top
    /// </summary>
    public void DrawHelpBar( float height = 80)
    {
        height *= DeviceDependentScale;
        
        GUIStyle style;
        if (mButtonGUIStyles.TryGetValue("HelpBar", out style))
        {
            if (QCARRuntimeUtilities.IsLandscapeOrientation)
            {
                GUI.Box(new Rect(0,0,Screen.width - NAVIGATION_BAR_SIZE*DeviceDependentScale,height),"Hold the device parallel to the target",style);
            }
            else
            {    
                GUI.Box(new Rect(0,0,Screen.width,height),"Hold the device parallel to the target",style);
            }
        }
    }
    
    
    /// <summary>
    /// Draws the UI for the ViewFinder Mode
    /// </summary>
    public ButtonID DrawViewFinderModeLandscapeUI()
    {
        // Gets the scale for each device
        float  scale = 1* DeviceDependentScale;
        
        // Draws the navigation Bar background
        DrawNavigationBarBackground(1,NAVIGATION_BAR_SIZE, true);
        
        // Draws the Create New Target button
        // try to fetch style:
        GUIStyle style;
        if (mButtonGUIStyles.TryGetValue("NewTargetButton", out style))
        {
            // Button widths is half the screen height
            float width = Screen.height /2f;
            
            // Button height is calculated from the style background image
            float height = style.normal.background.height * scale;
            
            // Defines a button margin 
            float margin = 8 * scale;   
            
            // Since the button is rotated, the real buttonHeight is the calculated width
            float buttonHeight = width;
            
            float top = Screen.height/2f + buttonHeight/2f;
            float left = Screen.width - height - margin;
            
            // Rotates the UI
            GUIUtility.RotateAroundPivot(-90,new Vector2(left,top));
            
            // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
            if (GUI.Button(new Rect(left, top, width, height), mTextures[(int)TextureName.TEXTURE_ICON_PLUS], style))
            {
                ButtonPressed = true;
                return ButtonID.ENTER_SCANNING_MODE_PRESSED;
            };
            
            GUIUtility.RotateAroundPivot(90,new Vector2(left,top));
        }

        return ButtonID.NONE_PRESSED;
        
    }
    
    /// <summary>
    /// Draws the UI for the ViewFinder Mode
    /// </summary>
    public ButtonID DrawViewFinderModePortraitUI()
    {
        // Gets the scale for each device
        float  scale = 1* DeviceDependentScale;
        
        // Draws the navigation Bar background
        DrawNavigationBarBackground(1,NAVIGATION_BAR_SIZE, true);
        
        // Draws the Create New Target button
        // try to fetch style:
        GUIStyle style;
        if (mButtonGUIStyles.TryGetValue("NewTargetButton", out style))
        {
            // Button widths is half the screen height
            float width = Screen.width /2f;
            
            // Button height is calculated from the style background image
            float height = style.normal.background.height * scale;
            
            // Defines a button margin 
            float margin = 8 * scale;   
            
            float top = Screen.height - height - margin;
            float left = Screen.width/2f  - width/2f;
            
            // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
            if (GUI.Button(new Rect(left, top, width, height), mTextures[(int)TextureName.TEXTURE_ICON_PLUS], style))
            {
                ButtonPressed = true;
                return ButtonID.ENTER_SCANNING_MODE_PRESSED;
            };

        }

        return ButtonID.NONE_PRESSED;
        
    }
    
    /// <summary>
    /// Draws the UI for the Scanning mode
    /// </summary>
    public ButtonID DrawScanningModeLandscapeUI()
    {
        // Gets the scale for each device
        float  scale = 1* DeviceDependentScale;
        
        // Draws the navigation Bar background
        DrawNavigationBarBackground(1,NAVIGATION_BAR_SIZE, true);
        
        // Draws the help top bar
        DrawHelpBar(50);
        
        // Draws the Create Build Target button
        // try to fetch style:
        GUIStyle buildTargetStyle;
        if (mButtonGUIStyles.TryGetValue("NewTargetButton", out buildTargetStyle))
        {
            // Button widths is half the screen height
            float width = Screen.height /2f;
            
            // Button height is calculated from the style background image
            float height = buildTargetStyle.normal.background.height * scale;
            
            // Defines a button margin 
            float margin = 8 * scale;   
            
            // Since the button is rotated, the real buttonHeight is the calculated width
            float buttonHeight = width;
            
            float top = Screen.height/2f + buttonHeight/2f;
            float left = Screen.width - height - margin;
            
            // Rotates the UI
            GUIUtility.RotateAroundPivot(-90,new Vector2(left,top));
            
            // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
            if (GUI.Button(new Rect(left, top, width, height), mTextures[(int)TextureName.TEXTURE_ICON_CAMERA_LANDSCAPE], buildTargetStyle))
            {
                ButtonPressed = true;
                return ButtonID.BUILD_TARGET_PRESSED;
            };
            
            GUIUtility.RotateAroundPivot(90,new Vector2(left,top));
        }
        
        // Draws the Create Cancel button
        GUIStyle cancelStyle;
        if (mButtonGUIStyles.TryGetValue("CancelButton", out cancelStyle))
        {
            // Button widths is half the screen height
            float width = Screen.height /4f;
            
            // Button height is calculated from the navigation bar size
            float height = NAVIGATION_BAR_SIZE * scale;
                
            float top = Screen.height;
            float left = Screen.width - height;
            
            // Rotates the UI
            GUIUtility.RotateAroundPivot(-90,new Vector2(left,top));
            
            // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
            if (GUI.Button(new Rect(left, top, width, height), "Cancel", cancelStyle))
            {
                ButtonPressed = true;
                return ButtonID.CANCEL_PRESSED;
            };
            
            GUIUtility.RotateAroundPivot(90,new Vector2(left,top));
        }

        return ButtonID.NONE_PRESSED;
    }
    
    
    /// <summary>
    /// Draws the UI for the Scanning mode
    /// </summary>
    public ButtonID DrawScanningModePortraitUI()
    {
        // Gets the scale for each device
        float  scale = 1* DeviceDependentScale;
		
		// Draws the navigation Bar background
        DrawNavigationBarBackground(1, NAVIGATION_BAR_SIZE, true);
        
        // Draws the help top bar
        DrawHelpBar(50);
        
        // Draws the Create Build Target button
        // try to fetch style:
        GUIStyle buildTargetStyle;
        if (mButtonGUIStyles.TryGetValue("NewTargetButton", out buildTargetStyle))
        {
            // Button widths is half the screen height
            float width = Screen.width /2f;
            
            // Button height is calculated from the style background image
            float height = buildTargetStyle.normal.background.height * scale;
            
            // Defines a button margin 
            float margin = 8 * scale;   
            
            float top = Screen.height - height - margin;
            float left = Screen.width/2f - width/2f;
        
            // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
            if (GUI.Button(new Rect(left, top, width, height), mTextures[(int)TextureName.TEXTURE_ICON_CAMERA], buildTargetStyle))
            {
                ButtonPressed = true;
                return ButtonID.BUILD_TARGET_PRESSED;
            }
        }
        
        // Draws the Create Cancel button
        GUIStyle cancelStyle;
        if (mButtonGUIStyles.TryGetValue("CancelButton", out cancelStyle))
        {
            // Button widths is half the screen height
            float width = Screen.width /4f;
            
            // Button height is calculated from the navigation bar size
            float height = NAVIGATION_BAR_SIZE * scale;
                
            float top = Screen.height - height;
            float left = 0;
            
            // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
            if (GUI.Button(new Rect(left, top, width, height), "Cancel", cancelStyle))
            {
                ButtonPressed = true;
                return ButtonID.CANCEL_PRESSED;
            }
        }

        return ButtonID.NONE_PRESSED;
    }
    
    /// <summary>
    /// Draws the instructions in portrait UI.
    /// </summary>
    public ButtonID DrawInstructionsPortraitUI()
    {
            
        GUIStyle instructionsTitleStyle;
        if (mButtonGUIStyles.TryGetValue("InstructionsTitle", out instructionsTitleStyle))
        {
             // Gets the scale for each device
            float  scale = 1* DeviceDependentScale;
        
            // Draws a semi transparent texture as a backrgound
            GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height), mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_BACKGROUND]);
        
            // Draws the navigation Bar background
            DrawNavigationBarBackground(1,NAVIGATION_BAR_SIZE, true);
            
            // Draws the Instructions screen title
            GUI.Label(new Rect(INSTRUCTIONS_MARGIN* scale, 5 * scale,Screen.width, 20* scale),"Instructions", instructionsTitleStyle);
            
            GUIStyle boxStyle;
            if (mButtonGUIStyles.TryGetValue("EmptyBox", out boxStyle))
            {
                // Draws a 2 pixel division white line
                GUI.Box(new Rect(0,40 * scale,Screen.width,2),string.Empty, boxStyle);
                
            }
            
            // Instructions Item 1 Texture
            GUI.DrawTexture(new Rect(INSTRUCTIONS_MARGIN* scale, 
                Screen.height/5f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].height /2f* scale, 
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].width * scale , 
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].height * scale),
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1]);
            
            // Instructions Item 2 Texture
            GUI.DrawTexture(new Rect(INSTRUCTIONS_MARGIN* scale, 
                Screen.height/2.18f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].height /2f* scale, 
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].width * scale , 
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].height * scale),
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2]);
            
            // Instructions Item 3 Texture
            GUI.DrawTexture(new Rect(INSTRUCTIONS_MARGIN* scale, 
                Screen.height/1.4f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].height /2f* scale, 
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].width * scale, 
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].height * scale),
                mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3]);
            
            GUIStyle instructionsItemStyle;
            if (mButtonGUIStyles.TryGetValue("InstructionsItems", out instructionsItemStyle))
            {
                GUIStyle instructionsItemBigNumberStyle;
                mButtonGUIStyles.TryGetValue("InstructionsItemBigNumber", out instructionsItemBigNumberStyle);
                            
                // Instructions Item 1 Text
                GUILayout.BeginArea(new Rect(INSTRUCTIONS_MARGIN* scale + mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].width* scale, Screen.height/5f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].height /2f* scale, 220* scale, 150* scale));
        
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                GUI.Label(new Rect(1* scale,-15,220* scale,150* scale),"1.",instructionsItemBigNumberStyle);
                GUI.Label(new Rect(1* scale,80,220* scale,150* scale),"Hold the device parallel to the target",instructionsItemStyle);
            
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.EndArea();
                
                
                // Instructions Item 2 Text
                GUILayout.BeginArea(new Rect(INSTRUCTIONS_MARGIN* scale + mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].width* scale, Screen.height/2.18f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].height /2f* scale, 220* scale, 150* scale));
        
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                GUI.Label(new Rect(1* scale,-15,220* scale,150* scale),"2.",instructionsItemBigNumberStyle);
                GUI.Label(new Rect(1* scale,80,220* scale,150* scale),"Find the viewfinder with the target",instructionsItemStyle);
            
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.EndArea();
                
                
                // Instructions Item 3 Text
                GUILayout.BeginArea(new Rect(INSTRUCTIONS_MARGIN* scale + mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].width* scale, Screen.height/1.4f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].height /2f* scale, 220* scale, 150* scale));
        
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                GUI.Label(new Rect(1* scale,-15,220* scale,150* scale),"3.",instructionsItemBigNumberStyle);
                GUI.Label(new Rect(1* scale,80,220* scale,150* scale),"Take Picture",instructionsItemStyle);
            
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.EndArea();
                
            }
            
            
            // Draws the button Cancel and Ok
            GUIStyle buttonStyle;
            if (mButtonGUIStyles.TryGetValue("StartButton", out buttonStyle))
            {
                
                if( GUI.Button(new Rect(5* scale,Screen.height - 70* scale, (Screen.width/2f)-10* scale, 64* scale),"Cancel", buttonStyle))
                {
                    ButtonPressed = true;
                    return ButtonID.CANCEL_PRESSED;
                }
                
                if( GUI.Button(new Rect(Screen.width/2f + 5* scale,Screen.height - 70* scale, Screen.width/2f - 10* scale, 64* scale),"Ok", buttonStyle))
                {
                    ButtonPressed = true;
                    return ButtonID.INSTRUCTIONS_OK_PRESSED;
                }
                
            }
        }

        return ButtonID.NONE_PRESSED;
        
    }
    
    /// <summary>
    /// Draws the instructions in landscape UI.
    /// </summary>
    public ButtonID DrawInstructionsLandscapeUI()
    {
        GUIStyle instructionsTitleStyle;
        if (mButtonGUIStyles.TryGetValue("InstructionsTitle", out instructionsTitleStyle))
        {
            // scale the menu buttons
            // because of this scaling, hardcoded values can be used
            int smallerScreenDimension = Screen.width < Screen.height ? Screen.width : Screen.height;
            float deviceDependentScale = smallerScreenDimension / 480f;
            
            // Gets the scale for each device
            float  scale = 1* deviceDependentScale;
    
               // Draws a semi transparent texture as a backrgound
            GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height), mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_BACKGROUND]);
            
            // Draws the navigation Bar background
            DrawNavigationBarBackground(1,NAVIGATION_BAR_SIZE, false);
            
            // Draws the Instructions screen title
            GUI.Label(new Rect(INSTRUCTIONS_MARGIN* scale, 10* scale,Screen.width, 20* scale),"Instructions", instructionsTitleStyle);
            
            GUIStyle boxStyle;
            if (mButtonGUIStyles.TryGetValue("EmptyBox", out boxStyle))
            {
                // Draws a 2 pixel division white line
                GUI.Box(new Rect(0,50* scale,Screen.width,2),string.Empty, boxStyle);
            }
            
            // Instructions Item 1 Texture
            GUI.DrawTexture(new Rect(Screen.width/7f -  mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].height /2f* scale, 100* scale, mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].width* scale , mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].height * scale),mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1]);
            
            // Instructions Item 2 Texture
            GUI.DrawTexture(new Rect(Screen.width/2.15f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].height /2f* scale, 100* scale, mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].width* scale , mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].height* scale ),mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2]);
            
            // Instructions Item 3 Texture
            GUI.DrawTexture(new Rect(Screen.width/1.25f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].height /2f* scale, 100* scale, mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].width* scale , mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].height* scale ),mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3]);
            
            GUIStyle instructionsItemStyle;
            if (mButtonGUIStyles.TryGetValue("InstructionsItems", out instructionsItemStyle))
            {
                GUIStyle instructionsItemBigNumberStyle;
                mButtonGUIStyles.TryGetValue("InstructionsItemBigNumber", out instructionsItemBigNumberStyle);
                            
                // Instructions Item 1 Text
                GUILayout.BeginArea(new Rect(Screen.width/7f -  mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].height /2.2f* scale, 100* scale +  mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON1].height* scale, 220* scale, 150* scale));
        
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                GUI.Label(new Rect(1* scale,0,150* scale,150* scale),"1.",instructionsItemBigNumberStyle);
                GUI.Label(new Rect(75,12* scale,120* scale,150* scale),"Hold the device parallel to the target",instructionsItemStyle);
            
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.EndArea();
                
                // Instructions Item 2 Text
                GUILayout.BeginArea(new Rect(Screen.width/2.2f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].height /2.2f* scale, 100* scale +  mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON2].height* scale, 220* scale, 150* scale));
        
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                GUI.Label(new Rect(1* scale,0,150* scale,150* scale),"2.",instructionsItemBigNumberStyle);
                GUI.Label(new Rect(75,12* scale,120* scale,150* scale),"Find the viewfinder with the target",instructionsItemStyle);
            
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.EndArea();
                
                // Instructions Item 3 Text
                GUILayout.BeginArea(new Rect(Screen.width/1.3f - mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].height /2.2f* scale, 100* scale +  mTextures[(int)TextureName.TEXTURE_INSTRUCTIONS_ICON3].height* scale, 220* scale, 150* scale));
        
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                GUI.Label(new Rect(1* scale,0,150* scale,150* scale),"3.",instructionsItemBigNumberStyle);
                GUI.Label(new Rect(75,12* scale,120* scale,150* scale),"Take Picture",instructionsItemStyle);
            
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.EndArea();
            }
            
            // Draws the button Cancel and Ok
            GUIStyle buttonStyle;
            if (mButtonGUIStyles.TryGetValue("StartButton", out buttonStyle))
            {
                if( GUI.Button(new Rect(5* scale,Screen.height - 70* scale, (Screen.width/2f)-10* scale, 64* scale),"Cancel", buttonStyle))
                {
                    ButtonPressed = true;
                    return ButtonID.CANCEL_PRESSED;
                }
                
                if( GUI.Button(new Rect(Screen.width/2f + 5* scale,Screen.height - 70* scale, Screen.width/2f - 10* scale, 64* scale),"Ok", buttonStyle))
                {
                    ButtonPressed = true;
                    return ButtonID.INSTRUCTIONS_OK_PRESSED;
                }
            }
        }

        return ButtonID.NONE_PRESSED;
    }
    
    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // draws a texture at the center of the screen with a given scale, taking screen space into account
    // used to render the view finder including marks.
    private static void DrawTextureCenteredScaledFromScreenSize(Texture texture, float scale)
    {
        float texWidth = Screen.width * scale;
        float texHeight = Screen.height * scale;
        float left = (Screen.width - texWidth) * 0.5f;
        float top = (Screen.height - texHeight) * 0.5f;
        GUI.DrawTexture(new Rect(left, top, texWidth, texHeight), texture);
    }

    // interpolates color between "from" and "to" by fixed amount scaled by time
    private static Color LerpColorContinuously(Vector4 from, Vector4 to)
    {
        Vector4 diff = to - from;
        Vector4 change = diff.normalized * 0.75f*Time.deltaTime;

        // return either the changed color, or the target color if change is more than difference
        return (change.magnitude > diff.magnitude) ? to : from + change;
    }

    #endregion // PRIVATE_METHODS
}