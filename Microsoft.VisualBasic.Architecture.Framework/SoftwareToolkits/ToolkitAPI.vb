Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Scripting.EntryPointMetaData

<[Namespace]("Software.Toolkits")>
Module ToolkitAPI

    <Command("Release.Notes.Editor.Open")>
    Public Function Edit(Optional path As String = "") As Boolean
        Using Editor As UpdatesEditor = New UpdatesEditor
            If Not String.IsNullOrEmpty(path) Then
                Call Editor.LoadDocument(path)
            End If

            Call Editor.ShowDialog()
        End Using

        Return True
    End Function

    ''' <summary>
    ''' Register a .NET dll file as a COM component.(将某一个.NET语言所编写的DLL文件注册为COM组件)
    ''' </summary>
    ''' <param name="COM_Dll"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("RegAsm")>
    Public Function RegisterCOMDll(<ParameterAlias("COM.Dll", "The file path of the DLL file which will be registered as a COM component.")> COM_Dll As String) As Boolean
        Dim Asm As Assembly = Assembly.LoadFile(COM_Dll)
        Dim regAsm As New RegistrationServices
        Dim bResult As Boolean = regAsm.RegisterAssembly(Asm, AssemblyRegistrationFlags.SetCodeBase)
        Return bResult
    End Function
End Module
