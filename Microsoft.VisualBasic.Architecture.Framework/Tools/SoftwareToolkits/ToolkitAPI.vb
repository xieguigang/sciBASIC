#Region "Microsoft.VisualBasic::602c45213de636781a70e3643247cbef, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\ToolkitAPI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace SoftwareToolkits

    <[Namespace]("Software.Toolkits")>
    Module ToolkitAPI

        <ExportAPI("Release.Notes.Editor.Open")>
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
        <ExportAPI("RegAsm")>
        Public Function RegisterCOMDll(<Parameter("COM.Dll", "The file path of the DLL file which will be registered as a COM component.")> COM_Dll As String) As Boolean
            Dim Asm As Assembly = Assembly.LoadFile(COM_Dll)
            Dim regAsm As New RegistrationServices
            Dim bResult As Boolean = regAsm.RegisterAssembly(Asm, AssemblyRegistrationFlags.SetCodeBase)
            Return bResult
        End Function
    End Module
End Namespace
