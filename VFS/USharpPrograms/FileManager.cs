
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using VirtualFileSystem;

namespace VirtualFileSystem
{
public class FileManager : UdonSharpBehaviour
{
    public UdonBehaviour fileSystem;
    public UdonBehaviour fileItemsToggleGroupScript;
    public Image[] fileItemSelectionBorderImages;
    public Image[] fileItemDiasbledImages;
    public Toggle[] fileItemBodyToggles;
    public Button PrevButton;
    public Button NextButton;
    public Text PageNoText;
    public InputField fileSystemDataInputField;
    public Text FileNameText;
    public Text FileSizeText;
    public Text OpStatusText;
    public Button LoadFileBtn;
    public Button SaveNewFileBtn;
    public Button DeleteFileBtn;
    int selectedFileItemIndex = 0; // selected FileItem (0 to filePerPage).
    int totalFileCount = 0; // Total file count read from FileSystem.
    int totalPages = 0;
    const int filesPerPage = 15;
    int currentPageNo = 0; // Starts at 0
    byte[] fileDataBuffer;
    int selectedFileIndex = 0; // Index of File, not FileItem.
    public Toggle[] impExpFormatToggleArr;
    public Color ToggleOnColor;
    public Color ToggleOffColor;
    ColorBlock toggleColorBlock;
    int impExpFormat = 0; // 0 = Plaint Text, 1 = Compressed

    void Start()
    {
        fileDataBuffer = new byte[4]; // Arbitrary initialization.
        // selectedFileItemIndex = 0;
        // selectedFileIndex = 0;
        UpdateFileManager();

        toggleColorBlock = ColorBlock.defaultColorBlock;
        OnImpExpFormatChanged();
    }

    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     // int i = (int)toggleGroupUScript.GetProgramVariable("selectedToggleIndex");
        //     // string i = (string)fileSystem.GetProgramVariable("Test");
        //     // Debug.Log(i);
        // }

        // if(Input.GetKeyDown(KeyCode.L))
        // {

        // }
        
    }

    void UpdateFileManager()
    {
        // Calculate total pages
        float totalPagesF = (float)totalFileCount/(float)filesPerPage;
        totalPages = Mathf.CeilToInt(totalPagesF);

        // 
        // for(int i = 0; i < fileItemSelectionBorderImages.Length; i++)
        // {
        //     Color col = fileItemSelectionBorderImages[i].color;
        //     if(i == selectedFileItemIndex) col.a = 1; 
        //     else col.a = 0;
        //     fileItemSelectionBorderImages[i].color = col;
        // }
        
        // If totalFileCount is not 0, calculate PageNo. Else, just display "1/1".
        PageNoText.text = totalFileCount != 0 ? $"{currentPageNo+1}/{totalPages}" : $"1/1";
            
        PrevButton.interactable = currentPageNo > 0 ? true : false;
        NextButton.interactable = currentPageNo < totalPages-1 ? true : false;
        LoadFileBtn.interactable = totalFileCount > 0 ? true : false;

        // Comment this line out until Delete frontend logic is fixed
        DeleteFileBtn.interactable = totalFileCount > 0 ? true : false;

        for(int i = 0; i < filesPerPage; i++)
        {
            int offset = (currentPageNo*filesPerPage)+i;
            fileItemDiasbledImages[i].gameObject.SetActive( 
                offset < totalFileCount ? false : true);
            fileItemBodyToggles[i].interactable = offset < totalFileCount ? true : false;

            Color col = fileItemSelectionBorderImages[i].color;
            if(i == selectedFileItemIndex) col.a = 1; 
            else col.a = 0;
            fileItemSelectionBorderImages[i].color = col;
        }
    }

