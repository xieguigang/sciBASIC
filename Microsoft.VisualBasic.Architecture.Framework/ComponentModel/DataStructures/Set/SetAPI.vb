#Region "Microsoft.VisualBasic::3876b6eed33a0a99378bfedad808b1b7, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\Set\SetAPI.vb"

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
    ''' 
    <Extension>
    Public Function Intersection(Of T)(s1 As IEnumerable(Of T), s2 As IEnumerable(Of T), __uid As GetUid(Of T)) As T()
        Dim tag As String = NameOf(s1) '  由于需要交换标签，所以在这里不能使用并行化
        Dim uids = (From x As T In s1.AsParallel Select uid = __uid(x), st = tag, x).AsList
        tag = NameOf(s2)
        uids += (From x As T In s2.AsParallel Select uid = __uid(x), st = tag, x).ToArray
        Dim Groups = (From x In uids Select x Group x By x.uid Into Group)  ' 按照uid字符串进行分组
        Dim GetIntersects = (From Group In Groups.AsParallel
                             Let source = Group.Group.ToArray  ' 对每一个分组数据，根据标签的数量来了解是否为交集的一部分，当为交集元素的时候，标签的数量是两个，即同时存在于两个集合之众
                             Let tags As String() = source.ToArray(Function(x) x.st).Distinct.ToArray
                             Where tags.Length > 1
                             Select source).ToArray
        tag = NameOf(s1)  ' 总是挑选出s1的数据
        Dim result As T() = (From x In GetIntersects.AsParallel
                             Select (From o In x
                                     Where String.Equals(tag, o.st)
                                     Select o).ToArray(Function(o) o.x)).ToVector
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
