Imports System.Text
''' <summary>
''' the object model of a query
''' </summary>
Public Class Query

    ''' <summary>
    ''' the query name
    ''' </summary>
    ''' <returns></returns>
    Public Property name As String
    Public Property parser As Parser
    Public Property isArray As Boolean

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
        Dim sb As New StringBuilder

        Call sb.Append(name)

        If isArray Then
            Call sb.Append("[]")
        End If

        If Not parser Is Nothing Then
            Call sb.Append(": ")
            Call sb.Append(parser.ToString)
        End If

        If m_memberList.Count > 0 Then
            sb.Append(" ")
            sb.Append("{")
            sb.Append(members.JoinBy(", "))
            sb.Append("}")
        End If

        Return sb.ToString
    End Function

End Class

