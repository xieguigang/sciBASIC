#Region "Microsoft.VisualBasic::3464b60d9dbb97cd2b999bb7b903409d, Data_science\Mathematica\SignalProcessing\SignalProcessing\Cluster\SignalClustering.vb"

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

    '   Total Lines: 52
    '    Code Lines: 39
    ' Comment Lines: 3
    '   Blank Lines: 10
    '     File Size: 1.70 KB


    ' Class SignalClustering
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AddSignal, CompareTwoSignals, GetClusters
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

''' <summary>
''' Try to make time serial signal data into multiple clusters
''' </summary>
Public Class SignalClustering

    ReadOnly tree As AVLTree(Of GeneralSignal, GeneralSignal)

    ReadOnly steps#
    ReadOnly sig_equals As Double
    ReadOnly sig_simialr As Double

    Sub New(Optional sig_equals# = 0.8, Optional sig_simialr# = 0.4, Optional steps# = 0.25)
        Me.tree = New AVLTree(Of GeneralSignal, GeneralSignal)(AddressOf CompareTwoSignals, Function(sig) sig.reference)
        Me.steps = steps
        Me.sig_simialr = sig_simialr
        Me.sig_equals = sig_equals
    End Sub

    Public Iterator Function GetClusters() As IEnumerable(Of NamedCollection(Of GeneralSignal))
        Dim i As i32 = Scan0

        For Each node In tree.GetAllNodes
            Yield New NamedCollection(Of GeneralSignal) With {
                .name = ++i,
                .value = node.Members
            }
        Next
    End Function

    Public Function CompareTwoSignals(a As GeneralSignal, b As GeneralSignal) As Integer
        Dim var As Double = Alignment.Similarity(a, b, steps)

        If var >= sig_equals Then
            Return 0
        ElseIf var >= sig_simialr Then
            Return 1
        Else
            Return -1
        End If
    End Function

    Public Function AddSignal(signal As GeneralSignal) As SignalClustering
        signal = signal.Normalize
        tree.Add(signal, signal, valueReplace:=False)

        Return Me
    End Function
End Class
