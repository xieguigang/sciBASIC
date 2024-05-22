#Region "Microsoft.VisualBasic::7c3ca3c20435ca7c8cf657e54ae92361, Data_science\MachineLearning\MachineLearning\SVM\Kernel\PrecomputedKernel.vb"

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

    '   Total Lines: 119
    '    Code Lines: 37 (31.09%)
    ' Comment Lines: 62 (52.10%)
    '    - Xml Docs: 29.03%
    ' 
    '   Blank Lines: 20 (16.81%)
    '     File Size: 4.64 KB


    '     Class PrecomputedKernel
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Namespace SVM

    ''' <summary>
    ''' Class encapsulating a precomputed kernel, where each position indicates the similarity score for two items in the training data.
    ''' </summary>
    <Serializable>
    Public Class PrecomputedKernel

        Private _similarities As Single(,)
        Private _rows As Integer
        Private _columns As Integer

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="similarities">The similarity scores between all items in the training data</param>
        Public Sub New(similarities As Single(,))
            _similarities = similarities
            _rows = _similarities.GetLength(0)
            _columns = _similarities.GetLength(1)
        End Sub

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="nodes">Nodes for self-similarity analysis</param>
        ''' <param name="param">Parameters to use when computing similarities</param>
        Public Sub New(nodes As List(Of Node()), param As Parameter)
            _rows = nodes.Count
            _columns = _rows
            _similarities = New Single(_rows - 1, _columns - 1) {}

            For r = 0 To _rows - 1

                For c = 0 To r - 1
                    _similarities(r, c) = _similarities(c, r)
                Next

                _similarities(r, r) = 1

                For c = r + 1 To _columns - 1
                    _similarities(r, c) = CSng(Kernel.KernelFunction(nodes(r), nodes(c), param))
                Next
            Next
        End Sub

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="rows">Nodes to use as the rows of the matrix</param>
        ''' <param name="columns">Nodes to use as the columns of the matrix</param>
        ''' <param name="param">Parameters to use when compute similarities</param>
        Public Sub New(rows As List(Of Node()), columns As List(Of Node()), param As Parameter)
            _rows = rows.Count
            _columns = columns.Count
            _similarities = New Single(_rows - 1, _columns - 1) {}

            For r = 0 To _rows - 1

                For c = 0 To _columns - 1
                    _similarities(r, c) = CSng(Kernel.KernelFunction(rows(r), columns(c), param))
                Next
            Next
        End Sub

        '''' <summary>
        '''' Constructs a <see cref="Problem"/> object using the labels provided. If a label is set to "0" that item is ignored.
        '''' </summary>
        '''' <param name="rowLabels">The labels for the row items</param>
        '''' <param name="columnLabels">The labels for the column items</param>
        '''' <returns>A <see cref="Problem"/> object</returns>
        'Public Function Compute(rowLabels As Double(), columnLabels As Double()) As Problem
        '    Dim X As List(Of Node()) = New List(Of Node())()
        '    Dim Y As New List(Of Double)()
        '    Dim maxIndex = 0

        '    For i = 0 To columnLabels.Length - 1
        '        If columnLabels(i) <> 0 Then maxIndex += 1
        '    Next

        '    maxIndex += 1

        '    For r = 0 To _rows - 1
        '        If rowLabels(r) = 0 Then Continue For
        '        Dim nodes As List(Of Node) = New List(Of Node)()

        '        nodes.Add(New Node(0, X.Count + 1))

        '        For c = 0 To _columns - 1
        '            If columnLabels(c) = 0 Then Continue For
        '            Dim value As Double = _similarities(r, c)
        '            nodes.Add(New Node(nodes.Count, value))
        '        Next

        '        X.Add(nodes.ToArray())
        '        Y.Add(rowLabels(r))
        '    Next

        '    Return New Problem(Y.ToArray(), X.ToArray(), maxIndex)
        'End Function
    End Class
End Namespace
