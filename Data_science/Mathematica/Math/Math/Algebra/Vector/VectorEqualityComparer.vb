#Region "Microsoft.VisualBasic::d55a2b3c72c6e0b8f8001e08f834bbde, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\Vector\VectorEqualityComparer.vb"

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

Namespace LinearAlgebra

    ''' <summary>
    ''' Determine that the two vector are equals to each other.
    ''' </summary>
    ''' <remarks>单实例模式？？</remarks>
    Public Class VectorEqualityComparer : Implements IEqualityComparer(Of Vector)

        ''' <summary>
        ''' Sequence Equals
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' Sum all elements' hashcode 
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Overloads Function GetHashCode(v As Vector) As Integer Implements IEqualityComparer(Of Vector).GetHashCode
            Dim sum As Integer = v.Sum(Function(x) x.GetHashCode)
            Return sum
        End Function
    End Class
End Namespace
