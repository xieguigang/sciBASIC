#Region "Microsoft.VisualBasic::e700022e96f1f2062263295cf9f273e1, mime\application%pdf\PdfReader\Document\PdfOutlineLevel.vb"

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

    '   Total Lines: 42
    '    Code Lines: 34 (80.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (19.05%)
    '     File Size: 1.30 KB


    '     Class PdfOutlineLevel
    ' 
    '         Properties: Count, Items
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfOutlineLevel
        Inherits PdfObject

        Private _Items As System.Collections.Generic.List(Of PdfReader.PdfOutlineItem)

        Public Sub New(parent As PdfObject, dictionary As PdfDictionary)
            MyBase.New(parent)
            Items = New List(Of PdfOutlineItem)()

            If dictionary IsNot Nothing Then
                Dim item = dictionary.OptionalValueRef(Of PdfDictionary)("First")

                While item IsNot Nothing
                    Items.Add(New PdfOutlineItem(Me, item))
                    item = item.OptionalValueRef(Of PdfDictionary)("Next")
                End While
            End If
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return Items.Count
            End Get
        End Property

        Public Property Items As List(Of PdfOutlineItem)
            Get
                Return _Items
            End Get
            Private Set(value As List(Of PdfOutlineItem))
                _Items = value
            End Set
        End Property
    End Class
End Namespace
