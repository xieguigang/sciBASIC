#Region "Microsoft.VisualBasic::3ec3ee5c3808a0b04fb701e1c4572a38, mime\text%html\Document\Elements\ValueAttribute.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 5 (9.26%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (20.37%)
    '     File Size: 1.80 KB


    '     Structure ValueAttribute
    ' 
    '         Properties: IsEmpty, Name, Value, Values
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Equals, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Namespace Document

    ''' <summary>
    ''' A key-value pair of the html attribute, includes the attribute name and attribute value
    ''' </summary>
    Public Structure ValueAttribute : Implements INamedValue, IsEmpty

        Public Property Name As String Implements INamedValue.Key
        ''' <summary>
        ''' A collection of the attribute values, apply for the multiple value attribute data, example as class attribute
        ''' </summary>
        ''' <returns></returns>
        Public Property Values As List(Of String)

        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            Get
                Return Name.StringEmpty AndAlso Values.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' get the first attribute value string
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As String
            Get
                Return Values?.FirstOrDefault
            End Get
        End Property

        Sub New(strText As String)
            Dim ep As Integer = InStr(strText, "=")
            Name = Mid(strText, 1, ep - 1)
            Dim Value = Mid(strText, ep + 1)
            If Value.First = """"c AndAlso Value.Last = """"c Then
                Value = Mid(Value, 2, Len(Value) - 2)
            End If

            Values = New List(Of String) From {Value}
        End Sub

        Sub New(name As String, value As String)
            Me.Name = name
            Me.Values = New List(Of String) From {value}
        End Sub

        ''' <summary>
        ''' equals to any value
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Overloads Function Equals(value As String) As Boolean
            Return Values.SafeQuery.Any(Function(str) str = value)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Name}={Values.Select(Function(v) $"""{v}""").JoinBy(", ")}"
        End Function

        ''' <summary>
        ''' get attribute value
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(attr As ValueAttribute) As String
            Return attr.Value
        End Operator
    End Structure

End Namespace
