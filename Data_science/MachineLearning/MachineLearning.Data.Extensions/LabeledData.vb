#Region "Microsoft.VisualBasic::f712bb8e689b8cc55b79dfb1b222f3aa, Data_science\MachineLearning\MachineLearning.Data.Extensions\LabeledData.vb"

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

    '   Total Lines: 61
    '    Code Lines: 51 (83.61%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (16.39%)
    '     File Size: 2.35 KB


    ' Module LabeledData
    ' 
    '     Function: LoadLabelData, SaveLabelData
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Serialization.Bencoding

Public Module LabeledData

    <Extension>
    Public Function SaveLabelData(data As IEnumerable(Of EntityClusterModel), buffer As Stream) As Boolean
        Dim bin As New BinaryDataWriter(buffer) With {.ByteOrder = ByteOrder.BigEndian}
        Dim matrix = data.ToArray
        Dim featureNames As String() = matrix(Scan0).Properties.Keys.ToArray

        Call bin.Write(featureNames.Length)
        Call bin.Write(matrix.Length)
        Call bin.Write(featureNames.ToBEncodeString, BinaryStringFormat.ZeroTerminated)

        For Each row As EntityClusterModel In matrix
            Call bin.Write(row.ID, BinaryStringFormat.ZeroTerminated)
            Call bin.Write(row.Cluster, BinaryStringFormat.ZeroTerminated)
            Call bin.Write(row(featureNames))
            Call bin.Write(CByte(0))
        Next

        Return True
    End Function

    <Extension>
    Public Iterator Function LoadLabelData(buffer As Stream) As IEnumerable(Of EntityClusterModel)
        Dim bin As New BinaryDataReader(buffer) With {.ByteOrder = ByteOrder.BigEndian}
        Dim featureSize As Integer = bin.ReadInt32
        Dim matrixRows As Integer = bin.ReadInt32
        Dim featureNames As String() = bin _
            .ReadString(BinaryStringFormat.ZeroTerminated) _
            .BDecode _
            .First _
            .ToList _
            .Select(Function(b) b.ToString) _
            .ToArray

        For i As Integer = 0 To matrixRows - 1
            Dim id As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
            Dim cluster As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
            Dim v As Double() = bin.ReadDoubles(featureSize)
            Dim props As New Dictionary(Of String, Double)

            bin.ReadByte()

            For idx As Integer = 0 To featureNames.Length - 1
                props(featureNames(idx)) = v(idx)
            Next

            Yield New EntityClusterModel With {
                .Cluster = cluster,
                .ID = id,
                .Properties = props
            }
        Next
    End Function
End Module
