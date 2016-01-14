Namespace Logging

    Public Interface ISupportLoggingClient
        Inherits System.IDisposable

#Region "Public Property"

        ReadOnly Property Logging As Logging.LogFile

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Save the log file into the filesystem.
        ''' </summary>
        ''' <returns></returns>
        Function WriteLog() As Boolean
#End Region

    End Interface
End Namespace