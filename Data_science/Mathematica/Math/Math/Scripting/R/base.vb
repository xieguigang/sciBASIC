#Region "Microsoft.VisualBasic::f6b6c0be2b40e1f2f6a168bd211182d4, Data_science\Mathematica\Math\Math\Scripting\R\base.vb"

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

'   Total Lines: 50
'    Code Lines: 37 (74.00%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 13 (26.00%)
'     File Size: 1.71 KB


'     Module base
' 
'         Function: c, cor, diff, range
' 
'     Module [is]
' 
'         Function: na
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq

Namespace Scripting.BasicR

    Public Module base

        Public Function c(Of T)(ParamArray v As IEnumerable(Of T)()) As T()
            Return v.IteratesALL.ToArray
        End Function

        Public Function range(x As IEnumerable(Of Double)) As Double()
            Return New DoubleRange(x).MinMax
        End Function

        Public Function diff(x As IEnumerable(Of Double)) As Double()
            Return NumberGroups.diff(x.SafeQuery.ToArray)
        End Function

        Public Function cor(x As Double()(), y As Double()(), Optional method As String = "pearson") As Double()()
            Dim cor_func As Func(Of Double(), Double(), Double)

            Select Case method
                Case "pearson" : cor_func = AddressOf Correlations.GetPearson
                Case Else
                    Throw New NotSupportedException(method)
            End Select

            Dim cor_mat As Double()() = RectangularArray.Matrix(Of Double)(x.Length, y.Length)

            For i As Integer = 0 To x.Length - 1
                For j As Integer = 0 To y.Length - 1
                    cor_mat(i)(j) = cor_func(x(i), y(j))
                Next
            Next

            Return cor_mat
        End Function

        Public Function mean(ParamArray x As Double()) As Double
            Return x.Average
        End Function

    End Module

    Public Module [is]

        Public Function na(x As IEnumerable(Of Double)) As BooleanVector
            Return New BooleanVector(x.Select(Function(xi) xi.IsNaNImaginary))
        End Function
    End Module
End Namespace
