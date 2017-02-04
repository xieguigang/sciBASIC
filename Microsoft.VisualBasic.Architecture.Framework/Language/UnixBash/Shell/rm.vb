Namespace Language.UnixBash

    Partial Public Module FileSystemAPI

        Public ReadOnly Property rm As FileDelete
            Get
                Return New FileDelete
            End Get
        End Property

        Public ReadOnly Property rf As rmOption
        Public ReadOnly Property f As rmOption

    End Module

    ''' <summary>
    ''' ``rm -rf /*``
    ''' </summary>
    Public Class FileDelete

        Public Shared Operator -(rm As FileDelete, opt As rmOption) As FileDelete
            Return rm
        End Operator

        Public Shared Operator <=(rm As FileDelete, file$) As Long
            Return 0
        End Operator

        Public Shared Operator >=(rm As FileDelete, file$) As Long
            Throw New NotSupportedException
        End Operator
    End Class

    Public Structure rmOption

    End Structure
End Namespace