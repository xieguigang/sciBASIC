#Region "Microsoft.VisualBasic::30f0809533229e3dc85abb86041f8892, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\VectorAssertExtensions.vb"

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

    '   Total Lines: 62
    '    Code Lines: 33 (53.23%)
    ' Comment Lines: 22 (35.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (11.29%)
    '     File Size: 2.38 KB


    '     Module VectorAssertExtensions
    ' 
    '         Function: InsideAny, LengthEquals, TestPairData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq

Namespace Linq

    <HideModuleName>
    Public Module VectorAssertExtensions

        ''' <summary>
        ''' Determine that is all of the collection <paramref name="array"/> have the same size? 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="n%">collection size Length</param>
        ''' <param name="any">Is required all of the sequence must be the length equals</param>
        ''' <param name="array"></param>
        ''' <returns></returns>
        Public Function LengthEquals(Of T)(n%, any As Boolean, ParamArray array As IEnumerable(Of T)()) As Boolean
            Dim c%() = array.Select(Function(s) s.Count).ToArray
            Dim equals = c.Where(Function(x) x = n).ToArray

            If any Then
                Return equals.Length > 0
            Else
                Return equals.Length = array.Length
            End If
        End Function

        ''' <summary>
        ''' + False: 测试失败，不会满足<see cref="MappingData(Of T)(T(), T())"/>的条件
        ''' + True: 可以使用<see cref="MappingData(Of T)(T(), T())"/>来生成Mapping匹配
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function TestPairData(Of T)(a As T(), b As T()) As Boolean
            If a.Length <> b.Length AndAlso Not LengthEquals(1, True, a, b) Then
                Return False
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' Any of the element in source <paramref name="sites"/> is in a specific <paramref name="range"/>??
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="sites"></param>
        ''' <returns></returns>
        <Extension>
        Public Function InsideAny(range As IntRange, sites As IEnumerable(Of Integer)) As Boolean
            For Each x% In sites
                If range.IsInside(x) Then
                    Return True
                End If
            Next

            Return False
        End Function
    End Module
End Namespace
