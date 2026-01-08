#Region "Microsoft.VisualBasic::5c6ffdd808c81d3192e64f92d2553b04, www\axel\Program.vb"

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

    '   Total Lines: 34
    '    Code Lines: 25 (73.53%)
    ' Comment Lines: 2 (5.88%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (20.59%)
    '     File Size: 1.16 KB


    ' Module Program
    ' 
    '     Function: Download, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.Net.WebClient

<CLI>
Module Program

    <STAThread>
    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.Command, executeFile:=AddressOf Download)
    End Function

    ' url/files.txt [--filename <save_filename>] [--download_to <directory_path>]
    Private Function Download(target As String, args As CommandLine) As Integer
        Dim hashset As HashHelper = HashHelper.Load($"{App.ProductProgramData}/downloads.db")

        If target.FileExists Then
            Dim downloads As String = args("--download_to")

            ' is local file of target download file list
            For Each url As String In target.ReadAllLines
                Call New Axel(hashset).Download(url, $"{downloads}/{url.FileName}").Wait()
            Next
        Else
            Dim filename As String = args("--filename")

            If filename.StringEmpty() Then
                filename = $"./{target.FileName}"
            End If

            Call New Axel(hashset).Download(target, filename).Wait()
        End If

        Return 0
    End Function
End Module
