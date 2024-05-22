#Region "Microsoft.VisualBasic::c50cd7b161793324a73956d91c37c893, CLI_tools\rundll32\Program.vb"

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


    ' Code Statistics:

    '   Total Lines: 106
    '    Code Lines: 86 (81.13%)
    ' Comment Lines: 3 (2.83%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (16.04%)
    '     File Size: 4.50 KB


    ' Module Program
    ' 
    '     Function: CreateInstance, GetArg, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Extensions

'rundll32 <assembly_path> <commandline_arguments>
'rundll32 --help
'rundll32 --man <assembly_path>

Module Program

    Public Function CreateInstance(AssemblyPath As String, Name As String) As Interpreter
        Dim AssemblyType As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(AssemblyPath)
        Dim EntryType As Type = GetType(RunDllEntryPoint)
        Dim LQuery = (From [Module] As System.Reflection.TypeInfo
                      In AssemblyType.DefinedTypes
                      Let attributes As Object() = [Module].GetCustomAttributes(EntryType, inherit:=False)
                      Where Not attributes Is Nothing AndAlso attributes.Count = 1
                      Select Entry = DirectCast(attributes.First, RunDllEntryPoint), [Module]).ToArray

        If LQuery.IsNullOrEmpty Then '没有找到执行入口点
            Return Nothing
        End If

        If LQuery.Count > 1 Then

            If Not String.IsNullOrEmpty(Name) Then
                Dim Find = (From Entry In LQuery Where String.Equals(Name, Entry.Entry.Namespace, StringComparison.OrdinalIgnoreCase) Select Entry.Module).ToArray
                If Find.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Dim Type As Type = Find.First
                    Return New Interpreter(Type)
                End If
            Else
                GoTo First
            End If

        Else
First:      Dim Type As Type = LQuery.First.Module
            Return New Interpreter(Type)
        End If
    End Function

    Public Function Main() As Integer
        Dim strCommand As String = App.Command
        Dim Tokens As String() = CLITools.GetTokens(strCommand)

        If String.IsNullOrEmpty(strCommand) OrElse String.Equals(Tokens.First.ToLower, "--help") Then
            Dim strMessage As String =
                "rundll32 <assembly_path> <commandline_arguments>" & vbCrLf & vbCrLf &
 _
                "Trying this command to get all of the commands information in the target assembly file:" & vbCrLf &
                "       rundll32 --man <assembly_path>"
            Call Console.WriteLine(strMessage)
        ElseIf String.Equals(Tokens.First.ToLower, "--man") Then
            Dim AssemblyFile As String = Mid(strCommand, 6).Trim

            If FileIO.FileSystem.FileExists(AssemblyFile) Then
                Dim Interpreter = CreateInstance(IO.Path.GetFullPath(AssemblyFile), "")

                If Interpreter Is Nothing Then
                    Call Console.WriteLine("""{0}"" is not a standard .NET assembly or commandline interpreter interface is not declared!", IO.Path.GetFullPath(AssemblyFile))
                    Return -1
                Else
                    Call Console.WriteLine(Interpreter.SDKdocs)
                    Return 0
                End If
            Else
                Call Console.WriteLine("""{0}"" is not exists on the filesystem!", IO.Path.GetFullPath(AssemblyFile))
                Return -1
            End If
        Else
            Dim AssemblyFile As String = Tokens.First

            If FileIO.FileSystem.FileExists(AssemblyFile) Then
                Dim strArgument As String = GetArg(strCommand, AssemblyFile)
                Dim Interpreter = CreateInstance(IO.Path.GetFullPath(AssemblyFile), "")

                If Interpreter Is Nothing Then
                    Call Console.WriteLine("""{0}"" is not a standard .NET assembly or commandline interpreter interface is not declared!", IO.Path.GetFullPath(AssemblyFile))
                    Return -1
                Else
                    Return Interpreter.Execute(strArgument)
                End If
            Else
                Call Console.WriteLine("""{0}"" is not exists on the filesystem!", IO.Path.GetFullPath(AssemblyFile))
                Return -1
            End If
        End If

        Return 0
    End Function

    Private Function GetArg(Command As String, Assembly As String) As String
        Dim offset As Integer

        If InStr(Command, """" & Assembly & """") = 1 Then
            offset = 1 + 2
        Else
            offset = 1
        End If

        Dim args As String = Mid(Command, Len(Assembly) + offset).Trim
        Return args
    End Function
End Module
