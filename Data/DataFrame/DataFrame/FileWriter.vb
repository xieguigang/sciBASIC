#Region "Microsoft.VisualBasic::17abeb043230ec6c68d117768de6e6b7, Data\DataFrame\DataFrame\FileWriter.vb"

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

    '   Total Lines: 37
    '    Code Lines: 29 (78.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.17 KB


    ' Module FileWriter
    ' 
    '     Sub: (+2 Overloads) WriteCsv
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports any = Microsoft.VisualBasic.Scripting

Public Module FileWriter

    <Extension>
    Public Sub WriteCsv(df As DataFrame, file As String)
        Using s As Stream = file.Open(FileMode.OpenOrCreate, doClear:=True)
            Call df.WriteCsv(s)
        End Using
    End Sub

    <Extension>
    Public Sub WriteCsv(df As DataFrame, file As Stream)
        Dim s As New StreamWriter(file, Encoding.UTF8)
        Dim names = df.featureNames
        Dim cols = names.Select(Function(c) df(c).Getter).ToArray
        Dim rownames = df.rownames
        Dim row As String() = New String(cols.Length) {}

        Call s.WriteLine("," & names.Select(Function(si) $"""{si}""").JoinBy(","))

        For i As Integer = 0 To rownames.Length - 1
            row(0) = rownames(i)

            For offset As Integer = 0 To cols.Length - 1
                row(offset + 1) = any.ToString(cols(offset)(i), "")
            Next

            Call s.WriteLine(row.Select(Function(si) $"""{si}""").JoinBy(","))
        Next

        Call s.Flush()
    End Sub
End Module
