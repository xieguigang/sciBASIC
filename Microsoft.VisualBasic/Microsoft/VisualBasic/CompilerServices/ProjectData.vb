Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.ConstrainedExecution
Imports System.Security
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute> _
    Public NotInheritable Class ProjectData
        ' Methods
        Private Sub New()
        End Sub

        <SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DynamicallyInvokableAttribute> _
        Public Shared Sub ClearProjectError()
            RuntimeHelpers.PrepareConstrainedRegions
            Try 
            Finally
                Information.Err.Clear
            End Try
        End Sub

        Public Shared Function CreateProjectError(hr As Integer) As Exception
            Dim obj1 As ErrObject = Information.Err
            obj1.Clear()
            Dim num As Integer = obj1.MapErrorNumber(hr)
            Return obj1.CreateException(hr, Utils.GetResourceString(DirectCast(num, vbErrors)))
        End Function

        <MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.SelfAffectingProcessMgmt), SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
        Public Shared Sub EndApp()
            FileSystem.CloseAllFiles(Assembly.GetCallingAssembly)
            Environment.Exit(0)
        End Sub

        Friend Function GetAssemblyData(assem As Assembly) As AssemblyData
            If ((assem Is Utils.VBRuntimeAssembly) OrElse (assem Is Me.m_CachedMSCoreLibAssembly)) Then
                Throw New SecurityException(Utils.GetResourceString("Security_LateBoundCallsNotPermitted"))
            End If
            Dim data As AssemblyData = DirectCast(Me.m_AssemblyData.Item(assem), AssemblyData)
            If (data Is Nothing) Then
                data = New AssemblyData
                Me.m_AssemblyData.Item(assem) = data
            End If
            Return data
        End Function

        Friend Shared Function GetProjectData() As ProjectData
            Dim oProject As ProjectData = ProjectData.m_oProject
            If (oProject Is Nothing) Then
                oProject = New ProjectData
                ProjectData.m_oProject = oProject
            End If
            Return oProject
        End Function

        <SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DynamicallyInvokableAttribute>
        Public Shared Sub SetProjectError(ex As Exception)
            RuntimeHelpers.PrepareConstrainedRegions()

            Try
            Finally
                Information.Err.CaptureException(ex)
            End Try
        End Sub

        <SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DynamicallyInvokableAttribute>
        Public Shared Sub SetProjectError(ex As Exception, lErl As Integer)
            RuntimeHelpers.PrepareConstrainedRegions()

            Try
            Finally
                Information.Err.CaptureException(ex, lErl)
            End Try
        End Sub


        ' Fields
        Friend m_AssemblyData As Hashtable = New Hashtable
        Private m_CachedMSCoreLibAssembly As Assembly = GetType(Integer).Assembly
        Friend m_DigitArray As Byte() = New Byte(30  - 1) {}
        Friend m_Err As ErrObject
        Friend m_numprsPtr As Byte() = New Byte(&H18  - 1) {}
        <ThreadStatic> _
        Private Shared m_oProject As ProjectData
        Friend m_rndSeed As Integer = &H50000
    End Class
End Namespace

