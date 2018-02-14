#Region "Microsoft.VisualBasic::0a54e6a4f7d02c6c1a8ac62395aaf1a5, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Set\StringSet.vb"

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

    '     Class StringSet
    ' 
    '         Sub: (+2 Overloads) New
    ' 
    '         Operators: (+2 Overloads) Or
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataStructures

    Public Class StringSet : Inherits [Set]

        Sub New(strings As IEnumerable(Of String), Optional caseSensitive As CompareMethod = CompareMethod.Binary)
            Call MyBase.New

            Dim compare As StringComparison = caseSensitive.GetCompareType

            MyBase._equals = Function(s1, s2)
                                 Return String.Equals(s1, s2, compare)
                             End Function
            MyBase._members = New [Set](strings, MyBase._equals)._members
        End Sub

        Friend Sub New([set] As [Set])
            Call MyBase.New

            MyBase._members = [set]._members
            MyBase._equals = [set]._equals
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator Or(s1 As StringSet, s2 As IEnumerable(Of String)) As StringSet
            Return New StringSet(DirectCast(s1, [Set]) Or New [Set](s2, s1._equals))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(s As StringSet) As String()
            Return s.ToArray(Of String)
        End Operator
    End Class
End Namespace