    public void OnFileItemSelected()
    {
        selectedFileItemIndex = 
            (int)fileItemsToggleGroupScript.GetProgramVariable("selectedToggleIndex");
        // Debug.Log((currentPageNo*filesPerPage)+selectedFileItemIndex);
        selectedFileIndex = (currentPageNo*filesPerPage)+selectedFileItemIndex;
        // Debug.Log(selectedFileIndex);
        UpdateFileManager();
    }

    public void PrevPage()
    {
        currentPageNo--;
        selectedFileItemIndex = 0; // Reset selected file
        selectedFileIndex = (currentPageNo*filesPerPage)+selectedFileItemIndex;
        UpdateFileManager();
    }

    public void NextPage()
    {
        currentPageNo++;
        selectedFileItemIndex = 0; // Reset selected file
        selectedFileIndex = (currentPageNo*filesPerPage)+selectedFileItemIndex;
        UpdateFileManager();
    }

    public void ImportFileSystemData()
    {
        int retCode = -1;

        // Plain Text
        if(impExpFormat == 0)
        {
            fileSystem.SetProgramVariable("PackedFileSystemData", fileSystemDataInputField.text);
            fileSystem.SendCustomEvent("UdonSetUnpackedFileSystemData");
            retCode = (int)fileSystem.GetProgramVariable("FileSystemDataUnpackReturnCode");
        }
        // Compressed
        else if(impExpFormat == 1)
        {
            fileSystem.SetProgramVariable("CompressedFileSystemData", fileSystemDataInputField.text);
            fileSystem.SendCustomEvent("UdonSetDecompressedFileSystemData");
            retCode = (int)fileSystem.GetProgramVariable("FileSystemDecompressReturnCode");
        }

        // Debug.Log(retCode);
        
        OpStatusText.text = retCode == 0 ? "Import Success" : "Import Error";
        totalFileCount = (int)fileSystem.GetProgramVariable("FileCount");
        
        // Since a new FileSystem has been imported, reset some values.
        selectedFileItemIndex = 0;
        currentPageNo = 0;
        selectedFileIndex = (currentPageNo*filesPerPage)+selectedFileItemIndex;
        
        UpdateFileManager();
        fileSystemDataInputField.DeactivateInputField(); 
        // Debug.Log(totalFileCount);
    }

    public void ExportFileSystemData()
    {
        string exportData = "";
        if(impExpFormat == 0)
        {
            fileSystem.SendCustomEvent("UdonGetPackedFileSystemData");
            exportData = (string)fileSystem.GetProgramVariable("PackedFileSystemData");
        }
        else if(impExpFormat == 1)
        {
            fileSystem.SendCustomEvent("UdonGetCompressedFileSystemData");
            exportData = (string)fileSystem.GetProgramVariable("CompressedFileSystemData");
        }
        // Debug.Log(temp);
        fileSystemDataInputField.text = exportData;
    }

    public void SaveNewFile()
    {
        // Random data for debugging.
        // fileDataBuffer = new byte[Random.Range(0,11)];
        // for(int i = 0; i < fileDataBuffer.Length; i++)
        // {
        //     fileDataBuffer[i] = (byte)Random.Range(0,256);
        // }
        fileSystem.SetProgramVariable("FileDataBuffer", fileDataBuffer);
        fileSystem.SendCustomEvent("UdonWriteFileToFileSystem");
        int retCode = (int)fileSystem.GetProgramVariable("FileWriteReturnCode");
        OpStatusText.text = retCode == 0 ? "Write Success" : "Write Error";
        totalFileCount = (int)fileSystem.GetProgramVariable("FileCount");
        UpdateFileManager();
        // Debug.Log($"SaveRetCode:{retCode},FileCount:{totalFileCount}");
    }

