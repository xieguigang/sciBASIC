#Region "Microsoft.VisualBasic::67da914564a61281e77141cf63782804, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Set\SetAPI.vb"

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

    '   Total Lines: 154
    '    Code Lines: 101 (65.58%)
    ' Comment Lines: 32 (20.78%)
    '    - Xml Docs: 65.62%
    ' 
    '   Blank Lines: 21 (13.64%)
    '     File Size: 6.48 KB


    '     Class GenericLambda
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    ' 
    '     Module SetAPI
    ' 
    '         Function: Contains, (+3 Overloads) Intersection, VennExclusiveSet
    '         Structure __stringCompares
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: Equals
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace ComponentModel.DataStructures

    Public NotInheritable Class GenericLambda(Of T)

        Public Delegate Function IEquals(a As T, b As T) As Boolean
        Public Delegate Function GetUid(x As T) As String

    End Class

    Public Module SetAPI

        '''<summary>
        ''' Performs an intersection of two sets.(求交集，这个函数总是会挑选出<paramref name="s1"/>集合之中的元素的)
        ''' </summary>
        ''' <param name="s1">Any set.</param>
        ''' <param name="s2">Any set.</param>
        ''' <returns>A new <see cref="[Set]">Set</see> object that contains the members
        ''' that were common to both of the input sets.</returns>
        <Extension>
        Public Function Intersection(Of T)(s1 As IEnumerable(Of T), s2 As IEnumerable(Of T), getUID As GenericLambda(Of T).GetUid) As T()
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

        '''<summary>
        ''' Performs an intersection of two sets.(求交集)
        ''' </summary>
        ''' <param name="s1">Any set.</param>
        ''' <param name="s2">Any set.</param>
        ''' <returns>A new <see cref="[Set]">Set</see> object that contains the members
        ''' that were common to both of the input sets.</returns>
        ''' 
        <Extension>
        Public Function Intersection(Of T)(s1 As IEnumerable(Of T), s2 As IEnumerable(Of T), __equals As GenericLambda(Of T).IEquals) As T()
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
        Public Function Contains(Of T)([set] As IEnumerable(Of T), x As T, __equals As GenericLambda(Of T).IEquals) As Boolean
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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="groups"></param>
        ''' <param name="commonIds">
        ''' common ids in all groups, if you do not need this parameter, you can set it to <c>Nothing</c>.
        ''' </param>
        ''' <returns>
        ''' exclusive ids in each groups
        ''' </returns>
        <Extension>
        Public Function VennExclusiveSet(groups As Dictionary(Of String, String()), Optional ByRef commonIds As String() = Nothing) As Dictionary(Of String, String())
            ' 统计每个ID出现的组数
            Dim idOccurrence As New Dictionary(Of String, Integer)
            ' 计算总组数
            Dim totalGroups = groups.Count
            ' 计算每个组特有的ID
            Dim exclusiveIds As New Dictionary(Of String, String())

            For Each group As KeyValuePair(Of String, String()) In groups
                ' 对每个组的ID进行去重处理
                For Each id As String In group.Value.Distinct()
                    If idOccurrence.ContainsKey(id) Then
                        idOccurrence(id) += 1
                    Else
                        idOccurrence(id) = 1
                    End If
                Next
            Next

            ' 计算所有组共有的ID
            commonIds = idOccurrence _
                .Where(Function(kv) kv.Value = totalGroups) _
                .Keys

            For Each group As KeyValuePair(Of String, String()) In groups
                ' 找出只在该组出现的ID
                Dim uniqueIds = group.Value _
                    .Distinct() _
                    .Where(Function(id) idOccurrence(id) = 1) _
                    .ToArray

                Call exclusiveIds.Add(group.Key, uniqueIds)
            Next

            Return exclusiveIds
        End Function
    End Module
End Namespace
