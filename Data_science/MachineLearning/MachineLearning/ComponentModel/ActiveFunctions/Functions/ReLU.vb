#Region "Microsoft.VisualBasic::c188e4a3a19fbfab5332b8f577dacda9, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\ReLU.vb"

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

    '   Total Lines: 86
    '    Code Lines: 72 (83.72%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (16.28%)
    '     File Size: 2.47 KB


    '     Class ReLU
    ' 
    '         Properties: Store
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: [Function], CalculateDerivative, Derivative, (+3 Overloads) ReLU, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ComponentModel.Activations

    Public Class ReLU : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {
                        New NamedValue With {
                            .name = "threshold",
                            .text = threshold
                        }
                    },
                    .name = NameOf(ReLU)
                }
            End Get
        End Property

        ReadOnly threshold# = 0

        Sub New()
        End Sub

        Sub New(threshold As Double)
            Me.threshold = threshold
        End Sub

        Public Overrides Function [Function](x As Double) As Double
            If x < threshold Then
                Return threshold
            ElseIf Truncate > 0 Then
                Return ValueTruncate(x, Truncate)
            Else
                Return x
            End If
        End Function

        Public Shared Function ReLU(x As Double) As Double
            If x < 0 Then
                Return 0
            Else
                Return x
            End If
        End Function

        Public Shared Function ReLU(x As Vector) As Vector
            x(x < 0.0) = Vector.Zero
            Return x
        End Function

        ''' <summary>
        ''' ReLU activator function will clip the negative value as zero
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Function ReLU(x As Double()) As Double()
            For i As Integer = 0 To x.Length - 1
                If x(i) < 0 Then
                    x(i) = 0
                End If
            Next

            Return x
        End Function

        Public Overrides Function CalculateDerivative(x As Double) As Double
            If x < threshold Then
                Return threshold
            Else
                Return 1
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            If x < threshold Then
                Return threshold
            Else
                Return 1
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(ReLU)}()"
        End Function
    End Class
End Namespace
