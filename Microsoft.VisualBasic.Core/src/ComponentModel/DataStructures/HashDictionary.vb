#Region "Microsoft.VisualBasic::abb1f44460101a4c04d6f44e10d206a9, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\HashDictionary.vb"

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

    '   Total Lines: 236
    '    Code Lines: 164 (69.49%)
    ' Comment Lines: 38 (16.10%)
    '    - Xml Docs: 60.53%
    ' 
    '   Blank Lines: 34 (14.41%)
    '     File Size: 9.36 KB


    '     Class HashDictionary
    ' 
    '         Properties: __keys, __values, (+2 Overloads) Count, IsReadOnly, Keys
    '                     Values
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Contains, (+2 Overloads) ContainsKey, GetEnumerator, GetEnumerator1, (+2 Overloads) Remove
    '                   (+2 Overloads) TryGetValue
    ' 
    '         Sub: (+2 Overloads) Add, Clear, CopyTo, (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.ObjectModel
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' The key of the dictionary is string value and the keys is not sensitive to the character case.
    ''' (字典的键名为字符串，大小写不敏感，行为和哈希表类型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Class HashDictionary(Of T) : Implements IDisposable
        Implements IDictionary(Of String, T)
        Implements IReadOnlyDictionary(Of String, T)

        Protected ReadOnly _hashBuffer As Dictionary(Of String, T)
        Protected ReadOnly _keysHash As Dictionary(Of String, String)

        Sub New(data As IDictionary(Of String, T))
            Call Me.New

            If Not data Is Nothing Then
                For Each ItemObject In data
                    Call Add(ItemObject)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        ''' that is empty, has the default initial capacity, and uses the default equality
        ''' comparer for the key type.
        ''' </summary>
        Sub New()
            _hashBuffer = New Dictionary(Of String, T)
            _keysHash = New Dictionary(Of String, String)
        End Sub

#Region "Implements System.IDisposable, Generic.IDictionary(Of String, T)"

        Public Sub Add(item As KeyValuePair(Of String, T)) Implements ICollection(Of KeyValuePair(Of String, T)).Add
            Dim Key As String = item.Key
            Me(Key) = item.Value
        End Sub

        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, T)).Clear
            Call _keysHash.Clear()
            Call _hashBuffer.Clear()
        End Sub

        Public Function Contains(item As KeyValuePair(Of String, T)) As Boolean Implements ICollection(Of KeyValuePair(Of String, T)).Contains
            Dim value = Me(item.Key)
            If item.Value Is Nothing AndAlso value Is Nothing Then
                Return True
            ElseIf value Is Nothing Then
                Return False
            ElseIf Object.Equals(item.Value, value) Then
                Return True
            End If

            Return False
        End Function

#If NET_40 = 1 Then
        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, T)).Count
#Else
           Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, T)).Count, IReadOnlyCollection(Of KeyValuePair(Of String, T)).Count
#End If
            Get
                Return _keysHash.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, T)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As KeyValuePair(Of String, T)) As Boolean Implements ICollection(Of KeyValuePair(Of String, T)).Remove
            Dim Key As String = item.Key.ToLower
            Call _hashBuffer.Remove(Key)
            Return _keysHash.Remove(Key)
        End Function

        Public Sub Add(key As String, value As T) Implements IDictionary(Of String, T).Add
            Call Add(New KeyValuePair(Of String, T)(key, value))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="key">大小写不敏感</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
#If NET_40 = 1 Then
        Public Function ContainsKey(key As String) As Boolean Implements IDictionary(Of String, T).ContainsKey
#Else
        Public Function ContainsKey(key As String) As Boolean Implements IDictionary(Of String, T).ContainsKey, IReadOnlyDictionary(Of String, T).ContainsKey
#End If
            Return _keysHash.ContainsKey(key.ToLower)
        End Function

        ''' <summary>
        ''' 添加<see cref="Add"></see>和替换操作主要在这里进行
        ''' </summary>
        ''' <param name="key">大小写不敏感</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
#If NET_40 = 1 Then
        Default Public Property Item(key As String) As T Implements IDictionary(Of String, T).Item
#Else
           Default Public Property Item(key As String) As T Implements IDictionary(Of String, T).Item, IReadOnlyDictionary(Of String, T).Item
#End If

            Get
                key = key.ToLower
                If _keysHash.ContainsKey(key) Then
                    Return _hashBuffer(key)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As T)
                Dim query As String = key.ToLower

                If _keysHash.ContainsKey(query) Then
                    _hashBuffer(query) = value
                Else
                    Call _hashBuffer.Add(query, value)
                    Call _keysHash.Add(query, key)
                End If
            End Set
        End Property

        Public Sub CopyTo(array() As KeyValuePair(Of String, T), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, T)).CopyTo
            Dim LQuery = (From item In _keysHash Select New KeyValuePair(Of String, T)(item.Value, _hashBuffer(item.Key))).ToArray
            Call System.Array.ConstrainedCopy(LQuery, 0, array, arrayIndex, array.Length - arrayIndex)
        End Sub

        Public ReadOnly Property Keys As ICollection(Of String) Implements IDictionary(Of String, T).Keys
            Get
                Return _keysHash.Values
            End Get
        End Property

#If NET_40 = 0 Then
        Private ReadOnly Property __keys As IEnumerable(Of String) Implements IReadOnlyDictionary(Of String, T).Keys
            Get
                Return Keys
            End Get
        End Property
#End If

        Public Function Remove(key As String) As Boolean Implements IDictionary(Of String, T).Remove
            key = key.ToLower
            If Me._keysHash.ContainsKey(key) Then
                Call _keysHash.Remove(key)
                Call _hashBuffer.Remove(key)

                Return True
            Else
                Return False
            End If
        End Function

#If NET_40 = 1 Then
        Public Function TryGetValue(key As String, ByRef value As T) As Boolean Implements IDictionary(Of String, T).TryGetValue
#Else
        Public Function TryGetValue(key As String, ByRef value As T) As Boolean Implements IDictionary(Of String, T).TryGetValue, IReadOnlyDictionary(Of String, T).TryGetValue
#End If

            value = Me(key)
            Return _keysHash.ContainsKey(key.ToLower)
        End Function

        Public ReadOnly Property Values As ICollection(Of T) Implements IDictionary(Of String, T).Values
            Get
                Return _hashBuffer.Values
            End Get
        End Property

#If NET_40 = 0 Then
        Private ReadOnly Property __values As IEnumerable(Of T) Implements IReadOnlyDictionary(Of String, T).Values
            Get
                Return Values
            End Get
        End Property
#End If
        Public Iterator Function GetEnumerator1() As IEnumerator(Of KeyValuePair(Of String, T)) Implements IEnumerable(Of KeyValuePair(Of String, T)).GetEnumerator
            For Each ItemObject In Me._hashBuffer
                Yield New KeyValuePair(Of String, T)(Me._keysHash(ItemObject.Key), ItemObject.Value)
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator1()
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Me.Clear()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
#End Region
    End Class
End Namespace
