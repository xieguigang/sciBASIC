#Region "Microsoft.VisualBasic::798648205ca3a3474306bad9ebb8f2a5, vs_solutions\dev\VisualStudio\Ngen\Registry.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module RegistryUtils
    ' 
    '     Function: RegisterCOMDll, RegisterExtensions, WriteToRegistry
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.Win32

#If netcore5 = 0 Then


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

#End If
