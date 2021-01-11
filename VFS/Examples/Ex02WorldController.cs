
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class Ex02WorldController : UdonSharpBehaviour
{
    public UdonBehaviour FileManager;
    public UdonBehaviour FileSystem;
    const int dataBufferSize = 10;
    byte[] dataBuffer;

    // Data Variables
    bool boolVar = false;
    int intVar = 0;
    char charVar = '-';
    Color32 colorVar;

    // UI Variables
    public Toggle boolVarToggle;
    public Slider intVarSlider;
    public Text intVarText;
    public InputField charVarInputField;
    public Slider ColorVarRSlider;
    public Slider ColorVarGSlider;
    public Slider ColorVarBSlider;
    public Image ColorVarPrevImage;

    void Start()
    {
        colorVar = new Color32((byte)0,(byte)0,(byte)0,(byte)255);
        OnBoolVarToggleValueChanged();
        OnIntVarSliderValueChanged();
        OnCharVarInputFieldValueChanged();
        OnColorVarRSliderValueChanged();
        OnColorVarGSliderValueChanged();
        OnColorVarBSliderValueChanged();
    }

    void Update()
    {

    }

    public void OnBoolVarToggleValueChanged()
    {
        boolVar = boolVarToggle.isOn;
    }

    public void OnIntVarSliderValueChanged()
    {
        intVar = (int)intVarSlider.value;
        intVarText.text = intVar.ToString();
    }

    public void OnCharVarInputFieldValueChanged()
    {
        charVar = charVarInputField.text.Length > 0 ? charVarInputField.text[0] : '-';
    }

    public void OnColorVarRSliderValueChanged()
    {
        colorVar.r = (byte)ColorVarRSlider.value;
        ColorVarPrevImage.color = colorVar;
    }

    public void OnColorVarGSliderValueChanged()
    {
        colorVar.g = (byte)ColorVarGSlider.value;
        ColorVarPrevImage.color = colorVar;
    }
    
    public void OnColorVarBSliderValueChanged()
    {
        colorVar.b = (byte)ColorVarBSlider.value;
        ColorVarPrevImage.color = colorVar;
    }

    public void SaveDataToFile()
    {
        // Debug.Log($"bool:{boolVar},int:{intVar},char:{charVar},color:{colorVar}");
        dataBuffer = new byte[dataBufferSize];
        dataBuffer[0] = System.Convert.ToByte(boolVar);
        
        byte[] intByteArr = Int32ToByteArray(intVar);
        dataBuffer[1] = intByteArr[0];
        dataBuffer[2] = intByteArr[1];
        dataBuffer[3] = intByteArr[2];
        dataBuffer[4] = intByteArr[3];

        dataBuffer[5] = System.Convert.ToByte(charVar);

        dataBuffer[6] = colorVar.r;
        dataBuffer[7] = colorVar.g;
        dataBuffer[8] = colorVar.b;
        dataBuffer[9] = colorVar.a;

        FileManager.SetProgramVariable("fileDataBuffer",dataBuffer);

    }

    public void LoadDataFromFile()
    {
        dataBuffer = (byte[])FileManager.GetProgramVariable("fileDataBuffer");

        boolVar = System.Convert.ToBoolean(dataBuffer[0]);

        byte[] intByteArr = new byte[4] {dataBuffer[1],dataBuffer[2],dataBuffer[3],dataBuffer[4]};
        intVar = ByteArrayToInt32(intByteArr);

        charVar = System.Convert.ToChar(dataBuffer[5]);

        colorVar.r = dataBuffer[6];
        colorVar.g = dataBuffer[7];
        colorVar.b = dataBuffer[8];
        colorVar.a = dataBuffer[9];

        Debug.Log($"bool:{boolVar},int:{intVar},char:{charVar},color:{colorVar}"); 
        UpdateUIComponents();
    }

    void UpdateUIComponents()
    {
        boolVarToggle.isOn = boolVar;
        intVarSlider.value = intVar;
        charVarInputField.text = charVar.ToString();
        ColorVarRSlider.value = (int)colorVar.r;
        ColorVarGSlider.value = (int)colorVar.g;
        ColorVarBSlider.value = (int)colorVar.b;
    }

    int ByteArrayToInt32(byte[] bArr)
    {
        if(bArr.Length != 4) return 0;
        int res = 0;
        for(int i = 0; i < bArr.Length; i++)
        {
            int maskedNum = 0;
            maskedNum = bArr[i] & 0xff;
            maskedNum <<= (8*((bArr.Length-1)-i));
            res |= maskedNum;
        }
        return res;
    }

    byte[] Int32ToByteArray(int num)
    {
        byte[] res = new byte[4];
        for(int i = 0; i < res.Length; i++)
        {
            int maskedNum = 0;
            maskedNum = num >> (8*((res.Length-1)-i));
            res[i] = (byte)(maskedNum & 0xff);
        }
        return res;        
    }
}
