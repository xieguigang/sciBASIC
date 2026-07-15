#Region "Microsoft.VisualBasic::790f07252af349ef9c716f55a3d9999a, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\Enums\SvgStrokeLineCap.vb"

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

'   Total Lines: 14
'    Code Lines: 11 (78.57%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 3 (21.43%)
'     File Size: 507 B


'     Class SvgStrokeLineCap
' 
'         Properties: Butt, Round, Square
' 
'         Constructor: (+1 Overloads) Sub New
' 
' 
' /********************************************************************************/

#End Region

Namespace SVG.XML.Enums

    Public Class SvgStrokeLineCap : Inherits SvgEnum

        Public Const cap_butt As String = "butt"
        Public Const cap_round As String = "round"
        Public Const cap_square As String = "square"

        Public Shared ReadOnly Property Butt As New SvgStrokeLineCap(cap_butt)
        Public Shared ReadOnly Property Round As New SvgStrokeLineCap(cap_round)
        Public Shared ReadOnly Property Square As New SvgStrokeLineCap(cap_square)

        Public ReadOnly Property [enum] As String
            Get
                Return _value
            End Get
        End Property

        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

    End Class
End Namespace
