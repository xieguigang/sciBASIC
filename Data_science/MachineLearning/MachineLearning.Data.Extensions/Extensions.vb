#Region "Microsoft.VisualBasic::785de2126341df562b89a6b78bf9932f, sciBASIC#\Data_science\MachineLearning\MachineLearning.Data.Extensions\Extensions.vb"

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

    '   Total Lines: 46
    '    Code Lines: 35
    ' Comment Lines: 5
    '   Blank Lines: 6
    '     File Size: 1.64 KB


    ' Module Extensions
    ' 
    '     Function: GetInput
    ' 
    ' Class QTableDump
    ' 
    '     Sub: Dump, Save
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.QLearning.DataModel
Imports Microsoft.VisualBasic.Text
Imports row = Microsoft.VisualBasic.Data.csv.IO.DataSet

Public Module Extensions

    ''' <summary>
    ''' 从csv文件数据之中读取和当前的数据集一样的元素顺序的向量用于预测分析
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetInput(dataset As DataSet, data As row) As Double()
        Return dataset _
            .NormalizeMatrix _
            .names _
            .Select(Function(key) data(key)) _
            .ToArray
    End Function
End Module

Public Class QTableDump

    ReadOnly __buffer As New Dictionary(Of IndexCurve)

    Public Sub Dump(table As IQTable, iteration As Integer)
        For Each o In table.Table.Values
            For i As Integer = 0 To table.ActionRange - 1
                Dim uid As String = $"[{i}] {o.EnvirState}"
                If Not __buffer.ContainsKey(uid) Then
                    Call __buffer.Add(uid, New IndexCurve(uid))
                End If
                Call __buffer(uid).Properties.Add(iteration, o.Qvalues(i))
            Next
        Next
    End Sub

    Public Sub Save(path As String)
        Call __buffer.Values.SaveTo(path, Encodings.ASCII)
    End Sub
End Class
