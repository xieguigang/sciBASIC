Imports System.Runtime.CompilerServices

Namespace Language.Python

    Public Class Array(Of T) : Inherits List(Of T)

        Public Overloads Property Item(index%) As T
            Get
                If index < 0 Then
                    Return MyBase.Item(Count + index)
                Else
                    Return MyBase.Item(index)
                End If
            End Get
            Set(value As T)
                If index < 0 Then
                    MyBase.Item(Count + index) = value
                Else
                    MyBase.Item(index) = value
                End If
            End Set
        End Property

        Sub New()
            MyBase.New
        End Sub

        Sub New(source As IEnumerable(Of T))
            MyBase.New(source)
        End Sub

    End Class
End Namespace