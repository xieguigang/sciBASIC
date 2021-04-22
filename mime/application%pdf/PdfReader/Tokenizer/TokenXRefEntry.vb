#Region "Microsoft.VisualBasic::9af5c243668c0e8e9ba3daceaf9faeb2, mime\application%pdf\PdfReader\Tokenizer\TokenXRefEntry.vb"

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

    '     Class TokenXRefEntry
    ' 
    '         Properties: Gen, Id, Offset, Used
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Class TokenXRefEntry
        Inherits TokenObject

        Private _Id As Integer, _Gen As Integer, _Offset As Long, _Used As Boolean

        Public Sub New(ByVal id As Integer, ByVal gen As Integer, ByVal offset As Long, ByVal used As Boolean)
            Me.Id = id
            Me.Gen = gen
            Me.Offset = offset
            Me.Used = used
        End Sub

        Public Property Id As Integer
            Get
                Return _Id
            End Get
            Private Set(ByVal value As Integer)
                _Id = value
            End Set
        End Property

        Public Property Gen As Integer
            Get
                Return _Gen
            End Get
            Private Set(ByVal value As Integer)
                _Gen = value
            End Set
        End Property

        Public Property Offset As Long
            Get
                Return _Offset
            End Get
            Private Set(ByVal value As Long)
                _Offset = value
            End Set
        End Property

        Public Property Used As Boolean
            Get
                Return _Used
            End Get
            Private Set(ByVal value As Boolean)
                _Used = value
            End Set
        End Property
    End Class
End Namespace

