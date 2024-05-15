#Region "Microsoft.VisualBasic::c09791bb244fb80b0519f844934ff396, Data\GraphQuery\Language\QueryToken.vb"

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

    '   Total Lines: 37
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 937 B


    '     Class QueryToken
    ' 
    '         Properties: func, name, text, token
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language

    Public Class QueryToken

        Public Property token As Token
        Public Property func As Parser

        Public ReadOnly Property name As Tokens
            Get
                If token Is Nothing Then
                    Return Tokens.NA
                Else
                    Return token.name
                End If
            End Get
        End Property

        Public ReadOnly Property text As String
            Get
                If token Is Nothing Then
                    Return ""
                Else
                    Return token.text
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            If token Is Nothing Then
                Return func.ToString
            Else
                Return token.text
            End If
        End Function

    End Class
End Namespace
