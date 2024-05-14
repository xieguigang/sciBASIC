#Region "Microsoft.VisualBasic::00cfe605bd6029497448cc5a909910c6, mime\application%pdf\PdfReader\Document\PdfStructTreeRoot.vb"

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

    '   Total Lines: 82
    '    Code Lines: 67
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 3.29 KB


    '     Class PdfStructTreeRoot
    ' 
    '         Properties: ClassMap, IDTree, K, ParentTree, ParentTreeNextKey
    '                     RoleMap
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports Microsoft.VisualBasic.Language

Namespace PdfReader
    Public Class PdfStructTreeRoot
        Inherits PdfDictionary

        Private _elements As List(Of PdfStructTreeElement)
        Private _IdTree As PdfNameTree
        Private _parentTree As PdfNumberTree

        Public Sub New(parent As PdfObject, dictionary As ParseDictionary)
            MyBase.New(parent, dictionary)
        End Sub

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
                                    Throw New ApplicationException($"PdfStructTreeRoot property K with array must contain dictionary or object reference and not '{item.GetType().Name}'.")
                                End If
                            End If

                            _elements.Add(New PdfStructTreeElement(dictionary))
                        Next
                    End If
                End If

                Return _elements
            End Get
        End Property

        Public ReadOnly Property IDTree As PdfNameTree
            Get
                If _IdTree Is Nothing Then _IdTree = New PdfNameTree(MandatoryValueRef(Of PdfDictionary)("IDTree"))
                Return _IdTree
            End Get
        End Property

        Public ReadOnly Property ParentTree As PdfNumberTree
            Get
                If _parentTree Is Nothing Then _parentTree = New PdfNumberTree(MandatoryValueRef(Of PdfDictionary)("ParentTree"))
                Return _parentTree
            End Get
        End Property

        Public ReadOnly Property ParentTreeNextKey As PdfInteger
            Get
                Return OptionalValue(Of PdfInteger)("ParentTreeNextKey")
            End Get
        End Property

        Public ReadOnly Property RoleMap As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("RoleMap")
            End Get
        End Property

        Public ReadOnly Property ClassMap As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("ClassMap")
            End Get
        End Property
    End Class
End Namespace
