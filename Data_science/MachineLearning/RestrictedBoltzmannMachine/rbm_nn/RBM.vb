#Region "Microsoft.VisualBasic::5745e47ddb1e7dc33591406f939038aa, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\RBM.vb"

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
    '    Code Lines: 37
    ' Comment Lines: 9
    '   Blank Lines: 12
    '     File Size: 1.85 KB


    '     Class RBM
    ' 
    '         Properties: HiddenSize, VisibleSize, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: addVisibleNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nn.rbm

    ''' <summary>
    ''' Created by kenny on 5/12/14.
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/kennycason/rbm/tree/master
    ''' </remarks>
    Public Class RBM

        Public ReadOnly Property VisibleSize As Integer
            Get
                Return Weights.rows()
            End Get
        End Property

        Public ReadOnly Property HiddenSize As Integer
            Get
                Return Weights.columns()
            End Get
        End Property

        Public Property Weights As DenseMatrix

        Public Sub New(visibleSize As Integer, hiddenSize As Integer)
            Weights = DenseMatrix.randomGaussian(visibleSize, hiddenSize)
        End Sub

        Public Sub addVisibleNodes(n As Integer)
            Dim weights = DenseMatrix.make(VisibleSize + n, HiddenSize)

            ' copy original values
            For i = 0 To weights.rows() - 1
                For j = 0 To weights.columns() - 1
                    weights.set(i, j, weights.get(i, j))
                Next
            Next
            ' randomly init new weights;
            For i = 0 To weights.rows() - 1
                For j = weights.columns() To weights.columns() - 1
                    weights.set(i, j, randf.NextGaussian() * 0.1)
                Next
            Next

            Me.Weights = weights
        End Sub

        Public Overrides Function ToString() As String
            Return "RBM{" & "visibleSize=" & VisibleSize.ToString() & ", hiddenSize=" & HiddenSize.ToString() & ", weights=" & Weights.ToString() & "}"c.ToString()
        End Function

    End Class

End Namespace
