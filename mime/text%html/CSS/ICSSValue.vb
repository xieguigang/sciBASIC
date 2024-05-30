#Region "Microsoft.VisualBasic::47f3f8a920a4cbc15f64bbea7c49fb9c, mime\text%html\CSS\ICSSValue.vb"

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
    '    Code Lines: 8 (42.11%)
    ' Comment Lines: 8 (42.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (15.79%)
    '     File Size: 552 B


    '     Class ICSSValue
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CSS

    ''' <summary>
    ''' an abstract object model for get <see cref="CSSValue"/> 
    ''' string from current object.
    ''' </summary>
    Public MustInherit Class ICSSValue

        ''' <summary>
        ''' the css string generator based on current object model
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property CSSValue As String

        Public Overrides Function ToString() As String
            Return CSSValue
        End Function
    End Class
End Namespace
