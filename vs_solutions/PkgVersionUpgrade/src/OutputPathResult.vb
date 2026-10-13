#Region "Microsoft.VisualBasic::4a4d6ef39be1de6cd6b68bed5b331c22, vs_solutions\PkgVersionUpgrade\src\OutputPathResult.vb"

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

    '   Total Lines: 25
    '    Code Lines: 15 (60.00%)
    ' Comment Lines: 5 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (20.00%)
    '     File Size: 963 B


    ' Class OutputPathResult
    ' 
    '     Properties: Changed, Created, DeclarationsAdded, OutputPath, Updated
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>单个工程的输出路径修正结果</summary>
Public Class OutputPathResult

    ''' <summary>被改写或者补写了 OutputPath 的条件组数量</summary>
    Public Property Updated As Integer
    ''' <summary>新建的 nuget_release|x64 条件组数量（0 或 1）</summary>
    Public Property Created As Integer
    ''' <summary>补齐的 Configurations / Platforms 声明条数</summary>
    Public Property DeclarationsAdded As Integer
    ''' <summary>本次计算出的、指向输出文件夹的相对路径，用于日志展示</summary>
    Public Property OutputPath As String

    Public ReadOnly Property Changed As Boolean
        Get
            Return Updated > 0 OrElse Created > 0 OrElse DeclarationsAdded > 0
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

End Class
