#Region "Microsoft.VisualBasic::a38b43b6e6690227e6aa649b55adf872, Data_science\Visualization\Plots\Contour\HeatMap\MatrixEvaluate.vb"

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

    '   Total Lines: 79
    '    Code Lines: 49 (62.03%)
    ' Comment Lines: 19 (24.05%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 11 (13.92%)
    '     File Size: 2.90 KB


    '     Class MatrixEvaluate
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, fromMatrixQuery
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Contour.HeatMap

    ''' <summary> 
    ''' 直接返回矩阵数据
    ''' </summary>
    Public Class MatrixEvaluate : Inherits EvaluatePoints

        ''' <summary>
        ''' [x -> [y, z]]
        ''' </summary>
        ReadOnly matrix As BinarySearchFunction(Of Double, (x#, bin As BinarySearchFunction(Of Double, (y#, z#))))

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="matrix">
        ''' id is the x axis value
        ''' properties in one data set object is the y vector and z vector
        ''' </param>
        Sub New(matrix As IEnumerable(Of DataSet), gridSize As SizeF)
            Dim compareWithError As Comparison(Of Double) =
                Function(a, b)
                    If stdNum.Abs(a - b) <= gridSize.Width Then
                        Return 0
                    ElseIf a < b Then
                        Return -1
                    Else
                        Return 1
                    End If
                End Function
            Dim ySearch = matrix _
                .DoCall(AddressOf fromMatrixQuery) _
                .Select(Function(row)
                            Dim bin As New BinarySearchFunction(Of Double, (y#, z#))(row.Item2, Function(p) p.y, compareWithError)

                            Return (row.x, bin)
                        End Function) _
                .ToArray

            Me.matrix = New BinarySearchFunction(Of Double, (x#, BinarySearchFunction(Of Double, (y#, z#))))(ySearch, Function(p) p.x, compareWithError)
        End Sub

        Private Shared Function fromMatrixQuery(matrix As IEnumerable(Of DataSet)) As IEnumerable(Of (x As Double, (y As Double, z As Double)()))
            Return From line As DataSet
                   In matrix
                   Let xi = Val(line.ID)
                   Let data = line.Properties.Select(Function(o) (Y:=Val(o.Key), Z:=o.Value)).OrderBy(Function(pt) pt.Y).ToArray
                   Select (xi, data)
        End Function

        ''' <summary>
        ''' query data by binary search
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(x As Double, y As Double) As Double
            Dim row = matrix.BinarySearch(x)

            If row = -1 Then
                Return 0.0
            End If

            Dim point = matrix(row).bin.BinarySearch(y)

            If point = -1 Then
                Return 0.0
            Else
                Return matrix(row).bin(point).z
            End If
        End Function
    End Class
End Namespace
