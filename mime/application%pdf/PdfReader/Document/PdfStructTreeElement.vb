#Region "Microsoft.VisualBasic::fe4b3e25b75f0cb077ef652fa237e67e, mime\application%pdf\PdfReader\Document\PdfStructTreeElement.vb"

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

    '   Total Lines: 114
    '    Code Lines: 93
    ' Comment Lines: 0
    '   Blank Lines: 21
    '     File Size: 3.94 KB


    '     Class PdfStructTreeElement
    ' 
    '         Properties: A, ActualText, Alt, C, E
    '                     ID, K, Lang, Pg, R
    '                     S, T
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports Microsoft.VisualBasic.Language

Namespace PdfReader
    Public Class PdfStructTreeElement
        Inherits PdfDictionary

        Private _elements As List(Of PdfStructTreeElement)

        Public Sub New(dictionary As PdfDictionary)
            MyBase.New(dictionary.Parent, dictionary.ParseDictionary)
        End Sub

        Public ReadOnly Property S As PdfName
            Get
                Return MandatoryValue(Of PdfName)("S")
            End Get
        End Property

        Public ReadOnly Property ID As PdfString
            Get
                Return OptionalValue(Of PdfString)("ID")
            End Get
        End Property

        Public ReadOnly Property Pg As PdfObjectReference
            Get
                Return OptionalValue(Of PdfObjectReference)("Pg")
            End Get
        End Property

        Public ReadOnly Property K As List(Of PdfStructTreeElement)
            Get
                Dim dictionary As New Value(Of PdfDictionary)
                Dim array As New Value(Of PdfArray)
                Dim reference As New Value(Of PdfObjectReference)

                If _elements Is Nothing Then
                    _elements = New List(Of PdfStructTreeElement)()
                    Dim lK = OptionalValueRef(Of PdfObject)("K")

                    If (dictionary = TryCast(lK, PdfDictionary)) IsNot Nothing Then
                        _elements.Add(New PdfStructTreeElement(dictionary))
                    ElseIf (array = TryCast(lK, PdfArray)) IsNot Nothing Then

                        For Each item As PdfObject In CType(array, PdfArray).Objects
                            dictionary = TryCast(item, PdfDictionary)

                            If dictionary Is Nothing Then
                                If (reference = TryCast(item, PdfObjectReference)) IsNot Nothing Then
                                    dictionary = Document.IndirectObjects.MandatoryValue(Of PdfDictionary)(reference)
                                Else
                                    Throw New ApplicationException($"PdfStructTreeElement property K has unrecognized content of type '{item.GetType().Name}'.")
                                End If
                            End If

                            _elements.Add(New PdfStructTreeElement(dictionary))
                        Next
                    End If
                End If

                Return _elements
            End Get
        End Property

        Public ReadOnly Property A As PdfObject
            Get
                Return OptionalValue(Of PdfObject)("A")
            End Get
        End Property

        Public ReadOnly Property C As PdfObject
            Get
                Return OptionalValue(Of PdfObject)("C")
            End Get
        End Property

        Public ReadOnly Property R As PdfInteger
            Get
                Return OptionalValue(Of PdfInteger)("R")
            End Get
        End Property

        Public ReadOnly Property T As PdfString
            Get
                Return OptionalValue(Of PdfString)("T")
            End Get
        End Property

        Public ReadOnly Property Lang As PdfString
            Get
                Return OptionalValue(Of PdfString)("Lang")
            End Get
        End Property

        Public ReadOnly Property Alt As PdfString
            Get
                Return OptionalValue(Of PdfString)("Alt")
            End Get
        End Property

        Public ReadOnly Property E As PdfString
            Get
                Return OptionalValue(Of PdfString)("E")
            End Get
        End Property

        Public ReadOnly Property ActualText As PdfString
            Get
                Return OptionalValue(Of PdfString)("ActualText")
            End Get
        End Property
    End Class
End Namespace
