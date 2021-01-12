// NOTE: This FileSystem was initially designed without keeping Udon's architecture in mind.
// So once the FileSystem was brought into an Udon project, wrapper methods and variables
// had to be made so the FileSystem would work with Udon. The good news is that the FileSystem
// given the current design, it's possible to write your own FileManager if you wish to do so.
// NOTE 2: Some of these variable and method names are a little long. Might refactor in the 
// future. For the time being they are gonna remain as they are.

using UnityEngine;
using UnityEngine.Events;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace VirtualFileSystem
{
public class FileSystem : UdonSharpBehaviour
{
    /* Max no. of bytes the FileSystem can store. Be careful with how big you set this. 4kb is 
    fine. Maybe 8kb as well. Any higher than that could cause lag spikes when importing and 
    exporting FileSystemData. */
    const int FileSystemDataSize = 4096;

    /* File signatures help the file system recognize a file. Every file will have a FileStartSig
    and FileEndSig. If you wish to avoid FileSystemData of other worlds being imported into your
    world's FileSystem, you can change these bytes to something of your choice. But another 
    alternative would be to setup your world's Save and Load logic so it can recognize FileData 
    that belongs to your world instead. For example, including magic numbers in the FileData. */
    byte[] FileStartSig = {0x78,0x74,0x65,0x6a,0x63,0x76}; // xtejcv 
    byte[] FileEndSig = {0x76,0x63,0x6a,0x65,0x74,0x78}; // vcjetx 
    
    byte[] FileSystemData;
    string FileSystemDataString;
    
    // Indices of FileSystemData pointing to beginning of each file.
    int[] FileStartAddresses;

    // Indices of FileSystemData pointing to end of each file.
    int[] FileEndAddresses;

    int FileCount = 0;
    int WritePosition = 0; // Where prev file was written
    int ReadPosition = 0; // Where prev file was read

    // **********************************************************************
    // NOTE: These variables were made so the FileSystem can work with Udon.
    
    // Contains data for file about to be written or file that has been read. 
    // Only file data, no FileStartSig or FileEndSig.
    byte[] FileDataBuffer; 
    
    int SelectedFileIndex = 0;
    int FileWriteReturnCode = 0; // 0=Success, -1=Error
    int FileReadReturnCode = 0; // 0=Success, -1=Error
    int FileDelReturnCode = 0; // 0=Success, -1=Error
    string UnpackedFileSystemData = "";
    int FileSystemDataUnpackReturnCode = 0; // 0=Success, -1=Error
    
    // PackFileSystemData() method's result gets written here.
    // UnpackFileSystemDat(string) method's argument is read from here.
    string PackedFileSystemData = ""; 

    string CompressedFileSystemData = "";
    string DecompressedFileSystemData = ""; // Might not be in use. Confirm and delete later.
    int FileSystemDecompressReturnCode = 0; // 0=Success, -1=Error
    // **********************************************************************
    
    // Start is called before the first frame update
    void Start()
    {
        // Set size for FileSystemData. 
        FileSystemData = new byte[FileSystemDataSize];
        
        FileStartAddresses = new int[1024];
        FileEndAddresses = new int[1024];
    }

    // Update is called once per frame
    void Update()
    {
        // return; 
        // byte[] testByteArr = new byte[]{0x1,0x2,0x3,0x4,0x5,0x6};
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     byte[] bArr1 = new byte[]{0x61,0x62,0x63,0x61,0x62,0x63,0x61,0x62,0x63};
        //     byte[] bArr2 = new byte[]{0x61,0x62,0x63,0x64,0x65,0x66,0x67,0x68};
        //     int i = WriteFileToFileSystem(bArr2);
        //     Debug.Log($"Write Op {IntArrayToString(FileStartAddresses)}");
        //     Debug.Log($"Write Op {IntArrayToString(FileEndAddresses)}");
        //     // Debug.Log(WritePosition);
        //     // i = WriteFileToFileSystem(bArr2);
        //     // Debug.Log(WritePosition);
        //     // i = WriteFileToFileSystem(bArr1);
        //     // Debug.Log(WritePosition);
            
        // }
        // if(Input.GetKeyDown(KeyCode.R))
        // {
        //     int res = ReadFileFromFileSystem(2);
        //     Debug.Log(ReadPosition);
        //     // string s = "";
        //     // for(int i = 0; i < FileDataBuffer.Length; i++)
        //     // {
        //     //     s += $"{System.Convert.ToString(FileDataBuffer[i],16).PadLeft(2,'0')} ";
        //     // }
        //     // Debug.Log(s);
        // }
        // if(Input.GetKeyDown(KeyCode.D))
        // {
        //     int i = DeleteFileFromFileSystem(0);
        //     // Debug.Log(i);
        //     // Debug.Log(FileCount);
        //     Debug.Log($"Del Op {IntArrayToString(FileStartAddresses)}");
        //     Debug.Log($"Del Op {IntArrayToString(FileEndAddresses)}");
            
        // }
        // if(Input.GetKeyDown(KeyCode.S))
        // {
        //     Debug.Log($"FileCount:{FileCount}, WritePosition:{WritePosition}");
        //     // Debug.Log($"Last FileEndAddress:{FileEndAddresses[FileCount-1]}");
            
        //     // for(int j = 0; j < FileCount; j++)
        //     // {
        //     //     Debug.Log($"{FileStartAddresses[j]}/{FileEndAddresses[j]}");
        //     // }
        //     // Debug.Log(PackFileSystemData());
        // }
        // if(Input.GetKeyDown(KeyCode.U))
        // {
        //     string s = "ff 0a ";
        //     int i = UnpackFileSystemData(s);
        //     // Debug.Log(i);
        // }
        // if(Input.GetKeyDown(KeyCode.B))
        // {
        //     UdonGetCompressedFileSystemData();
        //     // string s = CompressFileSystemData();
        //     Debug.Log(CompressedFileSystemData);
        // }
        // if(Input.GetKeyDown(KeyCode.N))
        // {
        //     UdonSetDecompressedFileSystemData();
        //     Debug.Log(FileCount);

        // }

    }

    /* NOTE: The scanning works by iterating over the FileSystemData array, and first looking 
    for a FileStartSig, then a FileEndSig, in that order. If both these are found, a file has 
    been found, so the next iteration jumps to the position in the FileSystemData array where 
    that file ends, and will continue reading from there.
    Data corruption:
    -A file needs a FileStartSig AND FileEndSig to be recognized as a file. If either one is
    missing in a file, the file is considered corrupt and you'll get unexpected behaviour.
    -If a file's FileStartSig is not found, the file won't be recognized as a file at all.
    -If a file's FileEndSig is not found, and another file exists after that file, both the 
    files will be read as one file. This counts as data corruption as well. */
    int ScanForFiles()
    {
        FileStartAddresses = new int[1024];
        FileEndAddresses = new int[1024];
        int count = 0;
        for(int i = 0; i < FileSystemData.Length; i++)
        {
            // Try to match bytes of FileSystemData in the range of (i,i+6) 
            // to bytes of FileStartSig
            if(IsMatchingBytePattern(FileSystemData, i, FileStartSig))
            {
                // If FileStartSig match found...
                // Read FileSystemData bytes in the range(i+6,FileSystemData.Length-1)
                // and look for FileEndSig. This shouldn't be all that wasteful for 
                // performance because once FileEndSig is found we break out of the 
                // loop. If FileStartSig is found but FileEndSig is not found, it will 
                // loop to the end, but that just means that it's corrupted data, not
                // a file. This method is used because we never know the file size 
                // while scanning.
                // i+6 starts iteration from the next byte after FileStartSig.
                for(int j = i+6; j < FileSystemData.Length; j++)
                {
                    if(IsMatchingBytePattern(FileSystemData, j, FileEndSig))
                    {
                        FileStartAddresses[count] = i;
                        FileEndAddresses[count] = j+5;

                        /* Since a file has been found, set i's value to the current file's end
                        address, so that the next iteration will start reading right after the 
                        position in the array where this file ends. */
                        i = FileEndAddresses[count];
                        
                        count++;
                        break;
                    }
                }
            }    
        }
        return count;
    }

    bool IsMatchingBytePattern(
        byte[] bArr, int readPos, byte[] patternByteArr)
    {
        for(int i = 0; i < patternByteArr.Length; i++)
        {
            if(bArr[i+readPos] != patternByteArr[i]) return false;
        }
        return true;
    }

    // Debug function. DO NOT USE IN PROD!
    string ByteArrayToString(byte[] bArr)
    {
        char[] cArr = new char[bArr.Length];
        for(int i = 0; i < bArr.Length; i++)
        {
            cArr[i] = (char)bArr[i];
        }
        string s = new string(cArr);
        return s;
    }

    // Debug function. DO NOT USE IN PROD!
    string IntArrayToString(int[] iArr)
    {
        string res = "";
        for(int i = 0; i < iArr.Length; i++)
        {
            res += $"{iArr[i]} ";
        }
        return res;
    }

    // Creates formatted file containing FileStartSig, file data and FileEndSig.
    // Writes formatted file to file system at the cuurrent write position.
    // Returns 0 if there is enough space in File System.
    // Returns -1 if there isn't enough space in File System.
    // NOTE: Make another imlementation that allows saving at selectedFileIndex.
    public int WriteFileToFileSystem(byte[] fileData)
    {
        // Array length = FileStartSig bytes count + fileData bytes count + FileEndSig bytes count.
        // This way, FileStartSig and FileEndSig arrays can be increased or decreased in future.
        byte[] formattedFileData = new byte[FileStartSig.Length+fileData.Length+FileEndSig.Length]; 
        
        // If formattedFileData size is great than size available, don't continue.
        // Return -1, signalling there was an error.
        if(formattedFileData.Length > (FileSystemData.Length-WritePosition)) return -1;
        // Otherwise, proceed to create and write file...

        int pos = 0;
        // Write FileStartSig
        for(int i = 0; i < FileStartSig.Length; i++)
        {
            formattedFileData[i] = FileStartSig[i];
        }
        // Increment pos by FileStartSig's byte count
        pos += FileStartSig.Length;
        // Write file data
        for(int i = 0; i < fileData.Length; i++)
        {
            formattedFileData[pos+i] = fileData[i];
        }
        // Increment pos by fileData's byte count
        pos += fileData.Length;
        // Write FileEndSig
        for(int i = 0; i < FileEndSig.Length; i++)
        {
            formattedFileData[pos+i] = FileEndSig[i];
        }

        // Write whole formatted file to File System
        for(int i = 0; i < formattedFileData.Length; i++)
        {
            FileSystemData[WritePosition+i] = formattedFileData[i];
        }

        FileCount = ScanForFiles();
        WritePosition += formattedFileData.Length;

        // Return 0, signaling a successful write operation.
        return 0;
    }

    // Read file at fileIndex. 
    // Returns 0 if file found.
    // Returns -1 if file not found.
    public int ReadFileFromFileSystem(int fileIndex)
    {
        // If fileIndex is negative or greater than length of FileStartAddresses array,
        // or if FileCount is zero, fileIndex doesn't exist, so return -1.
        if(fileIndex < 0 || fileIndex > (FileCount-1) || FileCount == 0) return -1;
        // Otherwise, continue...
        
        ReadPosition = FileStartAddresses[fileIndex];

        // Position after FileStartSig ends.
        int fileDataStartIndex = FileStartAddresses[fileIndex]+FileStartSig.Length;
        // Position before FileEndSig starts.
        int fileDataEndIndex = FileEndAddresses[fileIndex]-FileEndSig.Length;
        
        int fileDataSize = (fileDataEndIndex-fileDataStartIndex)+1;
        FileDataBuffer = new byte[fileDataSize];

        for(int i = 0; i < FileDataBuffer.Length; i++)
            FileDataBuffer[i] = FileSystemData[fileDataStartIndex+i];

        // Read operation succeeded, return 0.
        return 0;
    }

    // Deletes file at fileIndex. 
    // Returns 0 if file found.
    // Returns -1 if file not found.
    public int DeleteFileFromFileSystem(int fileIndex)
    {
        // If fileIndex is negative or greater than length of FileStartAddresses array,
        // or if FileCount is zero, fileIndex doesn't exist, so return -1.
        if(fileIndex < 0 || fileIndex > (FileCount-1) || FileCount == 0) return -1;
        // Otherwise, continue...
        // Debug.Log("Point 0");


        // Calculate size of file to be deleted.
        int delFileSize = (FileEndAddresses[fileIndex]-FileStartAddresses[fileIndex])+1;

        // If fileIndex is last file
        if(fileIndex == (FileCount-1))
        {
            for(int i = FileStartAddresses[fileIndex]; i < FileSystemData.Length; i++)
            {
                // Zero out the data.
                FileSystemData[i] = 0;
            }
        }
        // If fileIndex is not last file
        else
        {
            for(int i = FileStartAddresses[fileIndex+1]; i < FileSystemData.Length; i++)
            {
                // Shift data backwards by copying to new location, then zero out data at 
                // the old location.
                FileSystemData[i-delFileSize] = FileSystemData[i];
                FileSystemData[i] = 0;
            }
        }

        FileCount = ScanForFiles();
        // WritePosition = FileSystemData[FileEndAddresses[FileCount-1]]+1; 
        WritePosition = FileCount == 0 ? 0 : (FileEndAddresses[FileCount-1]+1); 

        return 0;
    }

    // Converts FileSystemData array into a formatted string and returns
    // the result. The format is space-separated hex numbers. 
    public string PackFileSystemData()
    {
        string res = "";
        for(int i = 0; i < FileSystemData.Length; i++)
            res += $"{System.Convert.ToString(FileSystemData[i],16).PadLeft(2,'0')} ";
        return res;
    }

    // Takes packedFileSystemData as argument and unpacks the data. Then sets
    // FileSystemData using the unpacked data.
    // Returns status. 0=Success, -1=Error
    public int UnpackFileSystemData(string packedFSData)
    {
        string[] sArr = packedFSData.Split(
            new char[]{' '}, FileSystemDataSize, StringSplitOptions.RemoveEmptyEntries );
        // Debug.Log(sArr.Length);
        
        // If packedFileSystemData is bigger than size of FileSystemData, return -1.
        if(sArr.Length > FileSystemData.Length || sArr.Length == 0) return -1;
        
        /* This is a method of sanitizing the string. This is necessary because for 
        whatever reason the string in the last cell of the array always contains 
        a non-parseable character, and that later throws an exception when converting
        to a byte. 
        UPDATE: This line is not necessary anymore because of byte.TryParse(), but leave
        this line commented for now, don't delete. */
        // sArr[sArr.Length-1] = sArr[sArr.Length-1].Trim(' ');
        // Debug.Log(sArr.Length);
        
        FileSystemData = new byte[FileSystemDataSize];
        for(int i = 0; i < FileSystemData.Length; i++)
        {
            if(i < sArr.Length)
            {
                /* Try to parse sArr[i] to a byte. If parse succeeds, it returns parsed value,
                if parse fails, returns 0. The value is being stored in a separate byte variable
                because trying to store directly in FileSystemData[i] doesn't work. This has
                something to do with the inner workings of Udon/U#. */
                byte b = 0;
                byte.TryParse(sArr[i],NumberStyles.AllowHexSpecifier,(IFormatProvider)null,out b);
                FileSystemData[i] = b;
                // Debug.Log(FileSystemData[i]);
            } 
            else FileSystemData[i] = 0;
        }

        // Scan FileSystemData for files since the data has changed.
        FileCount = ScanForFiles();
        
        /* Set WritePosition. We have to perform checks here to account for the possibility
        that the file system data being supplied might be corrupted. If corrupted data or
        and empty string is supplied for unpacking, FileCount will be 0. If that's the case
        trying to read file addresses will throw an exception. So if FileCount is 0 or less,
        set WritePosition to 0. */
        WritePosition = FileCount > 0 ? FileEndAddresses[FileCount-1]+1 : 0;

        return 0;
    }

    // Compresses FileSystemData. First encodes FileSystemData to base64 string. Then 
    // encoded base64 string with RLE encoding to shorten it. Returns encoded string.
    string CompressFileSystemData()
    {
        // Base64 encoded FileSystemData
        string b64str = System.Convert.ToBase64String(FileSystemData);
        
        string final_string = ""; // Stores final RLE encoded string.
        string count_string = ""; // Contains RLE counts.
        int len_counter = 1; // Stores current char repetition count.

        // Iterate over b64str's chars.
        for(int i = 0; i < b64str.Length; i++)
        {
            // If not iterating on the last b64str char AND if the succeeding char
            // is equal to the current char...
            if(i != b64str.Length-1 && b64str[i] == b64str[i+1])
            {
                //...that's a repetition, so increment len_counter.
                len_counter++;
            }
            // Else, current char has no succeeding repetitions.
            else
            {
                // If len_counter is more than one, the current char is a repitition. 
                if(len_counter > 1)
                {
                    // Add current char to final_string along with a ^ to signify it has 
                    // repetitions. Add len_counter to count_string to register char count.
                    final_string += $"{b64str[i]}^";
                    count_string += $"{len_counter}#";
                }
                // Else, add current char to final_string.
                else
                {
                    final_string += $"{b64str[i]}";
                }
                // Since current char has no succeeding repetitions, reset len_counter to 1.
                len_counter = 1;
            }
        }

        // Trim # from the end of final_string.
        if(count_string[count_string.Length-1] == '#')
            count_string = count_string.TrimEnd(new char[]{'#'});
        
        // Join final_string and count_string, separated by @.
        final_string += $"@{count_string}";

        return final_string;
    }

    // 
    int DecompressFileSystemData(string compString)
    {
        char[] sym = new char[]{'@','#'};
        string unpacked_data_string = "";
        
        // Split compString into data string and count string using @ separator
        string[] split_compString = compString.Split(sym[0]);
        
        // If there aren't exactly two strings, there's an error, return -1.
        if(split_compString.Length != 2) return -1;

        string dataString = split_compString[0];

        // Split count string into individual count strings using # separator
        string[] split_countString = new string[0];
        split_countString = split_compString[1].Split(sym[1]);

        // Convert individual count strings in ints
        int[] split_countStringInt = new int[split_countString.Length];
        for(int i = 0; i < split_countString.Length; i++)
        {
            int num = 0;
            Int32.TryParse(split_countString[i], NumberStyles.Integer, null, out num);
            split_countStringInt[i] = num;
        }

        // Unpack dataString. 
        // Iterate over dataString's chars.
        int arrayCounter = 0;
        for(int i = 0; i < dataString.Length; i++)
        {
            // If not iterating on the last dataString char AND if the succeeding char
            // is equal to ^, the current char has repetitions.
            if(i != dataString.Length-1 && dataString[i+1] == '^')
            {
                // 
                if(arrayCounter < split_countStringInt.Length)
                {
                    for(int j = 0; j < split_countStringInt[arrayCounter]; j++)
                    {
                        unpacked_data_string += $"{dataString[i]}";
                    }
                    i++;
                    arrayCounter++;
                }
            }
            else
            {
                unpacked_data_string += $"{dataString[i]}";
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        // Before decoding the base64 string, check if the string is base64. If a non-base64 
        // string is passed to FromBase64String a runtime exception will occur, halting the 
        // UdonBehaviour. If string is non-base64, return -1.
        
        // Check if base64 string is of expected length based on FileSystemData array's 
        // length. Every 3 bytes gets encoded to 4 bytes. Algorithm: 4*(3/n), where n is the 
        // multiplier needed to multiply by 4 and get base64 string's expected length. If 
        // byte array's lenght is not a multiple of 3, use Ceil to get the smallest integer 
        // greater than (byte array length/3).
        int expectedLen = Mathf.CeilToInt((float)FileSystemDataSize/3f);
        expectedLen *= 4;
        if(unpacked_data_string.Length != expectedLen) return -1;

        // If previous test passed, check if all dataString's chars are base64 chars.
        char[] b64chars = new char[65] {
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R',
            'S','T','U','V','W','X','Y','Z','a','b','c','d','e','f','g','h','i','j',
            'k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','0','1',
            '2','3','4','5','6','7','8','9','+','/','='
        };
        for(int i = 0; i < unpacked_data_string.Length; i++)
        {
            bool is_b64_char = false;
            for(int j = 0; j < b64chars.Length; j++)
            {
                if(unpacked_data_string[i] == b64chars[j]) { is_b64_char = true; break; }
                else is_b64_char = false;
            }
            if(is_b64_char == false) return -1;
        }
        /////////////////////////////////////////////////////////////////////////////////////

        FileSystemData = System.Convert.FromBase64String(unpacked_data_string);
        if(FileSystemData.Length < FileSystemDataSize)
        {
            for(int i = FileSystemData.Length; i < FileSystemDataSize; i++)
            {
                FileSystemData[i] = 0;
            }
        }

        FileCount = ScanForFiles();
        WritePosition = FileCount > 0 ? FileEndAddresses[FileCount-1]+1 : 0;

        return 0;
    }


    #region Udon Helper Methods 

    //////////////////////////////////////////////////////////////////////////////////////
    // Helper methods to use FileSystem with Udon. This FileSystem was initially written
    // without keeping Udon's architecture in mind. So a lot of the methods take arguments
    // and return values. Udon doesn't currently support that, so these methods help with
    // that issue. You set variable values, and then call the appropriate method. 
    // Addtionally you can read the corresponding return code for the operation you've 
    // just done, to find out if the operation was successful or not.
    //////////////////////////////////////////////////////////////////////////////////////
    public void UdonWriteFileToFileSystem() 
    { FileWriteReturnCode = WriteFileToFileSystem(FileDataBuffer); }
    public void UdonReadFileFromFileSystem() 
    {
        FileReadReturnCode = ReadFileFromFileSystem(SelectedFileIndex);
        if(FileReadReturnCode == -1) FileDataBuffer = new byte[0];
    }
    public void UdonDeleteFileFromFileSystem() 
    {
        FileDelReturnCode = DeleteFileFromFileSystem(SelectedFileIndex);
    }
    public void UdonGetPackedFileSystemData() 
    { 
        PackedFileSystemData = PackFileSystemData(); 
        // Debug.Log(PackedFileSystemData);
    }
    public void UdonSetUnpackedFileSystemData() 
    { 
        FileSystemDataUnpackReturnCode = UnpackFileSystemData(PackedFileSystemData);
    }
    public void UdonGetCompressedFileSystemData()
    {
        CompressedFileSystemData = CompressFileSystemData();
    }
    public void UdonSetDecompressedFileSystemData()
    {
        FileSystemDecompressReturnCode = DecompressFileSystemData(CompressedFileSystemData);
    }

    #endregion
    
}
}












