Namespace ManagedSqlite.Core.Tables
    Public Class Sqlite3SchemaRow
        Public Property Type() As String
            Get
                Return m_Type
            End Get
            Friend Set
                m_Type = Value
            End Set
        End Property
        Private m_Type As String

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Friend Set
                m_Name = Value
            End Set
        End Property
        Private m_Name As String

        Public Property TableName() As String
            Get
                Return m_TableName
            End Get
            Friend Set
                m_TableName = Value
            End Set
        End Property
        Private m_TableName As String

        Public Property RootPage() As UInteger
            Get
                Return m_RootPage
            End Get
            Friend Set
                m_RootPage = Value
            End Set
        End Property
        Private m_RootPage As UInteger

        Public Property Sql() As String
            Get
                Return m_Sql
            End Get
            Friend Set
                m_Sql = Value
            End Set
        End Property
        Private m_Sql As String
    End Class
End Namespace
