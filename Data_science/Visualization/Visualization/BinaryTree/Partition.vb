#Region "Microsoft.VisualBasic::e0b62ee226a6655a11e9b26edcefe8e3, Data_science\Visualization\Visualization\BinaryTree\Partition.vb"

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

    '   Total Lines: 47
    '    Code Lines: 38
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.58 KB


    '     Class Partition
    ' 
    '         Properties: members, NumOfEntity, PropertyMeans, Tag, uids
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DataMining.KMeans
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
        Public Property members As EntityClusterModel()

        Public ReadOnly Property PropertyMeans As Double()
            Get
                Dim pVector As New Dictionary(Of String, List(Of Double))

                For Each key As String In members.First.Properties.Keys
                    pVector(key) = New List(Of Double)
                Next

                For Each member As EntityClusterModel In members
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
