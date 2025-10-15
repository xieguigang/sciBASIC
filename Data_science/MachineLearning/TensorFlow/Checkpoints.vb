#Region "Microsoft.VisualBasic::4ee180d5170f9f13ba45aabf49d1a002, Data_science\MachineLearning\TensorFlow\Checkpoints.vb"

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

    '   Total Lines: 49
    '    Code Lines: 30 (61.22%)
    ' Comment Lines: 9 (18.37%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.53 KB


    ' Class Checkpoints
    ' 
    '     Properties: Instance
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: AddCheckpoint
    ' 
    '     Sub: CalculateCheckpointGradients, ClearCheckpoints
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Increases performence by saving states that the backtracking 
''' derivative calculations can be performed between.
''' </summary>
Public NotInheritable Class Checkpoints

    Public Shared ReadOnly Property Instance As New Checkpoints

    ' Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
    Shared Sub New()
    End Sub

    Private Sub New()
    End Sub

    Private nr As Integer = 1
    Private checkpoints_in As New SortedDictionary(Of Integer, Tensor)()
    Private checkpoints_out As New SortedDictionary(Of Integer, Tensor)()

    Public Function AddCheckpoint(data As Tensor) As Tensor
        checkpoints_in(nr) = data
        Dim data_copy = New Tensor(data)
        checkpoints_out(nr) = data_copy
        nr += 1

        Return data_copy
    End Function

    Public Sub ClearCheckpoints()
        checkpoints_in.Clear()
        checkpoints_out.Clear()
        nr = 1
    End Sub

    ''' <summary>
    ''' Perform derivative calculations for all elements in a tensor with input values from 
    ''' the derivatives of another tensor with the same shape
    ''' </summary>
    Public Sub CalculateCheckpointGradients()
        Dim checkpoint_numbers As New List(Of Integer)(checkpoints_in.Keys)
        checkpoint_numbers.Sort()
        checkpoint_numbers.Reverse()
        For Each nr In checkpoint_numbers
            checkpoints_in(nr).TransferDerivatives(checkpoints_out(nr))
        Next
    End Sub


End Class
