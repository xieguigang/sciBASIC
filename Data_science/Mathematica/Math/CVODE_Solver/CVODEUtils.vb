#Region "Microsoft.VisualBasic::b30875eaa1f693967dfe024d01fce305, Data_science\Mathematica\Math\CVODE_Solver\CVODEUtils.vb"

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

    '   Total Lines: 51
    '    Code Lines: 30 (58.82%)
    ' Comment Lines: 15 (29.41%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (11.76%)
    '     File Size: 1.41 KB


    ' Class CVODEUtils
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: DefaultOptions, HighPrecisionOptions, NonStiffOptions, StiffOptions
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' CVODE工具类
''' </summary>
Public NotInheritable Class CVODEUtils

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 创建默认选项
    ''' </summary>
    Public Shared Function DefaultOptions() As CVODEOptions
        Return New CVODEOptions()
    End Function

    ''' <summary>
    ''' 创建刚性问题的默认选项
    ''' </summary>
    Public Shared Function StiffOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.000001,
            .AbsoluteTolerance = 0.00000001,
            .MaxOrder = 5,
            .MaxNewtonIterations = 10
        }
    End Function

    ''' <summary>
    ''' 创建非刚性问题的默认选项
    ''' </summary>
    Public Shared Function NonStiffOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.000001,
            .AbsoluteTolerance = 0.00000001,
            .MaxOrder = 12
        }
    End Function

    ''' <summary>
    ''' 创建高精度选项
    ''' </summary>
    Public Shared Function HighPrecisionOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.0000000001,
            .AbsoluteTolerance = 0.000000000001,
            .MaxOrder = 5,
            .MaxNewtonIterations = 10
        }
    End Function

End Class
