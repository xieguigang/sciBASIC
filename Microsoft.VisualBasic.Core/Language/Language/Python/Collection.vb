#Region "Microsoft.VisualBasic::5a7b6b44add963ef9905b38607d3b3ac, Microsoft.VisualBasic.Core\Language\Language\Python\Collection.vb"

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

'     Module Collection
' 
'         Function: slice
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
        <Extension>
        Public Iterator Function slice(Of T)([set] As IEnumerable(Of T), start%, stop%, Optional step% = 1) As IEnumerable(Of T)
            Dim array As T() = [set].Skip(start).ToArray

            If [stop] < 0 Then
                [stop] = array.Length + [stop]
            End If

            [stop] -= start
            [stop] -= 1

            For i As Integer = 0 To [stop] Step [step]
                Yield array(i)
            Next
        End Function
    End Module
End Namespace
