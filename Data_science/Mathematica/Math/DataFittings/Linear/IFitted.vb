#Region "Microsoft.VisualBasic::aaddfcffa892d4cc3545f5e58f86ef6a, Data_science\Mathematica\Math\DataFittings\Linear\IFitted.vb"

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

    '   Total Lines: 31
    '    Code Lines: 7 (22.58%)
    ' Comment Lines: 19 (61.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (16.13%)
    '     File Size: 806 B


    ' Interface IFitted
    ' 
    '     Properties: ErrorTest, Polynomial, R2
    ' 
    '     Function: GetY
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' a unify interface model of linear fitting result
''' </summary>
Public Interface IFitted

    ''' <summary>
    ''' 相关系数 R2
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property R2 As Double
    ''' <summary>
    ''' 线性模型的多项式
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property Polynomial As Formula

    ''' <summary>
    ''' 保存拟合后的y值，在拟合时可设置为不保存节省内存
    ''' </summary>
    Property ErrorTest As IFitError()

    ''' <summary>
    ''' f(x) or f(x1, x2, x3)
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Function GetY(ParamArray x As Double()) As Double

End Interface
