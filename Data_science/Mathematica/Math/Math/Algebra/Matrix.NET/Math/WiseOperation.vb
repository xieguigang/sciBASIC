#Region "Microsoft.VisualBasic::6a524270bb14e169467a961b30b0201d, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Math\WiseOperation.vb"

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

    '   Total Lines: 45
    '    Code Lines: 34 (75.56%)
    ' Comment Lines: 1 (2.22%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (22.22%)
    '     File Size: 1.58 KB


    '     Class WiseOperation
    ' 
    '         Properties: matrix_wise
    ' 
    '         Function: ColWise, RowWise, ScaleX, Sum
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SimdParallel = Microsoft.VisualBasic.Math.SIMD.SimdParallel

Namespace LinearAlgebra.Matrix

    Public Class WiseOperation

        Public ReadOnly Property matrix_wise As Vector()

        Public Function Sum() As Vector
            Dim out As Double() = New Double(matrix_wise.Length - 1) {}

            ' 每一行/列直接走 SIMD 归约求和
            For i As Integer = 0 To matrix_wise.Length - 1
                out(i) = SimdParallel.Sum(matrix_wise(i).Array)
            Next

            Return New Vector(out)
        End Function

        Public Iterator Function ScaleX(Optional center As Boolean = True, Optional scale As Boolean = True) As IEnumerable(Of Vector)
            For Each v As Vector In matrix_wise
                Yield New Vector(ScaleMaps.Scale(v.Array, center, scale))
            Next
        End Function

        Public Shared Function RowWise(m As GeneralMatrix) As WiseOperation
            Return New WiseOperation With {
                ._matrix_wise = m.RowVectors.ToArray
            }
        End Function

        Public Shared Function ColWise(m As GeneralMatrix) As WiseOperation
            Return New WiseOperation With {
                ._matrix_wise = m.ColumnDimension _
                    .Sequence _
                    .Select(Function(i)
                                Return m.ColumnVector(i)
                            End Function) _
                    .ToArray
            }
        End Function

    End Class
End Namespace
