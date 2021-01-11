
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Ex01WorldController : UdonSharpBehaviour
{
    public UdonBehaviour FileManager;
    public UdonBehaviour FileSystem;
    const int dataBufferSize = 6;
    byte[] dataBuffer;
    bool boolVar = true;
    int intVar = -24;
    char charVar = 'a';

    void Start()
    {
        dataBuffer = new byte[dataBufferSize];
        
    }

    void Update()
    {

    }

    public void SaveDataToFile()
    {
        /* Data is packed as follows:
        dataBuffer[0] = boolVar
        dataBuffer[1-4] = intVar
        dataBuffer[5] = charVar
        */
        dataBuffer = new byte[dataBufferSize];

        // Convert bool to byte. True = 1, False = 0.
        dataBuffer[0] = System.Convert.ToByte(boolVar);

        /* Convert int to byte[4]. This won't always be necessary. If your int value is 
        between 0-255, that value can be fit into one byte, so you can type cast your 
        int to a byte. But if the value is negative or greater than 255, that cannot be 
        stored in one byte, so use bitshifting and bitmasking to split the int into 4 
        bytes. */
        byte[] bArr = Int32ToByteArray(intVar);
        dataBuffer[1] = bArr[0];
        dataBuffer[2] = bArr[1];
        dataBuffer[3] = bArr[2];
        dataBuffer[4] = bArr[3];
        
        // Convert char to byte. 
        dataBuffer[5] = System.Convert.ToByte(charVar);
        
        FileManager.SetProgramVariable("fileDataBuffer", dataBuffer);
    }

    public void LoadDataFromFile()
    {
        dataBuffer = (byte[])FileManager.GetProgramVariable("fileDataBuffer");
        string s = "";
        for(int i = 0; i < dataBuffer.Length; i++)
        {
            s += $"{System.Convert.ToString(dataBuffer[i],16).PadLeft(2,'0')} ";
        }
        Debug.Log(s);
        boolVar = System.Convert.ToBoolean(dataBuffer[0]);
        
        byte[] bArr = new byte[4]{dataBuffer[1],dataBuffer[2],dataBuffer[3],dataBuffer[4]};
        intVar = ByteArrayToInt32(bArr);
        charVar = System.Convert.ToChar(dataBuffer[5]);
        Debug.Log($"bool:{boolVar},int:{intVar},char:{charVar}");
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


// int maskedNum = 0;
        // maskedNum = intVar >> 24;
        // dataBuffer[1] = (byte)(maskedNum & 0xff);
        // maskedNum = intVar >> 16;
        // dataBuffer[2] = (byte)(maskedNum & 0xff);
        // maskedNum = intVar >> 8;
        // dataBuffer[3] = (byte)(maskedNum & 0xff);
        // maskedNum = intVar & 0xff;
        // dataBuffer[4] = (byte)(maskedNum & 0xff);


// int maskedNum = 0;
        // intVar = 0;
        // maskedNum = dataBuffer[1] & 0xff;
        // maskedNum <<= 24;
        // intVar |= maskedNum;
        // maskedNum = dataBuffer[2] & 0xff;
        // maskedNum <<= 16;
        // intVar |= maskedNum;
        // maskedNum = dataBuffer[3] & 0xff;
        // maskedNum <<= 8;
        // intVar |= maskedNum;
        // maskedNum = dataBuffer[4] & 0xff;
        // intVar |= maskedNum;



    // maskedNum = bArr[0] & 0xff;
    //     maskedNum <<= 24;
    //     res |= maskedNum;
    //     maskedNum = bArr[1] & 0xff;
    //     maskedNum <<= 16;
    //     res |= maskedNum;
    //     maskedNum = bArr[2] & 0xff;
    //     maskedNum <<= 8;
    //     res |= maskedNum;
    //     maskedNum = bArr[3] & 0xff;
    //     res |= maskedNum;