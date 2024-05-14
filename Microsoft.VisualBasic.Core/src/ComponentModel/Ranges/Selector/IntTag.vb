#Region "Microsoft.VisualBasic::cad912ae1140ccfb151d96a22f387105, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Selector\IntTag.vb"

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
    '    Code Lines: 41
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.75 KB


    '     Structure IntTag
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo, OrderSelector, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges

    Public Structure IntTag(Of T)
        Implements IComparable

        Public ReadOnly n As Integer
        Public ReadOnly x As T

        Sub New(x As T, getInt As Func(Of T, Integer))
            Me.x = x
            Me.n = getInt(x)
        End Sub

        Sub New(n As Integer)
            Me.n = n
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            Else
                If TypeOf obj Is Integer Then
                    Return n.CompareTo(DirectCast(obj, Integer))
                ElseIf TypeOf obj Is IntTag(Of T) Then
                    Return n.CompareTo(DirectCast(obj, IntTag(Of T)).n)
                Else
                    Return 0
                End If
            End If
        End Function

        Public Shared Function OrderSelector(source As IEnumerable(Of T),
                                             getInt As Func(Of T, Integer),
                                             Optional asc As Boolean = True) As OrderSelector(Of IntTag(Of T))
            Dim array As IEnumerable(Of IntTag(Of T)) = source.Select(Function(x) New IntTag(Of T)(x, getInt))
            Dim selects As New OrderSelector(Of IntTag(Of T))(array, asc)
            Return selects
        End Function

        Public Shared Widening Operator CType(n As Integer) As IntTag(Of T)
            Return New IntTag(Of T)(n)
        End Operator
    End Structure
End Namespace
