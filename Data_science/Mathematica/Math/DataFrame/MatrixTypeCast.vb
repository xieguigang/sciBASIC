#Region "Microsoft.VisualBasic::d83a3efb6c0f39e985bdabdbbf6adbc6, Data_science\Mathematica\Math\DataFrame\DataFrame\MatrixTypeCast.vb"

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

'   Total Lines: 56
'    Code Lines: 42 (75.00%)
' Comment Lines: 5 (8.93%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 9 (16.07%)
'     File Size: 1.68 KB


' Module MatrixTypeCast
' 
'     Function: GetDataFrame, Transpose
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module MatrixTypeCast

    ''' <summary>
    ''' cast a named NxN data matrix into a dataframe object
    ''' </summary>
    ''' <param name="mat"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetDataFrame(mat As DataMatrix) As DataFrame
        Dim table As New Dictionary(Of String, FeatureVector)
        Dim keys As String() = mat.names.Objects

        For i As Integer = 0 To keys.Length - 1
            table(keys(i)) = New FeatureVector(keys(i), mat.matrix(i))
        Next

        Return New DataFrame With {
            .features = table,
            .rownames = keys
        }
    End Function

    <Extension>
    Public Function AsVector(col As FeatureVector) As Vector
        If DataFramework.IsNumericType(col.type) Then
            Return New Vector(From xi As Object In col.vector Select CDbl(xi))
        Else
            Throw New InvalidCastException($"{col.type.Name} could not be cast to a number directly!")
        End If
    End Function
End Module
