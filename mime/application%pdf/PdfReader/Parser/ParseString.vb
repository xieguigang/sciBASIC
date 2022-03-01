#Region "Microsoft.VisualBasic::fc11eb05aed966ef5571f424a36cb1a3, mime\application%pdf\PdfReader\Parser\ParseString.vb"

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

    '     Class ParseString
    ' 
    '         Properties: Token, Value, ValueAsBytes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BytesToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Class ParseString
        Inherits ParseObjectBase

        Public Sub New(token As TokenString)
            Me.Token = token
        End Sub

        Public ReadOnly Property Value As String
            Get
                Return Token.Resolved
            End Get
        End Property

        Public ReadOnly Property ValueAsBytes As Byte()
            Get
                Return Token.ResolvedAsBytes
            End Get
        End Property

        Public Function BytesToString(bytes As Byte()) As String
            Return Token.BytesToString(bytes)
        End Function

        Private Property Token As TokenString
    End Class
End Namespace
