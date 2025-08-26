#Region "Microsoft.VisualBasic::d20306ac0094c9c0646fabd81e1c2151, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\FuzzyGroup.vb"

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

    '   Total Lines: 146
    '    Code Lines: 88 (60.27%)
    ' Comment Lines: 39 (26.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (13.01%)
    '     File Size: 6.09 KB


    '     Module FuzzyGroup
    ' 
    '         Function: (+2 Overloads) FuzzyGroups
    '         Structure __groupHelper
    ' 
    '             Function: Equals, ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    ''' <summary>
    ''' 对数据进行分组，通过标签数据的相似度
    ''' </summary>
    Public Module FuzzyGroup

        ''' <summary>
        ''' Grouping objects in a collection based on their <see cref="INamedValue.Key"/> string Fuzzy equals to others'.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="cut">字符串相似度的阈值</param>
        ''' <returns></returns>
        <Extension>
        Public Function FuzzyGroups(Of T As INamedValue)(
                        source As IEnumerable(Of T),
               Optional cut As Double = 0.6,
               Optional parallel As Boolean = False) As GroupResult(Of T, String)()

            Return source.FuzzyGroups(Function(x) x.Key, cut, parallel).ToArray
        End Function

        ''' <summary>
        ''' Grouping objects in a collection based on their unique key string Fuzzy equals to others'.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="getKey">The unique key provider</param>
        ''' <param name="cut">字符串相似度的阈值</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 由于list在查找方面的速度非常的慢，而字典可能在生成的时候会慢一些，但是查找很快，所以在这里函数里面使用字典来替代列表
        ''' </remarks>
        <Extension>
        Public Iterator Function FuzzyGroups(Of T)(
                                 source As IEnumerable(Of T),
                                 getKey As Func(Of T, String),
                        Optional cut As Double = 0.6,
                        Optional parallel As Boolean = False) As IEnumerable(Of GroupResult(Of T, String))

            Dim tmp As New List(Of __groupHelper(Of T))
            Dim buf As List(Of __groupHelper(Of T)) =
                LinqAPI.MakeList(Of __groupHelper(Of T)) <= From x As T
                                                            In source
                                                            Let s_key As String = getKey(x)
                                                            Select New __groupHelper(Of T) With {
                                                                .cut = cut,
                                                                .key = s_key,
                                                                .keyASC = s_key.Select(AddressOf Asc).ToArray,
                                                                .x = x
                                                            }
            Dim out As GroupResult(Of T, String)
            Dim lhash As Dictionary(Of __groupHelper(Of T), Object) =
                buf.ToDictionary(Function(x) x, Function(x) Nothing)

            If parallel Then
                Call "Fuzzy grouping running in parallel mode...".debug
            End If

            Do While lhash.Count > 0
                Dim ref As __groupHelper(Of T) = lhash.First.Key

                Call tmp.Clear()
                Call tmp.Add(ref)   ' 重置缓存
                Call lhash.Remove(ref)   ' 写入Group的参考数据

                If parallel Then
                    tmp += LQuerySchedule.LQuery(lhash.Keys, Function(x) x, where:=Function(x) ref.Equals(x:=x))
                Else
                    For Each x As __groupHelper(Of T) In lhash.Values
                        If ref.Equals(x:=x) Then
                            Call tmp.Add(x)
                        End If
                    Next
                End If

                Call Console.Write("-")

                For Each x As __groupHelper(Of T) In tmp
                    Call lhash.Remove(x)
                Next

                Call Console.Write("*")

                out = New GroupResult(Of T, String) With {
                    .Group = tmp.Select(Function(x) x.x).ToArray,
                    .Tag = ref.key
                }
                Yield out
            Loop
        End Function

        ''' <summary>
        ''' 分组操作的内部帮助类
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        Private Structure __groupHelper(Of T)

            ''' <summary>
            ''' Key for represent this object.
            ''' </summary>
            Public key As String
            ''' <summary>
            ''' Target element object in the grouping 
            ''' </summary>
            Public x As T
            Public cut As Double
            ''' <summary>
            ''' Key cache
            ''' </summary>
            Public keyASC As Integer()

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function

            ''' <summary>
            ''' 判断Key是否模糊相等
            ''' </summary>
            ''' <param name="x"></param>
            ''' <returns></returns>
            Public Overloads Function Equals(x As __groupHelper(Of T)) As Boolean
                Dim edits As DistResult = ComputeDistance(
                    keyASC, x.keyASC,
                    Function(a, b) a = b,
                    AddressOf Strings.Chr)

                If edits Is Nothing Then
                    Return False
                Else
                    Return edits.MatchSimilarity >= cut
                End If
            End Function
        End Structure
    End Module
End Namespace
