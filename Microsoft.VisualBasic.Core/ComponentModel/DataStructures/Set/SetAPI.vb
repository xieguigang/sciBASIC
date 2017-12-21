#Region "Microsoft.VisualBasic::994822921e7754c3999a46354ecc2456, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\Set\SetAPI.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language

Public Module SetAPI

    Public Delegate Function GetUid(Of T)(x As T) As String

    '''<summary>
    ''' Performs an intersection of two sets.(求交集，这个函数总是会挑选出<paramref name="s1"/>集合之中的元素的)
    ''' </summary>
    ''' <param name="s1">Any set.</param>
    ''' <param name="s2">Any set.</param>
    ''' <returns>A new <see cref="[Set]">Set</see> object that contains the members
    ''' that were common to both of the input sets.</returns>
    <Extension>
    Public Function Intersection(Of T)(s1 As IEnumerable(Of T), s2 As IEnumerable(Of T), getUID As GetUid(Of T)) As T()
        Dim tags = (From x As T In s1 Let uid As String = getUID(x) Select uid, tag = NameOf(s1), x).AsList +
            From x As T
            In s2
            Let uid As String = getUID(x)
            Select uid,
                tag = NameOf(s2),
                x
        Dim intersectGroups = From g
                              In tags.GroupBy(Function(x) x.uid) ' 按照uid字符串进行分组
                              Let source = g.ToArray
                              Let taglist As String() = source _
                                   .Select(Function(x) x.tag) _
                                   .Distinct _
                                   .ToArray
                              Where taglist.Length > 1 ' 对每一个分组数据，根据标签的数量来了解是否为交集的一部分，当为交集元素的时候，标签的数量是两个，即同时存在于两个集合之众
                              Select source
        ' 总是挑选出s1的数据
        Dim result = LinqAPI.Exec(Of T) _
 _
            () <= From x
                  In intersectGroups
                  Let s1Group = x.Where(Function(o) o.tag = NameOf(s1)).First
                  Let value As T = s1Group.x
                  Select value

        Return result
    End Function

    Public Delegate Function IEquals(Of T)(a As T, b As T) As Boolean

    '''<summary>
    ''' Performs an intersection of two sets.(求交集)
    ''' </summary>
    ''' <param name="s1">Any set.</param>
    ''' <param name="s2">Any set.</param>
    ''' <returns>A new <see cref="[Set]">Set</see> object that contains the members
    ''' that were common to both of the input sets.</returns>
    ''' 
    <Extension>
    Public Function Intersection(Of T)(s1 As IEnumerable(Of T), s2 As IEnumerable(Of T), __equals As IEquals(Of T)) As T()
        Dim result As New List(Of T)

        If s1.Count > s2.Count Then
            For Each o As T In s1
                If s2.Contains(o, __equals) Then
                    result.Add(o)
                End If
            Next
        Else
            For Each o As T In s2
                If s1.Contains(o, __equals) Then
                    result.Add(o)
                End If
            Next
        End If

        Return result
    End Function

    <Extension>
    Public Function Contains(Of T)([set] As IEnumerable(Of T), x As T, __equals As IEquals(Of T)) As Boolean
        Dim LQuery = (From obj As T In [set].AsParallel Where __equals(x, obj) Select 1).FirstOrDefault
        Return LQuery > 0
    End Function

    <Extension>
    Public Function Intersection(s1 As IEnumerable(Of String), s2 As IEnumerable(Of String), Optional strict As Boolean = True) As String()
        Return s1.Intersection(s2, AddressOf New __stringCompares(strict).Equals)
    End Function

    Private Structure __stringCompares
        Dim mode As StringComparison

        Sub New(strict As Boolean)
            mode = If(strict, StringHelpers.StrictCompares, StringHelpers.NonStrictCompares)
        End Sub

        Public Overloads Function Equals(a As String, b As String) As Boolean
            Return String.Equals(a, b, mode)
        End Function
    End Structure
End Module
