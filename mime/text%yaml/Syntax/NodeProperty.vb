#Region "Microsoft.VisualBasic::43712e9fa76de6c12eb5ebd7f29c50d5, mime\text%yaml\Syntax\NodeProperty.vb"

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

'     Class NodeProperty
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class NodeProperty

        Public Tag As Tag
        Public Anchor As String

        Public Overrides Function ToString() As String
            Return $"({Anchor}) {Tag.ToString}"
        End Function
    End Class
End Namespace
