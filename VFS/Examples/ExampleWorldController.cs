
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ExampleWorldController : UdonSharpBehaviour
{
    public UdonBehaviour FileManager;
    public UdonBehaviour FileSystem;

    void Start()
    {
        
    }

    void Update()
    {

        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     string s = "";
        //     byte[] bArr = (byte[])FileSystem.GetProgramVariable("FileSystemData");
        //     Debug.Log(System.Convert.ToBase64String(bArr));
        // }

    }

    public void SaveData()
    {
        byte[] bArr = new byte[8];
        for(int i = 0; i < bArr.Length; i++)
            bArr[i] = (byte)UnityEngine.Random.Range(0,255);
        FileManager.SetProgramVariable("fileDataBuffer", bArr);
    }

    public void LoadData()
    {
        byte[] bArr = (byte[])FileManager.GetProgramVariable("fileDataBuffer");
        string s = "";
        for(int i = 0; i < bArr.Length; i++)
        {
            s += $"{System.Convert.ToString(bArr[i],16).PadLeft(2,'0')} ";
        }
        Debug.Log(s);
        // byte[] b = System.BitConverter.GetBytes('c');
        // byte[] b = new byte[]{0x1a,0x2,0xc1,0xd};
        // float f = System.BitConverter.ToSingle(b, 0);
        // Debug.Log(bArr.Length); 
    }

}
