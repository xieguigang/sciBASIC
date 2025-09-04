#Region "Microsoft.VisualBasic::688ba6c548734ba1b715968e1a8984ea, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\struct\Matrix.vb"

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

    '   Total Lines: 211
    '    Code Lines: 185 (87.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 26 (12.32%)
    '     File Size: 8.17 KB


    '     Class Matrix
    ' 
    '         Properties: ToValue
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [get], add, columns, load, rows
    '                   ToString
    ' 
    '         Sub: [set], initializeGaussian, initializeNunif, initializeUnif, output
    '              releaseMemory, rescaleByRow, truncate, truncate_row
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.util
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

Namespace GraphEmbedding.struct

    Public Class Matrix
        Private pData As Double()() = Nothing
        Private iNumberOfRows As Integer
        Private iNumberOfColumns As Integer

        Public Sub New()
        End Sub

        Public Sub New(iRows As Integer, iColumns As Integer)
            pData = New Double(iRows - 1)() {}
            For i = 0 To iRows - 1
                pData(i) = New Double(iColumns - 1) {}
                For j = 0 To iColumns - 1
                    pData(i)(j) = 0.0
                Next
            Next
            iNumberOfRows = iRows
            iNumberOfColumns = iColumns
        End Sub

        Public Overridable Function rows() As Integer
            Return iNumberOfRows
        End Function

        Public Overridable Function columns() As Integer
            Return iNumberOfColumns
        End Function

        Public Overrides Function ToString() As String
            Return $"[rows:{rows()}, columns:{columns()}]"
        End Function

        Public Overridable Function [get](i As Integer, j As Integer) As Double
            If i < 0 OrElse i >= iNumberOfRows Then
                Throw New Exception("get error in Matrix: RowID out of range")
            End If
            If j < 0 OrElse j >= iNumberOfColumns Then
                Throw New Exception("get error in Matrix: ColumnID out of range")
            End If
            Return pData(i)(j)
        End Function

        Public Overridable Sub [set](i As Integer, j As Integer, dValue As Double)
            If i < 0 OrElse i >= iNumberOfRows Then
                Throw New Exception("set error in Matrix: RowID out of range")
            End If
            If j < 0 OrElse j >= iNumberOfColumns Then
                Throw New Exception("set error in Matrix: ColumnID out of range")
            End If
            pData(i)(j) = dValue
        End Sub

        Public Overridable WriteOnly Property ToValue As Double
            Set(value As Double)
                For i = 0 To iNumberOfRows - 1
                    For j = 0 To iNumberOfColumns - 1
                        pData(i)(j) = value
                    Next
                Next
            End Set
        End Property

        Public Function add(i As Integer, j As Integer, dValue As Double) As Double
            If i < 0 OrElse i >= iNumberOfRows Then
                Throw New Exception("add error in Matrix: RowID out of range")
            End If
            If j < 0 OrElse j >= iNumberOfColumns Then
                Throw New Exception("add error in Matrix: ColumnID out of range")
            End If
            pData(i)(j) += dValue
            Return pData(i)(j)
        End Function

        Public Overridable Sub initializeUnif()
            Dim rd As Random = New Random(123)
            For i = 0 To iNumberOfRows - 1
                For j = 0 To iNumberOfColumns - 1
                    Dim dValue As Double = rd.NextDouble()
                    pData(i)(j) = 2.0 * dValue - 1.0
                Next
            Next
        End Sub

        Public Overridable Sub initializeNunif()
            Dim rd As Random = New Random(123)
            Dim dBnd = std.Sqrt(6.0) / std.Sqrt(iNumberOfRows + iNumberOfColumns)
            For i = 0 To iNumberOfRows - 1
                For j = 0 To iNumberOfColumns - 1
                    Dim dValue As Double = rd.NextDouble()
                    pData(i)(j) = (2.0 * dValue - 1.0) * dBnd
                Next
            Next
        End Sub

        Public Overridable Sub initializeGaussian()
            Dim rd As Random = New Random(123)
            For i = 0 To iNumberOfRows - 1
                For j = 0 To iNumberOfColumns - 1
                    Dim dValue As Double = rd.NextGaussian()
                    pData(i)(j) = dValue
                Next
            Next
        End Sub

        Public Overridable Sub rescaleByRow()
            For i = 0 To iNumberOfRows - 1
                Dim dNorm = 0.0
                For j = 0 To iNumberOfColumns - 1
                    dNorm += pData(i)(j) * pData(i)(j)
                Next
                dNorm = std.Sqrt(dNorm)
                If dNorm <> 0.0 Then
                    For j = 0 To iNumberOfColumns - 1
                        pData(i)(j) *= std.Min(1.0, 1.0 / dNorm)
                    Next
                End If
            Next
        End Sub

        Public Overridable Sub truncate(dLower As Double, dUpper As Double)
            For i = 0 To iNumberOfRows - 1
                For j = 0 To iNumberOfColumns - 1
                    Dim dValue = pData(i)(j)
                    If pData(i)(j) < dLower Then
                        dValue = dLower
                    End If
                    If pData(i)(j) > dUpper Then
                        dValue = dUpper
                    End If
                    pData(i)(j) = dValue
                Next
            Next
        End Sub

        Public Overridable Sub truncate_row(dLower As Double, dUpper As Double, i As Integer)
            For j = 0 To iNumberOfColumns - 1
                Dim dValue = pData(i)(j)
                If pData(i)(j) < dLower Then
                    dValue = dLower
                End If
                If pData(i)(j) > dUpper Then
                    dValue = dUpper
                End If
                pData(i)(j) = dValue
            Next
        End Sub

        Public Overridable Function load(fnInput As String) As Boolean
            Dim reader As StreamReader = New StreamReader(New FileStream(fnInput, FileMode.Open, FileAccess.Read), Encoding.UTF8)

            Dim line As Value(Of String) = ""
            line = reader.ReadLine()
            Dim first_line = StringSplitter.RemoveEmptyEntries(StringSplitter.split(":; ", line))
            If iNumberOfRows <> Integer.Parse(first_line(1)) OrElse iNumberOfColumns <> Integer.Parse(first_line(3)) Then
                Throw New Exception("load error in Matrix: row/column number incorrect")
            End If

            Dim iRowID = 0
            While Not (line = reader.ReadLine()) Is Nothing
                Dim tokens = StringSplitter.RemoveEmptyEntries(StringSplitter.split(vbTab & " ", line))
                If iRowID < 0 OrElse iRowID >= iNumberOfRows Then
                    Throw New Exception("load error in Matrix: RowID out of range")
                End If
                If tokens.Length <> iNumberOfColumns Then
                    Throw New Exception("load error in Matrix: ColumnID out of range")
                End If
                For iColumnID = 0 To tokens.Length - 1
                    pData(iRowID)(iColumnID) = Double.Parse(tokens(iColumnID))
                Next
                iRowID += 1
            End While

            reader.Close()
            Return True
        End Function

        Public Overridable Sub output(fnOutput As String)
            Dim writer As StreamWriter = New StreamWriter(fnOutput.Open(FileMode.OpenOrCreate, doClear:=True), Encoding.UTF8)

            writer.Write("iNumberOfRows: " & iNumberOfRows.ToString() & "; iNumberOfColumns: " & iNumberOfColumns.ToString() & vbLf)
            For i = 0 To iNumberOfRows - 1
                writer.Write((pData(i)(0).ToString() & " ").Trim())
                For j = 1 To iNumberOfColumns - 1
                    writer.Write(vbTab & (pData(i)(j).ToString() & " ").Trim())
                Next
                writer.Write(vbLf)
            Next

            writer.Close()
        End Sub

        Public Overridable Sub releaseMemory()
            For i = 0 To iNumberOfRows - 1
                pData(i) = Nothing
            Next
            pData = Nothing
            iNumberOfRows = 0
            iNumberOfColumns = 0
        End Sub
    End Class

End Namespace
