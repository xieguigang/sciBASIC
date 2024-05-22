#Region "Microsoft.VisualBasic::6bac3abad158dee19713f863974f75ae, Microsoft.VisualBasic.Core\src\ComponentModel\ValuePair\GenericAbstract.vb"

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
    '    Code Lines: 48 (47.06%)
    ' Comment Lines: 41 (40.20%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (12.75%)
    '     File Size: 3.62 KB


    '     Interface IKeyValuePairObject
    ' 
    '         Properties: Key, Value
    ' 
    '     Interface IReadOnlyDataSource
    ' 
    '         Properties: Key, Value
    ' 
    '     Class KeyValuePairObject
    ' 
    '         Properties: Key, Value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CreateObject, ToString
    ' 
    '     Class KeyValuePairData
    ' 
    '         Properties: DataObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' Defines a key/value pair that can be set or retrieved.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Interface IKeyValuePairObject(Of TKey, TValue)
        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Key As TKey
        ''' <summary>
        ''' Gets the value in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Value As TValue
    End Interface

    ''' <summary>
    ''' Defines a key/value pair that only can be retrieved.
    ''' </summary>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Interface IReadOnlyDataSource(Of TValue)
        ReadOnly Property Key As String
        ReadOnly Property Value As TValue
    End Interface

    ''' <summary>
    ''' Defines a key/value pair that can be set or retrieved.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Class KeyValuePairObject(Of TKey, TValue) : Implements IKeyValuePairObject(Of TKey, TValue)

        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Key As TKey Implements IKeyValuePairObject(Of TKey, TValue).Key
        ''' <summary>
        ''' Gets the value in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Value As TValue Implements IKeyValuePairObject(Of TKey, TValue).Value

        Sub New()
        End Sub

        Sub New(KEY As TKey, VALUE As TValue)
            Me.Key = KEY
            Me.Value = VALUE
        End Sub

        Sub New(raw As KeyValuePair(Of TKey, TValue))
            Key = raw.Key
            Value = raw.Value
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> {1}", Key.ToString, Value.ToString)
        End Function

        Public Shared Function CreateObject(key As TKey, value As TValue) As KeyValuePairObject(Of TKey, TValue)
            Return New KeyValuePairObject(Of TKey, TValue) With {.Key = key, .Value = value}
        End Function

        Public Shared Widening Operator CType(args As Object()) As KeyValuePairObject(Of TKey, TValue)
            If args.IsNullOrEmpty Then
                Return New KeyValuePairObject(Of TKey, TValue)
            End If
            If args.Length = 1 Then
                Return New KeyValuePairObject(Of TKey, TValue) With {
                    .Key = DirectCast(args(Scan0), TKey)
                }
            End If

            Return New KeyValuePairObject(Of TKey, TValue) With {
                .Key = DirectCast(args(Scan0), TKey),
                .Value = DirectCast(args(1), TValue)
            }
        End Operator
    End Class

    Public Class KeyValuePairData(Of T) : Inherits KeyValuePairObject(Of String, String)
        Implements IKeyValuePairObject(Of String, String)

        Public Property DataObject As T
    End Class
End Namespace
