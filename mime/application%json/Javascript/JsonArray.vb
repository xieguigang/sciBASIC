#Region "Microsoft.VisualBasic::5ad3f7cc15c34137f27bfa9c32352115, sciBASIC#\mime\application%json\Javascript\JsonArray.vb"

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

    '   Total Lines: 75
    '    Code Lines: 54
    ' Comment Lines: 5
    '   Blank Lines: 16
    '     File Size: 2.32 KB


    '     Class JsonArray
    ' 
    '         Properties: Length
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: ContainsElement, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Add, Insert, Remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace Javascript

    Public Class JsonArray : Inherits JsonModel
        Implements IEnumerable(Of JsonElement)

        Friend ReadOnly list As New List(Of JsonElement)

        Public ReadOnly Property Length As Integer
            Get
                Return list.Count
            End Get
        End Property

        Public Sub New()
        End Sub

        Sub New(objs As IEnumerable(Of JsonElement))
            list = objs.SafeQuery.ToList
        End Sub

        Sub New(values As IEnumerable(Of String))
            Call Me.New(values.Select(Function(str) New JsonValue(str)))
        End Sub

        Sub New(values As IEnumerable(Of Double))
            Call Me.New(values.Select(Function(d) New JsonValue(d)))
        End Sub

        Public Sub Add(element As JsonElement)
            Call list.Add(element)
        End Sub

        Public Sub Insert(index As Integer, element As JsonElement)
            Call list.Insert(index, element)
        End Sub

        ''' <summary>
        ''' Gets/Set elements by index
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index As Integer) As JsonElement
            Get
                Return list(index)
            End Get
            Set(value As JsonElement)
                list(index) = value
            End Set
        End Property

        Public Sub Remove(index As Integer)
            list.RemoveAt(index)
        End Sub

        Public Function ContainsElement(element As JsonElement) As Boolean
            Return list.Contains(element)
        End Function

        Public Overrides Function ToString() As String
            Return "JsonArray: {count: " & list.Count & "}"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of JsonElement) Implements IEnumerable(Of JsonElement).GetEnumerator
            For Each x As JsonElement In list
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
