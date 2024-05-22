#Region "Microsoft.VisualBasic::5c06126fb2cc8024b9299a07964a3374, Microsoft.VisualBasic.Core\src\Serialization\BEncoding\BElements\BInteger.vb"

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

    '   Total Lines: 112
    '    Code Lines: 66 (58.93%)
    ' Comment Lines: 29 (25.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (15.18%)
    '     File Size: 3.57 KB


    '     Class BInteger
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CompareTo, Equals, GetHashCode, (+2 Overloads) ToBencodedString, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode integer.
    ''' </summary>
    Public Class BInteger : Implements BElement, IComparable(Of BInteger)

        ''' <summary>
        ''' The value of the bencoded integer.
        ''' </summary>
        Public Property Value As Long

        ''' <summary>
        ''' The main constructor.
        ''' </summary>
        ''' <param name="value">The value of the bencoded integer.</param>
        Public Sub New(value As Long)
            Me.Value = value
        End Sub

        Sub New(value As Integer)
            Me.Value = value
        End Sub

        Sub New(value As UInteger)
            Me.Value = value
        End Sub

        ''' <summary>
        ''' Generates the bencoded equivalent of the integer.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the integer.</returns>
        Public Function ToBencodedString() As String Implements BElement.ToBencodedString
            Return ToBencodedString(New StringBuilder()).ToString()
        End Function

        ''' <summary>
        ''' Generates the bencoded equivalent of the integer.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the integer.</returns>
        Public Function ToBencodedString(u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
            If u Is Nothing Then
                u = New StringBuilder("i"c)
            Else
                u.Append("i"c)
            End If

            Return u.Append(Value.ToString()).Append("e"c)
        End Function

        ''' <see cref="Object.GetHashCode()"/>
        Public Overrides Function GetHashCode() As Integer
            Return Value.GetHashCode()
        End Function

        ''' <summary>
        ''' Int32.Equals(object)
        ''' </summary>
        Public Overrides Function Equals(obj As Object) As Boolean
            Try
                Return Value.Equals(CType(obj, BInteger).Value)
            Catch
                Return False
            End Try
        End Function

        ''' <see cref="Object.ToString()"/>
        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        ''' <see cref="IComparable.CompareTo(Object)"/>
        Public Function CompareTo(other As BInteger) As Integer Implements IComparable(Of BInteger).CompareTo
            Return Value.CompareTo(other.Value)
        End Function

        ''' <summary>
        ''' Allows you to set an integer to a BInteger.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Widening Operator CType(n As Integer) As BInteger
            Return New BInteger(n)
        End Operator

        Public Shared Narrowing Operator CType(n As BInteger) As Integer
            If n Is Nothing Then
                Return 0
            Else
                Return n.Value
            End If
        End Operator

        Public Shared Narrowing Operator CType(n As BInteger) As UInteger
            If n Is Nothing Then
                Return 0
            Else
                Return n.Value
            End If
        End Operator

        Public Shared Narrowing Operator CType(n As BInteger) As Long
            If n Is Nothing Then
                Return 0
            Else
                Return n.Value
            End If
        End Operator
    End Class
End Namespace
