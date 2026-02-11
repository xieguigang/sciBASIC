#Region "Microsoft.VisualBasic::83d211382804daee7536b91fd788827b, Data_science\DataMining\DBNCode\dbn\Configuration.vb"

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

    '   Total Lines: 90
    '    Code Lines: 70 (77.78%)
    ' Comment Lines: 3 (3.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (18.89%)
    '     File Size: 3.08 KB


    '     Class Configuration
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: Equals, GetHashCode, toArray, ToString
    ' 
    '         Sub: reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Language.Java

Namespace dbn

    Public Class Configuration

        Protected Friend attributes As IList(Of Attribute)

        Protected Friend configuration As Integer()

        Protected Friend childNode As Integer

        ' markovLag = configuration.length/n -1 
        Protected Friend markovLag As Integer

        Public Sub New(c As Configuration)
            attributes = c.attributes
            configuration = CType(c.configuration.Clone(), Integer())
            markovLag = configuration.Length / attributes.Count - 1
            childNode = c.childNode
        End Sub

        Protected Friend Sub New(attributes As IList(Of Attribute), markovLag As Integer)
            Me.attributes = attributes
            Me.markovLag = markovLag
            configuration = New Integer((markovLag + 1) * attributes.Count - 1) {}
        End Sub

        Protected Friend Sub New(attributes As IList(Of Attribute), configuration As Integer(), markovLag As Integer, childNode As Integer)
            Me.attributes = attributes
            Me.configuration = configuration
            Me.markovLag = markovLag
            Me.childNode = childNode
        End Sub

        Protected Friend Overridable Sub reset()
            configuration.fill(-1)
        End Sub

        Public Overridable Function toArray() As Integer()
            Return configuration
        End Function

        Public Overrides Function ToString() As String
            ' return Arrays.toString(configuration);
            Dim sb As StringBuilder = New StringBuilder()

            sb.Append("[")
            Dim n = attributes.Count
            For i = 0 To configuration.Length - 1
                If configuration(i) <> -1 AndAlso i <> n * markovLag + childNode Then
                    Dim lag As Integer = i / n
                    Dim id = i Mod n
                    sb.Append(attributes(id).Name & "[" & lag.ToString() & "]=" & attributes(id).get(configuration(i)))
                    sb.Append(", ")
                End If
            Next
            ' Readable version
            If sb.Length > 2 Then
                sb.Length = sb.Length - 2
            End If
            sb.Append("]")
            Return sb.ToString()
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return hashCode(configuration)
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Not (TypeOf obj Is Configuration) Then
                Return False
            End If
            Dim other = CType(obj, Configuration)
            If Not configuration.SequenceEqual(other.configuration) Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace

