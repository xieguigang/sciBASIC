﻿#Region "Microsoft.VisualBasic::214369396ce57aafd3fbbfb35dee4a42, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/Math//Algebra/Matrix.NET/Math/WiseOperation.vb"

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

    '   Total Lines: 31
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 941 B


    '     Class WiseOperation
    ' 
    '         Function: ColWise, RowWise, Sum
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace LinearAlgebra.Matrix

    Public Class WiseOperation

        Dim matrix_wise As Vector()

        Public Function Sum() As Vector
            Return matrix_wise.Select(Function(xi) xi.Sum).AsVector
        End Function

        Public Shared Function RowWise(m As GeneralMatrix) As WiseOperation
            Return New WiseOperation With {
                .matrix_wise = m.RowVectors.ToArray
            }
        End Function

        Public Shared Function ColWise(m As GeneralMatrix) As WiseOperation
            Return New WiseOperation With {
                .matrix_wise = m.ColumnDimension _
                    .Sequence _
                    .Select(Function(i)
                                Return m.ColumnVector(i)
                            End Function) _
                    .ToArray
            }
        End Function

    End Class
End Namespace
