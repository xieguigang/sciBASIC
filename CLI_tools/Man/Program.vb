#Region "Microsoft.VisualBasic::8bfb47ac79a65bcb54354adaac2f97c5, sciBASIC#\CLI_tools\Man\Program.vb"

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

    '   Total Lines: 29
    '    Code Lines: 22
    ' Comment Lines: 2
    '   Blank Lines: 5
    '     File Size: 1.15 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module Program

    Sub Main()
        Dim CommandLine As CommandLine.CommandLine = App.CommandLine

        If CommandLine.IsNullOrEmpty Then
            Call Console.WriteLine("Usage for man command: " & vbCrLf & "     man <program>")
            Return
        End If

        Dim AssemblyName As String = CommandLine.Name
        If Not AssemblyName.FileExists Then
            AssemblyName = AssemblyName & ".exe"
            If Not AssemblyName.FileExists Then
                Call Console.WriteLine($"Could not found the assembly command which is named ""{CommandLine.Name}""! (Is it right?  {FileIO.FileSystem.GetFileInfo(AssemblyName).FullName.ToFileURL })")
            End If
        End If

        If CommandLine.Parameters.IsNullOrEmpty Then
            '默认显示出全部帮助信息
        Else
            '只显示出指定的命令对象
            Dim commandName As String = CommandLine.Parameters.First
            Call Console.WriteLine($"Help for command ""{commandName}"" in program ""{AssemblyName.ToFileURL}"":")
            Call Console.WriteLine()
        End If

    End Sub
End Module
