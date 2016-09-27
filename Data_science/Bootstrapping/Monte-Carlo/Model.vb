#Region "Microsoft.VisualBasic::074c8f697dfd2a0d4a95c4b1ad7aa72d, ..\visualbasic_App\Data_science\Bootstrapping\Monte-Carlo\Model.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace MonteCarlo

    Public MustInherit Class Model : Inherits ODEs

        ''' <summary>
        ''' 系统的初始值列表
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function yinit() As NamedValue(Of PreciseRandom)()
        ''' <summary>
        ''' 系统的状态列表，即方程里面的参数
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function params() As NamedValue(Of PreciseRandom)()
        Public MustOverride Function eigenvector() As Dictionary(Of String, Eigenvector)

        Protected NotOverridable Overrides Function y0() As var()
            Return {}
        End Function
    End Class
End Namespace
