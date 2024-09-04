using System;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
public static class PopupUtils
{
    // ---------------------------------------------------------------------------------------------------
    // [UIElements runtime popup example? - Unity Forum]
    // https://forum.unity.com/threads/uielements-runtime-popup-example.827565/
    public static Action ShowPopup(VisualElement rootElementForPopup, VisualElement content)
    {
        //Create visual element for popup
        var popupContainer = new VisualElement();
        popupContainer.style.position = new StyleEnum<Position>(Position.Absolute);
        popupContainer.style.top      = 0;
        popupContainer.style.left     = 0;
        popupContainer.style.flexGrow = new StyleFloat(1);
        popupContainer.style.height   = new StyleLength(new Length(100, LengthUnit.Percent));
        popupContainer.style.width    = new StyleLength(new Length(100, LengthUnit.Percent));

 
        //Popup background is button so that the popup is closed when the player
        //clicks anywhere outside the popup.
        var backgroundButton = new Button();
        backgroundButton.style.position         = new StyleEnum<Position>(Position.Absolute);
        backgroundButton.style.top              = 0;
        backgroundButton.style.left             = 0;
        backgroundButton.style.flexGrow         = new StyleFloat(1);
        backgroundButton.style.height           = new StyleLength(new Length(100, LengthUnit.Percent));
        backgroundButton.style.width            = new StyleLength(new Length(100, LengthUnit.Percent));
        backgroundButton.style.opacity          = new StyleFloat(0f);
        backgroundButton.style.backgroundColor  = new StyleColor(Color.red);
        backgroundButton.text = string.Empty;
 
        backgroundButton.clickable.clicked += ClosePopup;
        
        var dim = new VisualElement();
        dim.style.top               = 0;
        dim.style.left              = 0;
        dim.style.height            = new StyleLength(new Length(100, LengthUnit.Percent));
        dim.style.width             = new StyleLength(new Length(100, LengthUnit.Percent));
        dim.style.opacity           = new StyleFloat(0.1f);
        dim.style.backgroundColor   = new StyleColor(Color.black);
        popupContainer.Add(dim);
        popupContainer.Add(backgroundButton);
 
        float           heightInPercents = 80.0f;
            
        //Set content size
        content.style.height   = new StyleLength(new Length(heightInPercents, LengthUnit.Percent));
 
        //Show popupContent in the middle of the screen
        content.style.position = new StyleEnum<Position>(Position.Absolute);
 
        float topAndBottom     = (100f - heightInPercents) / 2f;
        content.style.top      = new StyleLength(new Length(topAndBottom, LengthUnit.Percent));
        content.style.bottom   = new StyleLength(new Length(topAndBottom, LengthUnit.Percent));
 
        content.style.width    = new StyleLength(new Length(200, LengthUnit.Pixel));
        // popupContent.style.left     = new StyleLength(new Length(leftAndRight, LengthUnit.Percent));
        content.style.right    = new StyleLength(new Length(60, LengthUnit.Pixel));
 
        popupContainer.Add(content);
 
        rootElementForPopup.Add(popupContainer);
        
        void ClosePopup() {
            rootElementForPopup.Remove(popupContainer);
        }
        return ClosePopup;
    }
    
    
    /// ---------------------------------------------------------------------------------------------------
    // [get the rootVisualElement in the 'Editor' - Unity Forum]
    // https://forum.unity.com/threads/get-the-rootvisualelement-in-the-editor.799335/
    public static VisualElement FindRootEditor<T>(T visualElement, int iteration = 100) where T : VisualElement
    {
        VisualElement prevParent = null;
 
        while (iteration > 0)
        {
            VisualElement parent;
            if (prevParent == null)
                parent = visualElement.parent;
            else
                parent = prevParent.parent;
 
            if (parent != null) {
                prevParent = parent;
            }
            else
            {
                foreach(var f in prevParent!.Children()) {
                    if(f.name.Contains("rootVisualContainer")) {
                        return f;
                    }
                }
                return null;
            }
            iteration--;
        }
        return null;
    }
}
}