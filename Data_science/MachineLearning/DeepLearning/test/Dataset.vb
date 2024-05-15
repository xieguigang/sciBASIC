#Region "Microsoft.VisualBasic::0f114190040d385a47fba9d7d194c40b, Data_science\MachineLearning\DeepLearning\test\Dataset.vb"

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

    '   Total Lines: 99
    '    Code Lines: 81
    ' Comment Lines: 0
    '   Blank Lines: 18
    '     File Size: 3.21 KB


    ' Class Dataset
    ' 
    '     Properties: LableIndex
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: getAttrs, getLable, getRecord, iter, load
    '               size
    ' 
    '     Sub: (+2 Overloads) append, clear
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Public Class Dataset

    Friend records As IList(Of SampleData)

    Friend m_lableIndex As Integer = -1
    Friend maxLable As Double = -1

    Public Sub New(classIndex As Integer)
        m_lableIndex = classIndex
        records = New List(Of SampleData)()
    End Sub

    Private Sub New()
        m_lableIndex = -1
        records = New List(Of SampleData)()
    End Sub

    Public Overridable Function size() As Integer
        Return records.Count
    End Function

    Public Overridable ReadOnly Property LableIndex As Integer
        Get
            Return m_lableIndex
        End Get
    End Property

    Public Overridable Sub append(record As SampleData)
        records.Add(record)
    End Sub

    Public Overridable Sub clear()
        records.Clear()
    End Sub

    Public Overridable Sub append(attrs As Double(), lable As Double?)
        records.Add(New SampleData(attrs, lable))
    End Sub

    Public Overridable Function iter() As IEnumerator(Of SampleData)
        Return records.GetEnumerator()
    End Function

    Public Overridable Function getAttrs(index As Integer) As Double()
        Return records(index).features
    End Function

    Public Overridable Function getLable(index As Integer) As Double?
        Return records(index).labels(0)
    End Function

    Public Shared Function load(filePath As String, tag As String, lableIndex As Integer) As Dataset
        Dim dataset As New Dataset(lableIndex)
        Dim file As Stream = IO.File.OpenRead(filePath)
        Dim sep = tag.ToArray
        Try

            Dim [in] As StreamReader = New StreamReader(file)
            Dim line As Value(Of String) = ""
            While Not (line = [in].ReadLine()) Is Nothing
                Dim datas = line.Value.Split(sep)
                If datas.Length = 0 Then
                    Continue While
                End If
                Dim data = New Double(datas.Length - 1) {}
                For i = 0 To datas.Length - 1
                    data(i) = Double.Parse(datas(i))
                Next

                If lableIndex < 0 Then
                    dataset.append(New SampleData(data, -1))
                Else
                    Dim label As Double = data(lableIndex)
                    data = data.Take(lableIndex).JoinIterates(data.Skip(lableIndex + 1)).ToArray
                    dataset.append(New SampleData(data, label))
                End If
            End While
            [in].Close()
        Catch e As IOException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
            Return Nothing
        End Try
        Console.WriteLine("get data set with data records:" & dataset.size().ToString())
        Return dataset
    End Function

    Public Overridable Function getRecord(index As Integer) As SampleData
        Return records(index)
    End Function

End Class
