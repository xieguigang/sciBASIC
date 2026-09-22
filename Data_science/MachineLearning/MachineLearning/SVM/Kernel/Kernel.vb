#Region "Microsoft.VisualBasic::39c5a0d698c7ac8282b95591c4eda0f7, Data_science\MachineLearning\MachineLearning\SVM\Kernel\Kernel.vb"

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

    '   Total Lines: 245
    '    Code Lines: 178 (72.65%)
    ' Comment Lines: 35 (14.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 32 (13.06%)
    '     File Size: 9.30 KB


    '     Class Kernel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: computeSquaredDistance, dot, (+2 Overloads) KernelFunction, powi
    ' 
    '         Sub: SwapIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports std = System.Math

Namespace SVM

    Friend MustInherit Class Kernel
        Implements IQMatrix

        Private _x As Node()()
        Private _xSquare As Double()
        Private _kernelType As KernelType
        Private _degree As Integer
        Private _gamma As Double
        Private _coef0 As Double

        ''' <summary>
        ''' Request a column of the kernel matrix with a given length.
        ''' </summary>
        ''' <param name="column">The zero based index of the target row of the kernel matrix.</param>
        ''' <param name="len">The number of the elements that will be requested.</param>
        ''' <returns>An array which contains the first <paramref name="len"/> elements of the target column.</returns>
        Public MustOverride Function GetQ(column As Integer, len As Integer) As Single() Implements IQMatrix.GetQ

        ''' <summary>
        ''' Gets the diagonal elements of the kernel matrix.
        ''' </summary>
        ''' <returns>An array which contains the diagonal elements <i>Q(i, i)</i>.</returns>
        Public MustOverride Function GetQD() As Double() Implements IQMatrix.GetQD

        ''' <summary>
        ''' Swap the position of the two samples in the kernel matrix.
        ''' </summary>
        ''' <param name="i">The zero based index of the first sample.</param>
        ''' <param name="j">The zero based index of the second sample.</param>
        Public Overridable Sub SwapIndex(i As Integer, j As Integer) Implements IQMatrix.SwapIndex
            _x.Swap(i, j)

            If _xSquare IsNot Nothing Then
                _xSquare.Swap(i, j)
            End If
        End Sub

        Private Shared Function powi(value As Double, times As Integer) As Double
            Dim tmp = value, ret = 1.0
            Dim t = times

            While t > 0
                If t Mod 2 = 1 Then ret *= tmp
                tmp = tmp * tmp
                t = CInt(t / 2)
            End While

            Return ret
        End Function

        ''' <summary>
        ''' Evaluate the kernel function value between the two samples which are 
        ''' specified by their indices in the training data.
        ''' </summary>
        ''' <param name="i">The zero based index of the first sample.</param>
        ''' <param name="j">The zero based index of the second sample.</param>
        ''' <returns>The kernel function value <i>K(x_i, x_j)</i>.</returns>
        Public Function KernelFunction(i As Integer, j As Integer) As Double
            Select Case _kernelType
                Case KernelType.LINEAR
                    Return dot(_x(i), _x(j))
                Case KernelType.POLY
                    Return powi(_gamma * dot(_x(i), _x(j)) + _coef0, _degree)
                Case KernelType.RBF
                    Return std.Exp(-_gamma * (_xSquare(i) + _xSquare(j) - 2 * dot(_x(i), _x(j))))
                Case KernelType.SIGMOID
                    Return std.Tanh(_gamma * dot(_x(i), _x(j)) + _coef0)
                Case KernelType.PRECOMPUTED
                    Return _x(i)(CInt(_x(j)(0).value)).value
                Case Else
                    Return 0
            End Select
        End Function

        ''' <summary>
        ''' Create the kernel matrix from the training data and the parameters.
        ''' </summary>
        ''' <param name="l">The number of the training samples.</param>
        ''' <param name="x_">The feature matrix of the training data, each row is the sparse vector of one sample.</param>
        ''' <param name="param">The training parameters, which provides the kernel type and its coefficients.</param>
        Public Sub New(l As Integer, x_ As Node()(), param As Parameter)
            _kernelType = param.kernelType
            _degree = param.degree
            _gamma = param.gamma
            _coef0 = param.coefficient0
            _x = CType(x_.Clone(), Node()())

            If _kernelType = KernelType.RBF Then
                _xSquare = New Double(l - 1) {}

                For i = 0 To l - 1
                    _xSquare(i) = dot(_x(i), _x(i))
                Next
            Else
                _xSquare = Nothing
            End If
        End Sub

        Private Shared Function dot(xNodes As Node(), yNodes As Node()) As Double
            Dim sum As Double = 0
            Dim xlen = xNodes.Length
            Dim ylen = yNodes.Length
            Dim i = 0
            Dim j = 0
            Dim x = xNodes(0)
            Dim y = yNodes(0)

            While True

                If x.index = y.index Then
                    sum += x.value * y.value
                    i += 1
                    j += 1

                    If i < xlen AndAlso j < ylen Then
                        x = xNodes(i)
                        y = yNodes(j)
                    ElseIf i < xlen Then
                        x = xNodes(i)
                        Exit While
                    ElseIf j < ylen Then
                        y = yNodes(j)
                        Exit While
                    Else
                        Exit While
                    End If
                Else

                    If x.index > y.index Then
                        j += 1

                        If j < ylen Then
                            y = yNodes(j)
                        Else
                            Exit While
                        End If
                    Else
                        i += 1

                        If i < xlen Then
                            x = xNodes(i)
                        Else
                            Exit While
                        End If
                    End If
                End If
            End While

            Return sum
        End Function

        Private Shared Function computeSquaredDistance(xNodes As Node(), yNodes As Node()) As Double
            Dim x = xNodes(0)
            Dim y = yNodes(0)
            Dim xLength = xNodes.Length
            Dim yLength = yNodes.Length
            Dim xIndex As i32 = 0
            Dim yIndex As i32 = 0
            Dim sum As Double = 0

            While True

                If x.index = y.index Then
                    Dim d = x.value - y.value
                    sum += d * d
                    xIndex += 1
                    yIndex += 1

                    If xIndex < xLength AndAlso yIndex < yLength Then
                        x = xNodes(xIndex)
                        y = yNodes(yIndex)
                    ElseIf xIndex < xLength Then
                        x = xNodes(xIndex)
                        Exit While
                    ElseIf yIndex < yLength Then
                        y = yNodes(yIndex)
                        Exit While
                    Else
                        Exit While
                    End If
                ElseIf x.index > y.index Then
                    sum += y.value * y.value

                    If ++yIndex < yLength Then
                        y = yNodes(yIndex)
                    Else
                        Exit While
                    End If
                Else
                    sum += x.value * x.value

                    If ++xIndex < xLength Then
                        x = xNodes(xIndex)
                    Else
                        Exit While
                    End If
                End If
            End While

            While xIndex < xLength
                Dim d = xNodes(xIndex).value
                sum += d * d
                xIndex += 1
            End While

            While yIndex < yLength
                Dim d = yNodes(yIndex).value
                sum += d * d
                yIndex += 1
            End While

            Return sum
        End Function

        ''' <summary>
        ''' Evaluate the kernel function value between two sparse feature vectors.
        ''' </summary>
        ''' <param name="x">The first sparse feature vector.</param>
        ''' <param name="y">The second sparse feature vector.</param>
        ''' <param name="param">The training parameters, which provides the kernel type and its coefficients.</param>
        ''' <returns>The kernel function value <i>K(x, y)</i>.</returns>
        Public Shared Function KernelFunction(x As Node(), y As Node(), param As Parameter) As Double
            Select Case param.kernelType
                Case KernelType.LINEAR
                    Return dot(x, y)
                Case KernelType.POLY
                    Return powi(param.degree * dot(x, y) + param.coefficient0, param.degree)
                Case KernelType.RBF
                    Dim sum = computeSquaredDistance(x, y)
                    Return std.Exp(-param.gamma * sum)
                Case KernelType.SIGMOID
                    Return std.Tanh(param.gamma * dot(x, y) + param.coefficient0)
                Case KernelType.PRECOMPUTED
                    Return x(CInt(y(0).value)).value
                Case Else
                    Return 0
            End Select
        End Function
    End Class
End Namespace
