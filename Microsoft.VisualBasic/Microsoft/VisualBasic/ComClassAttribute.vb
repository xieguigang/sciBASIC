Imports System

Namespace Microsoft.VisualBasic
    <AttributeUsage(AttributeTargets.Class, Inherited:=False, AllowMultiple:=False)> _
    Public NotInheritable Class ComClassAttribute
        Inherits Attribute
        ' Methods
        Public Sub New()
            Me.m_InterfaceShadows = False
        End Sub

        Public Sub New(_ClassID As String)
            Me.m_InterfaceShadows = False
            Me.m_ClassID = _ClassID
        End Sub

        Public Sub New(_ClassID As String, _InterfaceID As String)
            Me.m_InterfaceShadows = False
            Me.m_ClassID = _ClassID
            Me.m_InterfaceID = _InterfaceID
        End Sub

        Public Sub New(_ClassID As String, _InterfaceID As String, _EventId As String)
            Me.m_InterfaceShadows = False
            Me.m_ClassID = _ClassID
            Me.m_InterfaceID = _InterfaceID
            Me.m_EventID = _EventId
        End Sub


        ' Properties
        Public ReadOnly Property ClassID As String
            Get
                Return Me.m_ClassID
            End Get
        End Property

        Public ReadOnly Property EventID As String
            Get
                Return Me.m_EventID
            End Get
        End Property

        Public ReadOnly Property InterfaceID As String
            Get
                Return Me.m_InterfaceID
            End Get
        End Property

        Public Property InterfaceShadows As Boolean
            Get
                Return Me.m_InterfaceShadows
            End Get
            Set(Value As Boolean)
                Me.m_InterfaceShadows = Value
            End Set
        End Property


        ' Fields
        Private m_ClassID As String
        Private m_EventID As String
        Private m_InterfaceID As String
        Private m_InterfaceShadows As Boolean
    End Class
End Namespace

