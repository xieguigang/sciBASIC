#Region "Microsoft.VisualBasic::c461eec78f678476fd08ab6c61453801, Data_science\Mathematica\Math\Math\Scripting\PlotCommand.vb"

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

    '   Total Lines: 42
    '    Code Lines: 29 (69.05%)
    ' Comment Lines: 8 (19.05%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.90%)
    '     File Size: 1.41 KB


    '     Enum PlotKind
    ' 
    '         Line, Scatter, Surface
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class PlotCommand
    ' 
    '         Properties: Is3D
    ' 
    '     Class ScriptResult
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting

    ''' <summary>绘图类型</summary>
    Public Enum PlotKind
        Scatter
        Line
        Surface
    End Enum

    ''' <summary>
    ''' 一条绘图指令（纯数据，不引用任何绘图组件）。
    ''' 由数学脚本引擎产出，供可视化程序解释渲染。
    ''' </summary>
    Public Class PlotCommand
        Public Kind As PlotKind = PlotKind.Scatter
        Public X As Double() = {}
        Public Y As Double() = {}
        ''' <summary>三维散点/曲线可选；二维时为 Nothing</summary>
        Public Z As Double() = Nothing
        ''' <summary>曲面：ZGrid(i)(j)，i 沿 Y 轴、j 沿 X 轴</summary>
        Public ZGrid As Double()() = Nothing
        Public Scheme As String = "viridis"
        Public Label As String = ""

        Public ReadOnly Property Is3D As Boolean
            Get
                If Kind = PlotKind.Surface Then Return True
                Return Z IsNot Nothing
            End Get
        End Property
    End Class

    ''' <summary>脚本执行结果</summary>
    Public Class ScriptResult
        Public Variables As New Dictionary(Of String, Object)()
        Public Commands As New List(Of PlotCommand)()
        Public ErrorMessage As String = ""
        Public Success As Boolean = True
        Public Line As Integer = -1
    End Class

End Namespace

