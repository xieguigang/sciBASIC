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

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace