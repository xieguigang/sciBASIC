#Region "Microsoft.VisualBasic::8ceb492cdd712a59a5fb8f23f3da977e, mime\application%pdf\PdfReader\Document\PdfObject.vb"

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

    '   Total Lines: 184
    '    Code Lines: 153 (83.15%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 31 (16.85%)
    '     File Size: 7.71 KB


    '     Class PdfObject
    ' 
    '         Properties: Decrypt, Document, Parent, ParseObject
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ArrayToRectangle, AsArray, AsBoolean, AsInteger, AsNumber
    '                   AsNumberArray, AsString, ToString, TypedParent, WrapObject
    ' 
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports Microsoft.VisualBasic.Language

Namespace PdfReader
    Public MustInherit Class PdfObject
        Private _Parent As PdfReader.PdfObject, _ParseObject As PdfReader.ParseObjectBase

        Public Sub New(parent As PdfObject)
            Me.New(parent, Nothing)
        End Sub

        Public Sub New(parent As PdfObject, parse As ParseObjectBase)
            Me.Parent = parent
            ParseObject = parse
        End Sub

        Public Overrides Function ToString() As String
            Return $"({[GetType]().Name})"
        End Function

        Public Overridable Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property Parent As PdfObject
            Get
                Return _Parent
            End Get
            Private Set(value As PdfObject)
                _Parent = value
            End Set
        End Property

        Public Property ParseObject As ParseObjectBase
            Get
                Return _ParseObject
            End Get
            Private Set(value As ParseObjectBase)
                _ParseObject = value
            End Set
        End Property

        Public ReadOnly Property Document As PdfDocument
            Get
                Return TypedParent(Of PdfDocument)()
            End Get
        End Property

        Public ReadOnly Property Decrypt As PdfDecrypt
            Get
                Return TypedParent(Of PdfDocument)().DecryptHandler
            End Get
        End Property

        Public Function TypedParent(Of T As PdfObject)() As T
            Dim parent = Me.Parent

            While parent IsNot Nothing

                If TypeOf parent Is T Then
                    Return TryCast(parent, T)
                Else
                    parent = parent.Parent
                End If
            End While

            Return Nothing
        End Function

        Public Function AsBoolean() As Boolean
            Dim [boolean] As New Value(Of PdfBoolean)
            If ([boolean] = TryCast(Me, PdfBoolean)) IsNot Nothing Then Return [boolean].Value
            Throw New ApplicationException($"Unexpected object in content '{[GetType]().Name}', expected a boolean.")
        End Function

        Public Function AsString() As String
            Dim name As New Value(Of PdfName)
            Dim str As New Value(Of PdfString)

            If (name = TryCast(Me, PdfName)) IsNot Nothing Then
                Return name.Value.StrVal
            ElseIf (str = TryCast(Me, PdfString)) IsNot Nothing Then
                Return str.Value.StrVal
            End If

            Throw New ApplicationException($"Unexpected object in content '{[GetType]().Name}', expected a string.")
        End Function

        Public Function AsInteger() As Integer
            Dim [integer] As New Value(Of PdfInteger)
            If ([integer] = TryCast(Me, PdfInteger)) IsNot Nothing Then Return [integer].Value
            Throw New ApplicationException($"Unexpected object in content '{[GetType]().Name}', expected an integer.")
        End Function

        Public Function AsNumber() As Single
            Dim [integer] As New Value(Of PdfInteger)
            Dim real As New Value(Of PdfReal)

            If ([integer] = TryCast(Me, PdfInteger)) IsNot Nothing Then
                Return [integer].Value
            ElseIf (real = TryCast(Me, PdfReal)) IsNot Nothing Then
                Return real.Value
            End If

            Throw New ApplicationException($"Unexpected object in content '{[GetType]().Name}', expected a number.")
        End Function

        Public Function AsNumberArray() As Single()
            Dim array As New Value(Of PdfArray)
            Dim [integer] As New Value(Of PdfInteger)
            Dim real As New Value(Of PdfReal)

            If (array = TryCast(Me, PdfArray)) IsNot Nothing Then
                Dim numbers As List(Of Single) = New List(Of Single)()

                For Each item As PdfObject In CType(array, PdfArray).Objects
                    If ([integer] = TryCast(item, PdfInteger)) IsNot Nothing Then
                        numbers.Add([integer].Value)
                    ElseIf (real = TryCast(item, PdfReal)) IsNot Nothing Then
                        numbers.Add(real.Value)
                    Else
                        Throw New ApplicationException($"Array contains object of type '{[GetType]().Name}', expected only numbers.")
                    End If
                Next

                Return numbers.ToArray()
            End If

            Throw New ApplicationException($"Unexpected object in content '{[GetType]().Name}', expected an integer array.")
        End Function

        Public Function AsArray() As List(Of PdfObject)
            Dim array As New Value(Of PdfArray)
            If (array = TryCast(Me, PdfArray)) IsNot Nothing Then Return CType(array, PdfArray).Objects
            Throw New ApplicationException($"Unexpected object in content '{[GetType]().Name}', expected an integer array.")
        End Function

        Public Function WrapObject(obj As ParseObjectBase) As PdfObject
            Dim str As New Value(Of ParseString)
            If (str = TryCast(obj, ParseString)) IsNot Nothing Then Return New PdfString(Me, str)
            Dim name As New Value(Of ParseName)
            Dim [integer] As New Value(Of ParseInteger)
            Dim real As New Value(Of ParseReal)
            Dim dictionary As New Value(Of ParseDictionary)
            Dim reference As New Value(Of ParseObjectReference)
            Dim stream As New Value(Of ParseStream)
            Dim array As New Value(Of ParseArray)
            Dim identifier As New Value(Of ParseIdentifier)
            Dim [boolean] As New Value(Of ParseBoolean)

            If (name = TryCast(obj, ParseName)) IsNot Nothing Then
                Return New PdfName(Me, name)
            ElseIf ([integer] = TryCast(obj, ParseInteger)) IsNot Nothing Then
                Return New PdfInteger(Me, [integer])
            ElseIf (real = TryCast(obj, ParseReal)) IsNot Nothing Then
                Return New PdfReal(Me, real)
            ElseIf (dictionary = TryCast(obj, ParseDictionary)) IsNot Nothing Then
                Return New PdfDictionary(Me, dictionary)
            ElseIf (reference = TryCast(obj, ParseObjectReference)) IsNot Nothing Then
                Return New PdfObjectReference(Me, reference)
            ElseIf (stream = TryCast(obj, ParseStream)) IsNot Nothing Then
                Return New PdfStream(Me, stream)
            ElseIf (array = TryCast(obj, ParseArray)) IsNot Nothing Then
                Return New PdfArray(Me, array)
            ElseIf (identifier = TryCast(obj, ParseIdentifier)) IsNot Nothing Then
                Return New PdfIdentifier(Me, identifier)
            ElseIf ([boolean] = TryCast(obj, ParseBoolean)) IsNot Nothing Then
                Return New PdfBoolean(Me, [boolean])
            End If

            Dim nul As New Value(Of ParseNull)
            If (nul = TryCast(obj, ParseNull)) IsNot Nothing Then Return New PdfNull(Me)
            Throw New ApplicationException($"Cannot wrap object '{obj.GetType().Name}' as a pdf object .")
        End Function

        Public Shared Function ArrayToRectangle(array As PdfArray) As PdfRectangle
            If array IsNot Nothing Then
                Return New PdfRectangle(array.Parent, array.ParseArray)
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace
