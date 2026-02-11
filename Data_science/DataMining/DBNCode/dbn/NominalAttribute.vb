#Region "Microsoft.VisualBasic::e4b1bc5d7e4b417407f1c95a9422bfbd, Data_science\DataMining\DBNCode\dbn\NominalAttribute.vb"

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

    '   Total Lines: 56
    '    Code Lines: 41 (73.21%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (26.79%)
    '     File Size: 1.71 KB


    '     Class NominalAttribute
    ' 
    '         Properties: Name, Nominal, Numeric
    ' 
    '         Function: [get], add, getIndex, size, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.utils

Namespace dbn

    Public Class NominalAttribute
        Implements Attribute

        Private nameField As String

        Private values As New BidirectionalArray(Of String)()

        Public Overridable ReadOnly Property Numeric As Boolean Implements Attribute.Numeric
            Get
                Return False
            End Get
        End Property

        Public Overridable ReadOnly Property Nominal As Boolean Implements Attribute.Nominal
            Get
                Return True
            End Get
        End Property

        Public Overridable Function size() As Integer Implements Attribute.size
            Return values.size()
        End Function

        Public Overridable Function add(value As String) As Boolean Implements Attribute.add
            Return values.add(value)
        End Function

        Public Overrides Function ToString() As String Implements Attribute.ToString
            Return "" & values.ToString()
        End Function

        Public Overridable Function getIndex(value As String) As Integer Implements Attribute.getIndex
            Return values.getIndex(value)
        End Function

        Public Overridable Function [get](index As Integer) As String Implements Attribute.get
            Return values.get(index)
        End Function

        Public Overridable Property Name As String Implements Attribute.Name
            Set(value As String)
                nameField = value
            End Set
            Get
                Return nameField
            End Get
        End Property


    End Class

End Namespace

