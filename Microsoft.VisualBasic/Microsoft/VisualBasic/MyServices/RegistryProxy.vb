Imports Microsoft.Win32
Imports System
Imports System.ComponentModel
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.MyServices
    <EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class RegistryProxy
        ' Methods
        Friend Sub New()
        End Sub

        Public Function GetValue(keyName As String, valueName As String, defaultValue As Object) As Object
            Return Registry.GetValue(keyName, valueName, defaultValue)
        End Function

        Public Sub SetValue(keyName As String, valueName As String, value As Object)
            Registry.SetValue(keyName, valueName, value)
        End Sub

        Public Sub SetValue(keyName As String, valueName As String, value As Object, valueKind As RegistryValueKind)
            Registry.SetValue(keyName, valueName, value, valueKind)
        End Sub


        ' Properties
        Public ReadOnly Property ClassesRoot As RegistryKey
            Get
                Return Registry.ClassesRoot
            End Get
        End Property

        Public ReadOnly Property CurrentConfig As RegistryKey
            Get
                Return Registry.CurrentConfig
            End Get
        End Property

        Public ReadOnly Property CurrentUser As RegistryKey
            Get
                Return Registry.CurrentUser
            End Get
        End Property

        <Obsolete("The DynData registry key works only on Win9x, which is not supported by this version of the .NET Framework.  Use the PerformanceData registry key instead.  This property will be removed from a future version of the framework.")> _
        Public ReadOnly Property DynData As RegistryKey
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property LocalMachine As RegistryKey
            Get
                Return Registry.LocalMachine
            End Get
        End Property

        Public ReadOnly Property PerformanceData As RegistryKey
            Get
                Return Registry.PerformanceData
            End Get
        End Property

        Public ReadOnly Property Users As RegistryKey
            Get
                Return Registry.Users
            End Get
        End Property

    End Class
End Namespace

