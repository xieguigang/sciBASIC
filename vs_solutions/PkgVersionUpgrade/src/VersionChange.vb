#Region "Microsoft.VisualBasic::ae59f1645b76c179e90ff7f35dac8fd6, vs_solutions\PkgVersionUpgrade\src\VersionChange.vb"

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

    '   Total Lines: 27
    '    Code Lines: 15 (55.56%)
    ' Comment Lines: 7 (25.93%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (18.52%)
    '     File Size: 1.06 KB


    ' Class VersionChange
    ' 
    '     Properties: Changed, Inserted, Name, NewValue, OldValue
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region


''' <summary>
''' 单个版本元素的变更记录，用于 dry-run 预览以及最终的统计汇总
''' </summary>
Public Class VersionChange

    ''' <summary>版本元素的名称，例如 Version / AssemblyVersion / FileVersion</summary>
    Public Property Name As String
    ''' <summary>写入之前的值，元素原本不存在时为空字符串</summary>
    Public Property OldValue As String
    ''' <summary>本次计算出的新值</summary>
    Public Property NewValue As String
    ''' <summary>该元素是否是本次新建出来的</summary>
    Public Property Inserted As Boolean

    Public ReadOnly Property Changed As Boolean
        Get
            Return Inserted OrElse Not String.Equals(OldValue, NewValue, StringComparison.Ordinal)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim from As String = If(Inserted, "<none>", If(String.IsNullOrEmpty(OldValue), "<empty>", OldValue))
        Return $"{Name}: {from} -> {NewValue}"
    End Function

End Class
