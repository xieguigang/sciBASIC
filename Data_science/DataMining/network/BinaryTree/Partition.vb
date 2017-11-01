#Region "Microsoft.VisualBasic::2ca29f18bb9a318eb72022c027457fb5, ..\sciBASIC#\Data_science\DataMining\network\BinaryTree\Partition.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class Partition : Implements INamedValue

        Public Property Tag As String Implements INamedValue.Key
        Public ReadOnly Property NumOfEntity As Integer
            Get
                If uids Is Nothing Then
                    Return 0
                Else
                    Return uids.Length
                End If
            End Get
        End Property

        Public Property uids As String()
        Public Property members As EntityLDM()

        Public ReadOnly Property PropertyMeans As Double()
            Get
                Dim pVector As New Dictionary(Of String, List(Of Double))

                For Each key As String In members.First.Properties.Keys
                    pVector(key) = New List(Of Double)
                Next

                For Each member As EntityLDM In members
                    For Each key As String In pVector.Keys.ToArray
                        pVector(key) += member.Properties(key)
                    Next
                Next

                Return pVector.Keys.Select(Function(k) pVector(k).Average)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
