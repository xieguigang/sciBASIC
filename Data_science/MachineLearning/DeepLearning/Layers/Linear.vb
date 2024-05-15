#Region "Microsoft.VisualBasic::d73e8de7c0a480d20e68fc7a4cffae2f, Data_science\MachineLearning\DeepLearning\Layers\Linear.vb"

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

    '   Total Lines: 39
    '    Code Lines: 28
    ' Comment Lines: 3
    '   Blank Lines: 8
    '     File Size: 1.15 KB


    '     Class Linear
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Fit, gauss
    ' 
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Math

    ''' <summary>
    ''' implements torch.nn.Linear
    ''' </summary>
    Public Class Linear

        Dim w As NumericMatrix
        Dim b As Vector
        Dim bias As Boolean

        Sub New(in_features As Integer, out_features As Integer, Optional bias As Boolean = True)
            Me.bias = bias
            Me.w = New NumericMatrix(gauss(in_features, out_features))
            Me.b = Vector.Zero(out_features)
        End Sub

        Private Iterator Function gauss(in_features As Integer, out_features As Integer) As IEnumerable(Of Vector)
            For i As Integer = 0 To out_features - 1
                Yield Vector.rand(in_features)
            Next
        End Function

        Public Function Fit(x As Vector) As Vector
            Return w.DotMultiply(x) + b
        End Function

        Public Sub backward(loss As Vector)
            w = w - loss

            If bias Then
                b -= loss
            End If
        End Sub
    End Class
End Namespace
