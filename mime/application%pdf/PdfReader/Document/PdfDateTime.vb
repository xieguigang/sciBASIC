#Region "Microsoft.VisualBasic::d1a26c565fbb53add348a72b5fda1a2c, mime\application%pdf\PdfReader\Document\PdfDateTime.vb"

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

    '     Class PdfDateTime
    ' 
    '         Properties: DateTime
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Text

Namespace PdfReader
    Public Class PdfDateTime
        Inherits PdfReader.PdfString

        Private _DateTime As System.DateTime

        Public Sub New(parent As PdfReader.PdfObject, str As PdfReader.PdfString)
            MyBase.New(parent, TryCast(str.ParseObject, PdfReader.ParseString))
            Me.DateTime = str.ValueAsDateTime
        End Sub

        Public Overrides Function ToString() As String
            Return Me.DateTime.ToString()
        End Function

        Public Overrides Sub Visit(visitor As PdfReader.IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property DateTime As System.DateTime
            Get
                Return _DateTime
            End Get
            Private Set(value As System.DateTime)
                _DateTime = value
            End Set
        End Property
    End Class
End Namespace
