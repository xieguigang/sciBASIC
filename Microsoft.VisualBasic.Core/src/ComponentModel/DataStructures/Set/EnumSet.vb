Namespace ComponentModel.DataStructures

    Public Class EnumSet(Of E As {Structure, IComparable, IComparable(Of E)})

        ReadOnly enums As List(Of E)

        Sub New(enums As E())
            Me.enums = enums.AsList
        End Sub

        Public Sub add(e As E)
            If enums.IndexOf(e) = -1 Then
                Call enums.Add(e)
            End If
        End Sub

        Public Shared Function [of](ParamArray vec As E()) As EnumSet(Of E)
            Return New EnumSet(Of E)(vec)
        End Function

        ''' <summary>
        ''' create a new empty enum set
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function noneOf() As EnumSet(Of E)
            Return New EnumSet(Of E)({})
        End Function

        Public Shared Function allOf() As EnumSet(Of E)
            Return New EnumSet(Of E)(EnumHelpers.Enums(Of E))
        End Function

    End Class
End Namespace