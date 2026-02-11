#Region "Microsoft.VisualBasic::32ea7225156f9c941327a5a9f0f2a7c5, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\base\Combination\CombinationExtensions.vb"

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

    '   Total Lines: 47
    '    Code Lines: 29 (61.70%)
    ' Comment Lines: 10 (21.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (17.02%)
    '     File Size: 1.64 KB


    '     Module CombinationExtensions
    ' 
    '         Function: AllCombinations, Generate, (+2 Overloads) Iterates
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' 任意多个集合之间的对象之间相互组成组合输出
    ''' </summary>
    ''' <remarks></remarks>
    Public Module CombinationExtensions

        ''' <summary>
        ''' wrapper call for <see cref="NDimensionCartesianProduct.CreateMultiCartesianProduct"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Generate(Of T)(source As T()()) As IEnumerable(Of T())
            Return NDimensionCartesianProduct.CreateMultiCartesianProduct(Of T)(source)
        End Function

        <Extension>
        Public Iterator Function AllCombinations(Of T)(source As T(), Optional size As Integer = -1) As IEnumerable(Of T())
            If size <= 0 Then
                size = source.Length
            End If

            Dim combinator As New Combination(Of T)(source, size)

            Do While combinator.CanRun
                Yield combinator.Execute
            Loop
        End Function

        <Extension>
        Public Iterator Function Iterates(Of T)(comb As (T, T)) As IEnumerable(Of T)
            Yield comb.Item1
            Yield comb.Item2
        End Function

        <Extension>
        Public Iterator Function Iterates(Of T)(comb As Tuple(Of T, T)) As IEnumerable(Of T)
            Yield comb.Item1
            Yield comb.Item2
        End Function
    End Module
End Namespace