    public void LoadFile()
    {
        fileSystem.SetProgramVariable("SelectedFileIndex", selectedFileIndex);
        
        fileSystem.SendCustomEvent("UdonReadFileFromFileSystem");
        fileDataBuffer = (byte[])fileSystem.GetProgramVariable("FileDataBuffer");
        int retCode = (int)fileSystem.GetProgramVariable("FileReadReturnCode");
        
        FileNameText.text = retCode == 0 ? $"File #{selectedFileIndex+1}" : "None";
        FileSizeText.text = retCode == 0 ? $"{fileDataBuffer.Length+12} B" : "0 B";
        OpStatusText.text = retCode == 0 ? "Read Success" : "Read Error";
        
        // string s = $"FileNo:{selectedFileIndex} | ";
        // for(int i = 0; i < fileDataBuffer.Length; i++)
        // {
        //     s += $"{System.Convert.ToString(fileDataBuffer[i],16).PadLeft(2,'0')} ";
        // }
        // Debug.Log(s);
    }

    /* NOTES:
    -When an file is deleted...
        -selectedFileItemIndex resets to 0.
        -If file being deleted is the only remaining file in the current page, deleting it 
        decrement will leave the page blanks, decrement currentPageNo by one and call 
        UpdateFileManager(). 
        -
    */
    public void DeleteFile()
    {
        // Run Delete operation on backend.
        fileSystem.SetProgramVariable("SelectedFileIndex", selectedFileIndex);
        fileSystem.SendCustomEvent("UdonDeleteFileFromFileSystem");

        int retCode = (int)fileSystem.GetProgramVariable("FileDelReturnCode");
        OpStatusText.text = retCode == 0 ? "Delete Success" : "Error";
        totalFileCount = (int)fileSystem.GetProgramVariable("FileCount");


        //////////////////////////////////////////////////////////////
        // totalFileCount has been updated after file delete op. Recalculate total page count
        // based on the current totalFileCount. 
        // Figure out if the deleted file was the last file on the page. If so, that would leave
        // the page blank, so decrement the current page number. Calculate what total pages
        // would be if totalFileCount was one less than it's actual value.
        float totalPagesF = (float)totalFileCount/(float)filesPerPage;
        totalPages = Mathf.CeilToInt(totalPagesF);
        if(currentPageNo > 0 && currentPageNo == totalPages) currentPageNo--;
        //////////////////////////////////////////////////////////////

        // Set selection to first fileItem of the current page.
        selectedFileItemIndex = 0;
        // fileItemsToggleGroupScript.SendCustomEvent("ResetToggleGroup");
        // selectedFileItemIndex = 
        //     (int)fileItemsToggleGroupScript.GetProgramVariable("selectedToggleIndex");
        selectedFileIndex = (currentPageNo*filesPerPage)+selectedFileItemIndex;
        
        UpdateFileManager();

        // Debug.Log($"DelRetCode:{retCode},FileCount:{totalFileCount}");
        // Debug.Log($"ToggleGroupVal:{retCode}");
        // Debug.Log($"selectedFileIndex:{selectedFileIndex},selectedFileItemIndex:{selectedFileItemIndex}");
    }

    public void ClearInputField()
    {
        fileSystemDataInputField.text = "";
    }

    // Method called when the Import/Export options toggles are changed.
    public void OnImpExpFormatChanged()
    {
        // Iterate over impExpFormatToggleArr.
        for(int i = 0; i < impExpFormatToggleArr.Length; i++)
        {
            UpdateToggleColorBlock(impExpFormatToggleArr[i].isOn);

            // Set toggle colors based on whether it's on or off.
            impExpFormatToggleArr[i].colors = toggleColorBlock;

            // Set import/export format to index of whichever toggle is on.
            if(impExpFormatToggleArr[i].isOn) impExpFormat = i;
        }

    }

    void UpdateToggleColorBlock(bool toggleVal)
    {
        toggleColorBlock.normalColor = toggleVal ? ToggleOnColor : ToggleOffColor;
        toggleColorBlock.highlightedColor = toggleVal ? ToggleOnColor : ToggleOffColor;
        toggleColorBlock.pressedColor = toggleVal ? ToggleOnColor : ToggleOffColor;
    }

    void Test()
    {
        
    }
}
}