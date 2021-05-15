''' <summary>
''' the object model of a query
''' </summary>
Public Class Query

    ''' <summary>
    ''' the query name
    ''' </summary>
    ''' <returns></returns>
    Public Property name As String
    Public Property parser As Parse

    Dim m_memberList As New List(Of Query)

    Public Property members As Query()
        Get
            Return m_memberList.ToArray
        End Get
        Set(value As Query())
            m_memberList = New List(Of Query)(value)
        End Set
    End Property

    Public Sub Add(member As Query)
        m_memberList.Add(member)
    End Sub

    Public Overrides Function ToString() As String
        Return name
    End Function

End Class

