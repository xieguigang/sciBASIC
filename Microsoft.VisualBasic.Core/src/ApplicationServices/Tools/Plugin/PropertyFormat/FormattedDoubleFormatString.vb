#Region "Microsoft.VisualBasic::f2cc2acc479845a6474512ac08698bf7, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\Plugin\PropertyFormat\FormattedDoubleFormatString.vb"

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

    '   Total Lines: 16
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 464 B


    '     Class FormattedDoubleFormatString
    ' 
    '         Properties: FormatString
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Plugin

    <AttributeUsage(AttributeTargets.Property)>
    Public Class FormattedDoubleFormatString : Inherits Attribute

        Public Property FormatString As String

        Sub New(formatString As String)
            Me.FormatString = formatString
        End Sub

        Public Overrides Function ToString() As String
            Return $"format({FormatString})"
        End Function
    End Class
End Namespace
