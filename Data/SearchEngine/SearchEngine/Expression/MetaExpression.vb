#Region "Microsoft.VisualBasic::70d402beae714073189a913489113cfe, sciBASIC#\Data\SearchEngine\SearchEngine\Expression\MetaExpression.vb"

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

    '   Total Lines: 19
    '    Code Lines: 8
    ' Comment Lines: 8
    '   Blank Lines: 3
    '     File Size: 708.00 B


    ' Class MetaExpression
    ' 
    '     Properties: [Operator], Expression
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ``&lt;expr> &lt;opr>``，假如是以NOT操作符起始的元表达式，则<see cref="MetaExpression.Expression"/>属性为空
''' </summary>
Public Class MetaExpression

    Public Property [Operator] As Tokens
    ''' <summary>
    ''' Public <see cref="System.Delegate"/> Function <see cref="IAssertion"/>(data As <see cref="IObject"/>) As <see cref="Boolean"/>.
    ''' (这个可能是包含有括号运算的表达式)
    ''' </summary>
    ''' <returns></returns>
    Public Property Expression As IAssertion

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
