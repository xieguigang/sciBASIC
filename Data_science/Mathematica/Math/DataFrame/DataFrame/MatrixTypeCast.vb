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
    '    Code Lines: 42
    ' Comment Lines: 5
    '   Blank Lines: 9
    '     File Size: 1.68 KB


    ' Module MatrixTypeCast
    ' 
    '     Function: GetDataFrame, Transpose
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

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
    Public Function Transpose(mat As DataFrame) As DataFrame
        Dim table As New Dictionary(Of String, FeatureVector)
        Dim cols As String() = mat.featureNames

        If mat.rownames.IsNullOrEmpty Then
            mat.rownames = mat.dims.Height _
                .Sequence _
                .Select(Function(i) CStr(i + 1)) _
                .ToArray
        End If

        Dim nrows = mat.dims.Height
        Dim index As Integer

        For i As Integer = 0 To nrows - 1
            index = i
            table(mat.rownames(i)) = New FeatureVector(
                name:=mat.rownames(i),
                doubles:=cols _
                    .Select(Function(k) CDbl(mat(k)(index))) _
                    .ToArray
            )
        Next

        Return New DataFrame With {
            .features = table,
            .rownames = cols
        }
    End Function
End Module
