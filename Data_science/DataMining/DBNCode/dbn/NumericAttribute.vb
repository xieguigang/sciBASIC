#Region "Microsoft.VisualBasic::c16a593344dfa20b910a36ce91a0fdd4, Data_science\DataMining\DBNCode\dbn\NumericAttribute.vb"

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

    '   Total Lines: 86
    '    Code Lines: 67 (77.91%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (22.09%)
    '     File Size: 2.78 KB


    '     Class NumericAttribute
    ' 
    '         Properties: Name, Nominal, Numeric
    ' 
    '         Function: [get], add, Equals, GetHashCode, getIndex
    '                   size, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.utils

Namespace dbn

    Public Class NumericAttribute
        Implements Attribute

        Private nameField As String

        Private values As New BidirectionalArray(Of Single)()

        Public Overridable ReadOnly Property Numeric As Boolean Implements Attribute.Numeric
            Get
                Return True
            End Get
        End Property

        Public Overridable ReadOnly Property Nominal As Boolean Implements Attribute.Nominal
            Get
                Return False
            End Get
        End Property

        Public Overridable Function size() As Integer Implements Attribute.size
            Return values.size()
        End Function

        Public Overridable Function add(value As String) As Boolean Implements Attribute.add
            Return values.add(Single.Parse(value))
        End Function

        Public Overrides Function ToString() As String Implements Attribute.ToString
            Return "" & values.ToString()
        End Function

        Public Overridable Function getIndex(value As String) As Integer Implements Attribute.getIndex
            Return values.getIndex(Single.Parse(value))
        End Function

        Public Overridable Function [get](index As Integer) As String Implements Attribute.get
            Return Convert.ToString(values.get(index))
        End Function

        Public Overridable Property Name As String Implements Attribute.Name
            Set(value As String)
                nameField = value
            End Set
            Get
                Return nameField
            End Get
        End Property


        Public Overrides Function GetHashCode() As Integer
            Const prime = 31
            Dim result = 1
            result = prime * result + (If(ReferenceEquals(nameField, Nothing), 0, nameField.GetHashCode()))
            Return result
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Not (TypeOf obj Is NumericAttribute) Then
                Return False
            End If
            Dim other = CType(obj, NumericAttribute)
            If ReferenceEquals(nameField, Nothing) Then
                If Not ReferenceEquals(other.nameField, Nothing) Then
                    Return False
                End If
            ElseIf Not nameField.Equals(other.nameField) Then
                Return False
            End If
            Return True
        End Function



    End Class

End Namespace

