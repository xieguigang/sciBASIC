#Region "Microsoft.VisualBasic::e9fdb7a0d607be12633b933d716cd1ad, mime\application%pdf\PdfReader\Tokenizer\TokenName.vb"

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
    '     File Size: 1.11 KB


    '     Class TokenName
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
    Public Class TokenName
        Inherits TokenObject

        Private _Value As String
        Private Shared _lookup As ConcurrentDictionary(Of String, TokenName) = New ConcurrentDictionary(Of String, TokenName)()
        Private Shared _nullUpdate As Func(Of String, TokenName, TokenName) = Function(x, y) y

        Public Sub New(name As String)
            Value = name
        End Sub

        Public Property Value As String
            Get
                Return _Value
            End Get
            Private Set(value As String)
                _Value = value
            End Set
        End Property

        Public Shared Function GetToken(name As String) As TokenName
            Dim tokenName As TokenName = Nothing

            If Not _lookup.TryGetValue(name, tokenName) Then
                tokenName = New TokenName(name)
                _lookup.AddOrUpdate(name, tokenName, _nullUpdate)
            End If

            Return tokenName
        End Function
    End Class
End Namespace
