Namespace HDF5

    ''' <summary>
    ''' A internal pointer in a hdf5 binary data file.
    ''' </summary>
    Public MustInherit Class HDF5Ptr

        Protected m_address&

        Public ReadOnly Property address As Long
            Get
                Return m_address
            End Get
        End Property

        Sub New(address&)
            Me.m_address = address
        End Sub

        Public Overrides Function ToString() As String
            Return $"&{address} {Me.GetType.Name}"
        End Function
    End Class
End Namespace