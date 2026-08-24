#Region "Microsoft.VisualBasic::df15c626520da9d51946428df197f470, Microsoft.VisualBasic.Core\src\Extensions\StringHelpers\StringBuilders.vb"

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

    '   Total Lines: 59
    '    Code Lines: 27 (45.76%)
    ' Comment Lines: 26 (44.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (10.17%)
    '     File Size: 2.01 KB


    ' Module StringBuilders
    ' 
    '     Function: (+2 Overloads) Replace, ToHex, ToHexString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' <see cref="StringBuilder"/> helpers
''' </summary>
Public Module StringBuilders

    ''' <summary>
    ''' Convert to string via <see cref="BitConverter.ToString"/> with - was stripped.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ToHex(data As Byte()) As String
        Return BitConverter.ToString(data).Replace("-", "")
    End Function

    ''' <summary>
    ''' Cast byte data as hex string via byte tostring('x2') 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ToHexString(data As IEnumerable(Of Byte)) As String
        Return data.SafeQuery.Select(Function(b) b.ToString("x2")).JoinBy("")
    End Function

    ''' <summary>
    ''' 批量进行替换操作
    ''' </summary>
    ''' <param name="sb"></param>
    ''' <param name="replacements"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Replace(sb As StringBuilder, ParamArray replacements As NamedValue(Of String)()) As StringBuilder
        For Each tuple As NamedValue(Of String) In replacements.SafeQuery
            Call sb.Replace(tuple.Name, tuple.Value)
        Next

        Return sb
    End Function

    ''' <summary>
    ''' 适用于更加复杂的结果值的产生的链式替换
    ''' </summary>
    ''' <param name="sb"></param>
    ''' <param name="find$"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Replace(sb As StringBuilder, find$, value As Func(Of String)) As StringBuilder
        Return sb.Replace(find, value())
    End Function
End Module
