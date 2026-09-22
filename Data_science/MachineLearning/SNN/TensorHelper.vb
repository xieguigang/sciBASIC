#Region "Microsoft.VisualBasic::24d28ff7b0bf2d2d91838c69088be712, Data_science\MachineLearning\SNN\TensorHelper.vb"

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

    '   Total Lines: 23
    '    Code Lines: 20 (86.96%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (13.04%)
    '     File Size: 762 B


    ' Module TensorHelper
    ' 
    '     Function: AllTensor, BatchTensor
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Public Module TensorHelper

    Public Function BatchTensor(data As Double()(), idx As Integer(), offset As Integer, count As Integer) As Tensor
        Dim f = data(0).Length
        Dim flat(f * count - 1) As Double
        For b = 0 To count - 1
            Array.Copy(data(idx(offset + b)), 0, flat, b * f, f)
        Next
        Return New Tensor(flat, count, f)
    End Function

    Public Function AllTensor(d As NumericTable) As Tensor
        Dim n = d.nsamples
        Dim idx(n - 1) As Integer
        For i = 0 To n - 1
            idx(i) = i
        Next
        Return BatchTensor(d.features, idx, 0, n)
    End Function
End Module
