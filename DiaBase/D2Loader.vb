﻿Module D2Loader

    Private TargetProcessHandle As Integer
    Private pfnStartAddr As Integer
    Private pszLibFileRemote As String
    Private TargetBufferSize As Integer

    Public Const PROCESS_VM_READ = &H10
    Public Const TH32CS_SNAPPROCESS = &H2
    Public Const MEM_COMMIT = 4096
    Public Const PAGE_READWRITE = 4
    Public Const PROCESS_CREATE_THREAD = (&H2)
    Public Const PROCESS_VM_OPERATION = (&H8)
    Public Const PROCESS_VM_WRITE = (&H20)
    Dim DLLFileName As String
    Public Declare Function ReadProcessMemory Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpBaseAddress As Integer,
    ByVal lpBuffer As String,
    ByVal nSize As Integer,
    ByRef lpNumberOfBytesWritten As Integer) As Integer

    Public Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" (
    ByVal lpLibFileName As String) As Integer

    Public Declare Function VirtualAllocEx Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpAddress As Integer,
    ByVal dwSize As Integer,
    ByVal flAllocationType As Integer,
    ByVal flProtect As Integer) As Integer

    Public Declare Function WriteProcessMemory Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpBaseAddress As Integer,
    ByVal lpBuffer As String,
    ByVal nSize As Integer,
    ByRef lpNumberOfBytesWritten As Integer) As Integer

    Public Declare Function GetProcAddress Lib "kernel32" (
    ByVal hModule As Integer, ByVal lpProcName As String) As Integer

    Private Declare Function GetModuleHandle Lib "Kernel32" Alias "GetModuleHandleA" (
    ByVal lpModuleName As String) As Integer

    Public Declare Function CreateRemoteThread Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpThreadAttributes As Integer,
    ByVal dwStackSize As Integer,
    ByVal lpStartAddress As Integer,
    ByVal lpParameter As Integer,
    ByVal dwCreationFlags As Integer,
    ByRef lpThreadId As Integer) As Integer

    Public Declare Function OpenProcess Lib "kernel32" (
    ByVal dwDesiredAccess As Integer,
    ByVal bInheritHandle As Integer,
    ByVal dwProcessId As Integer) As Integer

    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (
    ByVal lpClassName As String,
    ByVal lpWindowName As String) As Integer

    Declare Function CloseHandle Lib "kernel32" Alias "CloseHandle" (ByVal hObject As Integer) As Integer

    Dim ExeName As String = IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath)

    Public Sub Inject(ByVal x)

        TargetProcessHandle = OpenProcess(PROCESS_CREATE_THREAD Or PROCESS_VM_OPERATION Or PROCESS_VM_WRITE, False, x)
        pszLibFileRemote = "Clobot.dll"
        pfnStartAddr = GetProcAddress(GetModuleHandle("Kernel32"), "LoadLibraryA")
        TargetBufferSize = 1 + Len(pszLibFileRemote)
        Dim Rtn As Integer
        Dim LoadLibParamAdr As Integer
        LoadLibParamAdr = VirtualAllocEx(TargetProcessHandle, 0, TargetBufferSize, MEM_COMMIT, PAGE_READWRITE)
        Rtn = WriteProcessMemory(TargetProcessHandle, LoadLibParamAdr, pszLibFileRemote, TargetBufferSize, 0)
        CreateRemoteThread(TargetProcessHandle, 0, 0, pfnStartAddr, LoadLibParamAdr, 0, 0)
        CloseHandle(TargetProcessHandle)

    End Sub

    Public Sub WriteLoaderFile(ByVal x)
        If ItemObjects(x).MulePass = "Unknown" Then Return

        Try
            Dim WriteFile As System.IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(AppSettings.InstallPath + "\Scripts\Starter.js", False)

            WriteFile.WriteLine("var ingame = false;")
            WriteFile.WriteLine()
            WriteFile.WriteLine(" while (ingame == false){")
            WriteFile.WriteLine("   if(GetClientState() == 2)    {ingame = true;}")
            WriteFile.WriteLine("   SetTitle(""DiaBase Mule"");")
            WriteFile.WriteLine()
            WriteFile.WriteLine("   switch(GetOOGLocation())    {")
            WriteFile.WriteLine("       case 18:")
            WriteFile.WriteLine("           ClickScreen(0, 10, 10);")
            WriteFile.WriteLine("           Sleep(1000);")
            WriteFile.WriteLine("           break;")

            WriteFile.WriteLine("       case 8:")
            WriteFile.WriteLine("           SelectRealm(""" & ItemObjects(x).ItemRealm & """);")
            WriteFile.WriteLine("           Sleep(1000);")
            WriteFile.WriteLine("           break;")

            WriteFile.WriteLine("       case 9:")
            WriteFile.WriteLine("           Login(""" & ItemObjects(x).MuleAccount & """,""" & ItemObjects(x).MulePass & """);")
            WriteFile.WriteLine("           Sleep(1000);")
            WriteFile.WriteLine("           break;")

            WriteFile.WriteLine("       case 12:")
            ' WriteFile.WriteLine("           SelectChar(""SugarLips"");")
            WriteFile.WriteLine("           SelectChar(""" & ItemObjects(x).MuleName & """);")
            WriteFile.WriteLine("           Sleep(1000);")
            WriteFile.WriteLine("           break;")


            ' WriteFile.WriteLine("       case 20:")
            ' WriteFile.WriteLine("           ClickScreen(0, 300, 272);") ' normal difficulty
            ' WriteFile.WriteLine("           Sleep(1000);")
            ' WriteFile.WriteLine("           break;")

            WriteFile.WriteLine("        default:")
            WriteFile.WriteLine("           Sleep(1000);")
            WriteFile.WriteLine("    }")
            WriteFile.WriteLine("}")


            WriteFile.Close()
        Catch ex As Exception
            Return
        End Try

        loadD2(x)

    End Sub

    Public Sub loadD2(ByVal x)

        Dim key As Microsoft.Win32.RegistryKey

        key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Blizzard Entertainment\Diablo II")
        If key Is Nothing Then
            Main.RichTextBox1.Text = "Error reading registry"
            Return
        End If

        Dim D2Path As String = key.GetValue("InstallPath").ToString()
        If D2Path = Nothing Then Return
        D2Path = D2Path & "\Game.exe"

        ' load routine


        Dim myProcess As Process = New Process
        myProcess.StartInfo.FileName = D2Path
        Dim ApArgs As String = ""
        ApArgs = ApArgs & "-w"
        myProcess.StartInfo.Arguments = ApArgs
        'need someway to multi load TODO		 
        myProcess.Start()
        Dim AppName = Process.GetProcessesByName("Game")
        If AppName.Length = 0 Then
            Return
        End If
        Inject(AppName(0).Id)

    End Sub


End Module