#Region "Microsoft.VisualBasic::597af3fae14c62b9de8a5e6992668221, mime\application%pdf\PdfReader\Parser\ParseDictionary.vb"

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

    '   Total Lines: 102
    '    Code Lines: 85
    ' Comment Lines: 0
    '   Blank Lines: 17
    '     File Size: 3.57 KB


    '     Class ParseDictionary
    ' 
    '         Properties: Count, Keys, Values
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ContainsName, GetEnumerator, MandatoryValue, OptionalValue
    ' 
    '         Sub: BuildDictionary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class ParseDictionary
        Inherits ParseObjectBase

        Private _names As List(Of String)
        Private _values As List(Of ParseObjectBase)
        Private _dictionary As Dictionary(Of String, ParseObjectBase)

        Public Sub New(names As List(Of String), values As List(Of ParseObjectBase))
            _names = names
            _values = values
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return If(_names IsNot Nothing, _names.Count, _dictionary.Count)
            End Get
        End Property

        Public Function ContainsName(name As String) As Boolean
            BuildDictionary()
            Return _dictionary.ContainsKey(name)
        End Function

        Public ReadOnly Property Keys As Dictionary(Of String, ParseObjectBase).KeyCollection
            Get
                BuildDictionary()
                Return _dictionary.Keys
            End Get
        End Property

        Public ReadOnly Property Values As Dictionary(Of String, ParseObjectBase).ValueCollection
            Get
                BuildDictionary()
                Return _dictionary.Values
            End Get
        End Property

        Public Function GetEnumerator() As Dictionary(Of String, ParseObjectBase).Enumerator
            BuildDictionary()
            Return _dictionary.GetEnumerator()
        End Function

        Default Public Property Item(name As String) As ParseObjectBase
            Get
                BuildDictionary()
                Return _dictionary(name)
            End Get
            Set(value As ParseObjectBase)
                BuildDictionary()
                _dictionary(name) = value
            End Set
        End Property

        Public Function OptionalValue(Of T As ParseObjectBase)(name As String) As T
            BuildDictionary()
            Dim entry As ParseObjectBase = Nothing

            If _dictionary.TryGetValue(name, entry) Then
                If TypeOf entry Is T Then
                    Return entry
                Else
                    Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                End If
            End If

            Return Nothing
        End Function

        Public Function MandatoryValue(Of T As ParseObjectBase)(name As String) As T
            BuildDictionary()
            Dim entry As ParseObjectBase = Nothing

            If _dictionary.TryGetValue(name, entry) Then
                If TypeOf entry Is T Then
                    Return entry
                Else
                    Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                End If
            Else
                Throw New ApplicationException($"Dictionary is missing mandatory name '{name}'.")
            End If
        End Function

        Private Sub BuildDictionary()
            If _dictionary Is Nothing Then
                _dictionary = New Dictionary(Of String, ParseObjectBase)()
                Dim count = Me.Count

                For i = 0 To count - 1
                    _dictionary.Add(_names(i), _values(i))
                Next

                _names = Nothing
                _values = Nothing
            End If
        End Sub
    End Class
End Namespace
