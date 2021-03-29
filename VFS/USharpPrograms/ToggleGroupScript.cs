
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace VirtualFileSystem
{
public class ToggleGroupScript : UdonSharpBehaviour
{
    public Toggle[] toggles;
    public int selectedToggleIndex;
    int Test = 5;

    void Start()
    {
        
    }

    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.J))
        //     ResetToggleGroup();
    }

    public void OnToggleValueChanged()
    {
        for(int i = 0; i < toggles.Length; i++)
        {
            if(toggles[i].isOn == true)
            {
                selectedToggleIndex = i;
                break;
            }
        }
        // Debug.Log(GetSelectedToggle().gameObject.name);
        // Debug.Log(selectedToggleIndex);
    }


    public Toggle GetToggle(uint index) 
    {
        if(index < toggles.Length) return toggles[index];
        // if(index > -1 && index < toggles.Length) return toggles[index];
        else return null;
    }

    public Toggle GetSelectedToggle() 
    { return selectedToggleIndex < toggles.Length ? toggles[selectedToggleIndex] : null; }

    void ResetToggleGroup()
    {
        toggles[0].isOn = true;
        selectedToggleIndex = 0;
    }
}
}