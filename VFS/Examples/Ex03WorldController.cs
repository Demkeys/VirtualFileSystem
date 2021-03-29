
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace VirtualFileSystem
{
public class Ex03WorldController : UdonSharpBehaviour
{
    public UdonBehaviour FileManager;
    public UdonBehaviour FileSystem;
    const int blockSize = 4;
    const int blockCount = 4;
    byte[] dataBuffer; // The size should always be blockSize*blockCount, for encryption purposes.
    byte[] keyByteArr = new byte[]{0xa1,0x5d,0x1d,0xf2};
    byte[] magicNumberByteArr = new byte[]{0xd2,0x8c,0x4a,0xc6};

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
        dataBuffer = new byte[blockSize*blockCount];

        // Bytes 0-3 are the magic number
        for(int i = 0; i < magicNumberByteArr.Length; i++)
            dataBuffer[i] = magicNumberByteArr[i];

        dataBuffer[4] = System.Convert.ToByte(boolVar);
        
        byte[] intByteArr = Int32ToByteArray(intVar);
        dataBuffer[5] = intByteArr[0];
        dataBuffer[6] = intByteArr[1];
        dataBuffer[7] = intByteArr[2];
        dataBuffer[8] = intByteArr[3];

        dataBuffer[9] = System.Convert.ToByte(charVar);

        dataBuffer[10] = colorVar.r;
        dataBuffer[11] = colorVar.g;
        dataBuffer[12] = colorVar.b;
        dataBuffer[13] = colorVar.a;

        // Encrypt data
        dataBuffer = EncryptByteArray(dataBuffer);

        FileManager.SetProgramVariable("fileDataBuffer",dataBuffer);

    }

    public void LoadDataFromFile()
    {
        dataBuffer = (byte[])FileManager.GetProgramVariable("fileDataBuffer");

        // Decrypt data
        dataBuffer = DecryptByteArray(dataBuffer);

        // Data integrity check. If magic number in dataBuufer is wrong, data has been 
        // tampered with. Let user know, and don't proceed further.
        for(int i = 0; i < magicNumberByteArr.Length; i++)
        {
            if(dataBuffer[i] != magicNumberByteArr[i])
            {
                Debug.Log("File data has been tampered with. Data integrity test failed.");
                return;
            } 
        }

        boolVar = System.Convert.ToBoolean(dataBuffer[4]);

        byte[] intByteArr = new byte[4] {dataBuffer[5],dataBuffer[6],dataBuffer[7],dataBuffer[8]};
        intVar = ByteArrayToInt32(intByteArr);

        charVar = System.Convert.ToChar(dataBuffer[9]);

        colorVar.r = dataBuffer[10];
        colorVar.g = dataBuffer[11];
        colorVar.b = dataBuffer[12];
        colorVar.a = dataBuffer[13];

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

    byte[] EncryptByteArray(byte[] dataArr)
    {
        for(int i = 0; i < blockCount; i++)
        {
            for(int j = 0; j < blockSize; j++)
            {
                if(i==0) dataArr[(i*blockSize)+j] ^= keyByteArr[j];
                else
                {
                    dataArr[(i*blockSize)+j] ^= dataArr[((i*blockSize)-blockSize)+j];
                    dataArr[(i*blockSize)+j] ^= keyByteArr[j];
                    dataArr[j] ^= dataArr[(i*blockSize)+j];
                }
            }
        }
        
        return dataArr;
    }

    byte[] DecryptByteArray(byte[] dataArr)
    {
        for(int i = blockCount-1; i > -1; i--)
        {
            for(int j = 0; j < blockSize; j++)
            {
                if(i==0) dataArr[(i*blockSize)+j] ^= keyByteArr[j];
                else
                {
                    dataArr[j] ^= dataArr[(i*blockSize)+j];
                    dataArr[(i*blockSize)+j] ^= keyByteArr[j];
                    dataArr[(i*blockSize)+j] ^= dataArr[((i*blockSize)-blockSize)+j];
                } 
            }
        }
        return dataArr;
    }
}
}