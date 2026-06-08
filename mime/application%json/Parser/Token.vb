#Region "Microsoft.VisualBasic::a4c4fe8fad8a6d319a21a0198bd96ab6, mime\application%json\Parser\Token.vb"

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

    '   Total Lines: 65
    '    Code Lines: 48 (73.85%)
    ' Comment Lines: 10 (15.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (10.77%)
    '     File Size: 1.90 KB


    ' Class Token
    ' 
    ' 
    '     Enum JSONElements
    ' 
    '         [Boolean], [Double], [Integer], [String], Close
    '         Colon, Comment, Delimiter, Invalid, Key
    '         NewLine, Open, Serial, WhiteSpace
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: GetValue, IsJsonValue
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Class Token : Inherits CodeToken(Of JSONElements)

    Public Enum JSONElements
        Invalid
        [Boolean]
        [Integer]
        [Double]
        [String]
        Open
        Close
        ''' <summary>
        ''' :
        ''' </summary>
        Colon
        Key
        Delimiter
        ''' <summary>
        ''' hjson comment
        ''' </summary>
        ''' <remarks>
        ''' just parse the hjson comment, this module will not save the
        ''' comment data when do json serialization
        ''' </remarks>
        Comment
        Serial
        NewLine
        WhiteSpace
    End Enum

    Sub New(type As JSONElements, text As String)
        Call MyBase.New(type, text)
    End Sub

    Sub New(type As JSONElements, buffer As Char())
        Call MyBase.New(type, New String(buffer))
    End Sub

    Public Function GetValue() As JsonValue
        Select Case name
            Case JSONElements.String
                Return New JsonValue(text)
            Case JSONElements.Boolean
                Return New JsonValue(text.ParseBoolean)
            Case JSONElements.Double
                Return New JsonValue(Val(text))
            Case JSONElements.Integer
                Return New JsonValue(Long.Parse(text))
            Case Else
                Throw New InvalidCastException($"{name.Description} could not be cast to a literal value!")
        End Select
    End Function

    Public Function IsJsonValue() As Boolean
        Select Case name
            Case JSONElements.Boolean, JSONElements.Double, JSONElements.Integer, JSONElements.String
                Return True
            Case Else
                Return False
        End Select
    End Function

End Class
