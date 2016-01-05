Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=False), EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public NotInheritable Class MyGroupCollectionAttribute
        Inherits Attribute
        ' Methods
        Public Sub New(typeToCollect As String, createInstanceMethodName As String, disposeInstanceMethodName As String, defaultInstanceAlias As String)
            Me.m_NameOfBaseTypeToCollect = typeToCollect
            Me.m_NameOfCreateMethod = createInstanceMethodName
            Me.m_NameOfDisposeMethod = disposeInstanceMethodName
            Me.m_DefaultInstanceAlias = defaultInstanceAlias
        End Sub


        ' Properties
        Public ReadOnly Property CreateMethod As String
            Get
                Return Me.m_NameOfCreateMethod
            End Get
        End Property

        Public ReadOnly Property DefaultInstanceAlias As String
            Get
                Return Me.m_DefaultInstanceAlias
            End Get
        End Property

        Public ReadOnly Property DisposeMethod As String
            Get
                Return Me.m_NameOfDisposeMethod
            End Get
        End Property

        Public ReadOnly Property MyGroupName As String
            Get
                Return Me.m_NameOfBaseTypeToCollect
            End Get
        End Property


        ' Fields
        Private m_DefaultInstanceAlias As String
        Private m_NameOfBaseTypeToCollect As String
        Private m_NameOfCreateMethod As String
        Private m_NameOfDisposeMethod As String
    End Class
End Namespace

