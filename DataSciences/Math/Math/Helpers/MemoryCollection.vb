#Region "Microsoft.VisualBasic::ae378f455623b62f84049fbc4cbe63c8, ..\visualbasic_App\Scripting\Math\Math\Helpers\MemoryCollection.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Public MustInherit Class MemoryCollection(Of T) : Implements IEnumerable(Of KeyValuePair(Of String, T))

    Protected ReadOnly _objHash As Dictionary(Of String, T) = New Dictionary(Of String, T)
    Protected ReadOnly __engine As Expression

    Dim __caches As String()

    Public Sub New(engine As Expression)
        __engine = engine
    End Sub

    Public ReadOnly Property Objects As String()
        Get
            Return __caches
        End Get
    End Property

    Public ReadOnly Property DictData As Dictionary(Of String, T)
        Get
            Return _objHash
        End Get
    End Property

    Protected Sub __buildCache()
        __caches = (From strName As String
                    In _objHash.Keys
                    Select strName
                    Order By Len(strName) Descending).ToArray
    End Sub

    ''' <summary>
    ''' 名称的大小写不敏感
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Protected Function Add(Name As String, value As T, cache As Boolean, sensitive As Boolean) As Integer
        If Not sensitive Then
            Name = Name.ToLower
        End If

        Name = Name.Trim

        If _objHash.ContainsKey(Name) Then
            Call _objHash.Remove(Name)
        End If

        Call _objHash.Add(Name, value)
        If cache Then
            Call __buildCache()
        End If

        Return 0
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, T)) Implements IEnumerable(Of KeyValuePair(Of String, T)).GetEnumerator
        For Each item In _objHash
            Yield item
        Next
    End Function

    Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
