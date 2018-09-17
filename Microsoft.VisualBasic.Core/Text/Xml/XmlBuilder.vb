#Region "Microsoft.VisualBasic::011cb04c7d4f4e8ed121fca513cefed1, Microsoft.VisualBasic.Core\Text\Xml\XmlBuilder.vb"

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

    '     Class XmlBuilder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.SymbolBuilder

Namespace Text.Xml.Models

    ''' <summary>
    ''' Builder for XML and html
    ''' </summary>
    Public Class XmlBuilder : Inherits ScriptBuilder

        Sub New()
            Call MyBase.New(1024)
        End Sub

        Public Overloads Shared Operator +(xb As XmlBuilder, node As XElement) As XmlBuilder
            Call xb.Script.AppendLine(node.ToString)
            Return xb
        End Operator
    End Class
End Namespace
