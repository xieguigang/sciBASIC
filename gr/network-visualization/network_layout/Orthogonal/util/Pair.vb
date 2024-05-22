#Region "Microsoft.VisualBasic::4e0fdab80c65c5c2e2c9ae3a2dfea02c, gr\network-visualization\network_layout\Orthogonal\util\Pair.vb"

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

    '   Total Lines: 42
    '    Code Lines: 34 (80.95%)
    ' Comment Lines: 1 (2.38%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (16.67%)
    '     File Size: 1.39 KB


    '     Class Pair
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Orthogonal.util

    Public Class Pair(Of T1, T2)

        Public m_a As T1
        Public m_b As T2

        Public Sub New(a As T1, b As T2)
            m_a = a
            m_b = b
        End Sub

        Public Overrides Function Equals(o As Object) As Boolean
            If Not (TypeOf o Is Pair(Of T1, T2)) Then
                Return False
            End If
            If m_a Is Nothing AndAlso CType(o, Pair(Of T1, T2)).m_a IsNot Nothing Then
                Return False
            End If
            If m_b Is Nothing AndAlso CType(o, Pair(Of T1, T2)).m_b IsNot Nothing Then
                Return False
            End If
            If Not m_a.Equals(CType(o, Pair(Of T1, T2)).m_a) Then
                Return False
            End If
            If Not m_b.Equals(CType(o, Pair(Of T1, T2)).m_b) Then
                Return False
            End If
            Return True
        End Function

        Public Overrides Function ToString() As String
            Return "<" & m_a.ToString() & ", " & m_b.ToString() & ">"
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return 31 * m_a.GetHashCode() + m_b.GetHashCode()
            ' http://stackoverflow.com/questions/299304/why-does-javas-hashcode-in-string-use-31-as-a-multiplier
        End Function
    End Class

End Namespace
