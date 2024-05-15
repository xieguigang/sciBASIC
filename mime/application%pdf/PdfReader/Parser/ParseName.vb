#Region "Microsoft.VisualBasic::05f688a5f9d248c33ab0aec76fa7d318, mime\application%pdf\PdfReader\Parser\ParseName.vb"

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
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.12 KB


    '     Class ParseName
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Concurrent

Namespace PdfReader
    Public Class ParseName
        Inherits ParseObjectBase

        Private _Value As String
        Private Shared _lookup As ConcurrentDictionary(Of String, ParseName) = New ConcurrentDictionary(Of String, ParseName)()
        Private Shared _nullUpdate As Func(Of String, ParseName, ParseName) = Function(x, y) y

        Public Sub New(value As String)
            Me.Value = value
        End Sub

        Public Property Value As String
            Get
                Return _Value
            End Get
            Private Set(value As String)
                _Value = value
            End Set
        End Property

        Public Shared Function GetParse(name As String) As ParseName
            Dim parseName As ParseName = Nothing

            If Not _lookup.TryGetValue(name, parseName) Then
                parseName = New ParseName(name)
                _lookup.AddOrUpdate(name, parseName, _nullUpdate)
            End If

            Return parseName
        End Function
    End Class
End Namespace
