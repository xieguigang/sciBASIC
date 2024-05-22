#Region "Microsoft.VisualBasic::2843442d7f9c74cde1791d4f1ce8ab15, Microsoft.VisualBasic.Core\src\Language\Language\Python\Collection.vb"

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

    '   Total Lines: 55
    '    Code Lines: 25 (45.45%)
    ' Comment Lines: 23 (41.82%)
    '    - Xml Docs: 95.65%
    ' 
    '   Blank Lines: 7 (12.73%)
    '     File Size: 2.12 KB


    '     Module Collection
    ' 
    '         Function: slice, SpanSlice
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.Python

    Public Module Collection

        ''' <summary>
        ''' 将序列之中的指定区域的内容取出来
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="[set]"></param>
        ''' <param name="start"></param>
        ''' <param name="[stop]">
        ''' 可以接受负数，如果为负数，则表示终止的下标为长度将去这个stop值的结果，即从后往前数
        ''' </param>
        ''' <param name="[step]"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 20210810 test success by kdtree
        ''' </remarks>
        <Extension>
        Public Iterator Function slice(Of T)([set] As IEnumerable(Of T),
                                             Optional start% = 0,
                                             Optional stop% = -1,
                                             Optional step% = 1) As IEnumerable(Of T)

            Dim array As T() = [set].ToArray

            If [stop] < 0 Then
                [stop] = array.Length + [stop]
            Else
                [stop] -= 1
            End If

            For i As Integer = start To [stop] Step [step]
                Yield array(i)
            Next
        End Function

        ''' <summary>
        ''' Forms a slice out of the current span starting at a specified index for a specified length.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="span"></param>
        ''' <param name="start">The index at which to begin this slice.</param>
        ''' <param name="length">The desired length for the slice.</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SpanSlice(Of T)(span As IEnumerable(Of T), start As Integer, length As Integer) As IEnumerable(Of T)
            Return span.slice(start, [stop]:=start + length)
        End Function
    End Module
End Namespace
