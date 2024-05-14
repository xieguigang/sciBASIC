#Region "Microsoft.VisualBasic::56ff105b23674216f6ca7a1419ec8f88, mime\application%pdf\PdfReader\Document\PdfOutlineItem.vb"

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

    '   Total Lines: 52
    '    Code Lines: 43
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.59 KB


    '     Class PdfOutlineItem
    ' 
    '         Properties: A, C, Dest, F, SE
    '                     Title
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Class PdfOutlineItem
        Inherits PdfOutlineLevel

        Private _dictionary As PdfDictionary

        Public Sub New(parent As PdfObject, dictionary As PdfDictionary)
            MyBase.New(parent, dictionary)
            _dictionary = dictionary
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property Title As PdfString
            Get
                Return _dictionary.MandatoryValueRef(Of PdfString)("Title")
            End Get
        End Property

        Public ReadOnly Property Dest As PdfObject
            Get
                Return _dictionary.OptionalValueRef(Of PdfObject)("Dest")
            End Get
        End Property

        Public ReadOnly Property A As PdfDictionary
            Get
                Return _dictionary.OptionalValueRef(Of PdfDictionary)("A")
            End Get
        End Property

        Public ReadOnly Property SE As PdfDictionary
            Get
                Return _dictionary.OptionalValueRef(Of PdfDictionary)("SE")
            End Get
        End Property

        Public ReadOnly Property C As PdfArray
            Get
                Return _dictionary.OptionalValueRef(Of PdfArray)("C")
            End Get
        End Property

        Public ReadOnly Property F As PdfInteger
            Get
                Return _dictionary.OptionalValueRef(Of PdfInteger)("F")
            End Get
        End Property
    End Class
End Namespace
