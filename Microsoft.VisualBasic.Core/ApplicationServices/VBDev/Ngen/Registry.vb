#Region "Microsoft.VisualBasic::64625ae6702b8f28df8c85a6224313e0, Microsoft.VisualBasic.Core\ApplicationServices\VBDev\Ngen\Registry.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module RegistryUtils
    ' 
    '         Function: RegisterCOMDll, RegisterExtensions, WriteToRegistry
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.Win32

Namespace ApplicationServices

    Public Module RegistryUtils

        ''' <summary>
        ''' Register a .NET dll file as a COM component.(将某一个.NET语言所编写的DLL文件注册为COM组件)
        ''' </summary>
        ''' <param name="COM_Dll"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("RegAsm")>
        Public Function RegisterCOMDll(<Parameter("COM.Dll", "The file path of the DLL file which will be registered as a COM component.")> COM_Dll As String) As Boolean
            Dim Asm As Assembly = Assembly.LoadFile(COM_Dll)
            Dim regAsm As New RegistrationServices
            Dim bResult As Boolean = regAsm.RegisterAssembly(Asm, AssemblyRegistrationFlags.SetCodeBase)
            Return bResult
        End Function

        ''' <summary>
        ''' ### Registering Extensions of the .NET Framework
        ''' 
        ''' > https://msdn.microsoft.com/en-us/library/ee722096.aspx
        ''' </summary>
        ''' <param name="PATH$"></param>
        Public Function RegisterExtensions(PATH$, company$) As String
            Try
                Call WriteToRegistry(RegistryHive.LocalMachine, $"SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\{company}\", "", PATH)
            Catch ex As Exception
                Return ex.ToString
            End Try

            Return "success!"
        End Function

        Public Function WriteToRegistry(ParentKeyHive As RegistryHive, SubKeyName$, ValueName$, Value As Object) As Boolean

            ' DEMO USAGE
            ' Dim bAns As Boolean
            ' bAns = WriteToRegistry(RegistryHive.LocalMachine, "SOFTWARE\MyCompany\MyProgram\", "ProgramHasRunBefore", "Y")
            ' Debug.WriteLine("Registry Write Successful: " & bAns)

            Dim objSubKey As RegistryKey
            Dim sException$ = Nothing
            Dim objParentKey As RegistryKey
            Dim bAns As Boolean

            Select Case ParentKeyHive
                Case RegistryHive.ClassesRoot
                    objParentKey = Registry.ClassesRoot
                Case RegistryHive.CurrentConfig
                    objParentKey = Registry.CurrentConfig
                Case RegistryHive.CurrentUser
                    objParentKey = Registry.CurrentUser
                Case RegistryHive.DynData
#Disable Warning
                    objParentKey = Registry.DynData
#Enable Warning
                Case RegistryHive.LocalMachine
                    objParentKey = Registry.LocalMachine
                Case RegistryHive.PerformanceData
                    objParentKey = Registry.PerformanceData
                Case RegistryHive.Users
                    objParentKey = Registry.Users
                Case Else
                    Throw New Exception(ParentKeyHive.ToString)
            End Select

            'Open 
            objSubKey = objParentKey.OpenSubKey(SubKeyName, True)

            'create if doesn't exist
            If objSubKey Is Nothing Then
                objSubKey = objParentKey.CreateSubKey(SubKeyName)
            End If

            objSubKey.SetValue(ValueName, Value)
            bAns = True

            Return bAns
        End Function
    End Module
End Namespace
