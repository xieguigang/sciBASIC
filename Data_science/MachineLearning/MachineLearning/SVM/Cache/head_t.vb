Namespace SVM

    Friend NotInheritable Class head_t

        Dim m_enclosingInstance As Cache

        Public ReadOnly Property EnclosingInstance As Cache
            Get
                Return m_enclosingInstance
            End Get
        End Property

        ''' <summary>
        ''' a cicular list
        ''' </summary>
        Friend prev, [next] As head_t
        ''' <summary>
        ''' data[0,len) is cached in this entry
        ''' </summary>
        Friend data As Single()
        Friend len As Integer

        Public Sub New(enclosingInstance As Cache)
            m_enclosingInstance = enclosingInstance
        End Sub
    End Class
End Namespace