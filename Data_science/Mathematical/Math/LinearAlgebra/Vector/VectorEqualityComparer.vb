#Region "Microsoft.VisualBasic::0b700fbf57d545fa38cad5f88032ac5a, ..\sciBASIC#\Data_science\Mathematical\Math\LinearAlgebra\Vector\VectorEqualityComparer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic

Namespace LinearAlgebra

    Public Class VectorEqualityComparer
        Implements IEqualityComparer(Of Vector)

        Public Shared Function VectorEqualsToAnother(v1 As List(Of Double), v2 As List(Of Double)) As Boolean
            If v1.Count <> v2.Count Then
                Return False
            End If

            For i As Integer = 0 To v1.Count - 1
                If v1(i) <> v2(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Shared Function VectorEqualsToAnother(v1#(), v2#()) As Boolean
            If v1.Length <> v2.Length Then
                Return False
            End If

            For i As Integer = 0 To v1.Length - 1
                If v1(i) <> v2(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Overloads Function Equals(x As Vector, y As Vector) As Boolean Implements IEqualityComparer(Of Vector).Equals
            Return VectorEqualsToAnother(x, y)
        End Function

        Public Overloads Function GetHashCode(v As Vector) As Integer Implements IEqualityComparer(Of Vector).GetHashCode
            Dim sum As Integer = v.Sum(Function(x) x.GetHashCode)
            Return sum
        End Function
    End Class
End Namespace
