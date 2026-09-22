#Region "Microsoft.VisualBasic::300f61c3ce68c525b2f56c547b3040fb, Data\DataFrame\DataFrame\FileWriter.vb"

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

    '   Total Lines: 58
    '    Code Lines: 37 (63.79%)
    ' Comment Lines: 12 (20.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (15.52%)
    '     File Size: 2.05 KB


    ' Module FileWriter
    ' 
    '     Function: WriteCsv
    ' 
    '     Sub: WriteCsv
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports any = Microsoft.VisualBasic.Scripting

Public Module FileWriter

    ''' <summary>
    ''' save dataframe object as csv file
    ''' </summary>
    ''' <param name="df">dataframe should contains the row names values</param>
    ''' <param name="file"></param>
    ''' <param name="blank"></param>
    <Extension>
    Public Function WriteCsv(df As DataFrame, file As String, Optional blank As String = "") As Boolean
        Try
            Using s As Stream = file.Open(FileMode.OpenOrCreate, doClear:=True)
                Call df.WriteCsv(s, blank)
            End Using

            Return True
        Catch ex As Exception
            Call App.LogException(ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' save dataframe object as csv file
    ''' </summary>
    ''' <param name="df">dataframe should contains the row names values</param>
    ''' <param name="file"></param>
    ''' <param name="blank"></param>
    <Extension>
    Public Sub WriteCsv(df As DataFrame, file As Stream, Optional blank As String = "")
        Dim s As New StreamWriter(file, Encoding.UTF8)
        Dim names As String() = df.featureNames
        Dim cols = names.Select(Function(c) df(c).Getter).ToArray
        Dim rownames As String() = df.rownames
        Dim row As String() = New String(cols.Length) {}

        Call s.WriteLine("," & names.Select(Function(si) $"""{si}""").JoinBy(","))

        For i As Integer = 0 To rownames.Length - 1
            row(0) = rownames(i)

            For offset As Integer = 0 To cols.Length - 1
                row(offset + 1) = any.ToString(cols(offset)(i), null:=blank)
                row(offset + 1) = If(row(offset + 1).StringEmpty, blank, row(offset + 1))
            Next

            Call s.WriteLine(New RowObject(row).AsLine)
        Next

        Call s.Flush()
    End Sub
End Module
