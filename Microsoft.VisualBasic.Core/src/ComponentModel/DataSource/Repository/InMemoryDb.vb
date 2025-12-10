#Region "Microsoft.VisualBasic::613a5e2782e9f04919acfbebca5be083, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\InMemoryDb.vb"

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

    '   Total Lines: 30
    '    Code Lines: 17 (56.67%)
    ' Comment Lines: 5 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (26.67%)
    '     File Size: 1.09 KB


    '     Class InMemoryDb
    ' 
    '         Function: [Get]
    ' 
    '         Sub: Put
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace ComponentModel.DataSourceModel.Repository

    Public MustInherit Class InMemoryDb

        ''' <summary>
        ''' 枚举数据库中所有的键。
        ''' 此操作会遍历所有数据文件，可能比较耗时，建议在需要时调用。
        ''' </summary>
        ''' <returns>返回一个包含所有键的字符串集合。</returns>
        Public MustOverride Iterator Function EnumerateAllKeys() As IEnumerable(Of Byte())

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Get](key As String) As Byte()
            Return [Get](Encoding.UTF8.GetBytes(key))
        End Function

        Public MustOverride Function [Get](keydata As Byte()) As Byte()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Put(key As String, data As Byte())
            Call Put(Encoding.UTF8.GetBytes(key), data)
        End Sub

        Public MustOverride Sub Put(keybuf As Byte(), data As Byte())

    End Class
End Namespace
