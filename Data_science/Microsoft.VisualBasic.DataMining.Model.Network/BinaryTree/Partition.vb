Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class Partition : Implements sIdEnumerable

        Public Property Tag As String Implements sIdEnumerable.Identifier
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

                Return pVector.Keys.ToArray(Function(k) pVector(k).Average)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace