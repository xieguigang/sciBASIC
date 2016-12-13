Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FuzzyCMeans

    Public Class Entity : Inherits KMeans.Entity

        ''' <summary>
        ''' ``Key``键名和数组的下标一样是从0开始的
        ''' </summary>
        ''' <returns></returns>
        Public Property Memberships As Dictionary(Of Integer, Double)

        ''' <summary>
        ''' Max probably of <see cref="Memberships"/> its key value.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProbablyMembership As Integer
            Get
                Return Memberships _
                    .Keys _
                    .Select(Function(i) Memberships(i)) _
                    .MaxIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{uid} --> {Memberships.GetJson}"
        End Function
    End Class
End Namespace