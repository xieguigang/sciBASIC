#Region "Microsoft.VisualBasic::b210ca51ba20d26b804e256c354d88f0, mime\application%pdf\PdfReader\Tokenizer\TokenIdentifier.vb"

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

    '   Total Lines: 36
    '    Code Lines: 29 (80.56%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (19.44%)
    '     File Size: 1.23 KB


    '     Class TokenIdentifier
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetToken
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Concurrent

Namespace PdfReader
    Public Class TokenIdentifier
        Inherits TokenObject

        Private _Value As String
        Private Shared _lookup As ConcurrentDictionary(Of String, TokenIdentifier) = New ConcurrentDictionary(Of String, TokenIdentifier)()
        Private Shared _nullUpdate As Func(Of String, TokenIdentifier, TokenIdentifier) = Function(x, y) y

        Public Sub New(identifier As String)
            Value = identifier
        End Sub

        Public Property Value As String
            Get
                Return _Value
            End Get
            Private Set(value As String)
                _Value = value
            End Set
        End Property

        Public Shared Function GetToken(identifier As String) As TokenIdentifier
            Dim tokenIdentifier As TokenIdentifier = Nothing

            If Not _lookup.TryGetValue(identifier, tokenIdentifier) Then
                tokenIdentifier = New TokenIdentifier(identifier)
                _lookup.AddOrUpdate(identifier, tokenIdentifier, _nullUpdate)
            End If

            Return tokenIdentifier
        End Function
    End Class
End Namespace
