Imports Microsoft.Win32.SafeHandles
Imports System
Imports System.ComponentModel
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes
Imports System.Security

Namespace Microsoft.VisualBasic.CompilerServices
    <ComVisible(False), SuppressUnmanagedCodeSecurity> _
    Friend NotInheritable Class UnsafeNativeMethods
        ' Methods
        <SecurityCritical> _
        Private Sub New()
        End Sub

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function CreateFileMapping(hFile As HandleRef, <MarshalAs(UnmanagedType.LPStruct)> lpAttributes As SECURITY_ATTRIBUTES, flProtect As Integer, dwMaxSizeHi As Integer, dwMaxSizeLow As Integer, lpName As String) As SafeFileHandle
        End Function

        <SecurityCritical, DllImport("Kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function GetDiskFreeSpaceEx(Directory As String, ByRef UserSpaceFree As Long, ByRef TotalUserSpace As Long, ByRef TotalFreeSpace As Long) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <SecurityCritical, DllImport("User32.dll", CharSet:=CharSet.Auto, ExactSpelling:=True)>
        Friend Shared Function GetKeyState(KeyCode As Integer) As Short
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Unicode)>
        Friend Shared Function GetLogicalDrives() As Integer
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function LCMapString(Locale As Integer, dwMapFlags As Integer, <MarshalAs(UnmanagedType.VBByRefStr)> ByRef lpSrcStr As String, cchSrc As Integer, <MarshalAs(UnmanagedType.VBByRefStr)> ByRef lpDestStr As String, cchDest As Integer) As Integer
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function LCMapStringA(Locale As Integer, dwMapFlags As Integer, <MarshalAs(UnmanagedType.LPArray)> lpSrcStr As Byte(), cchSrc As Integer, <MarshalAs(UnmanagedType.LPArray)> lpDestStr As Byte(), cchDest As Integer) As Integer
        End Function

        <SecurityCritical, DllImport("kernel32", SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function LocalFree(LocalHandle As IntPtr) As IntPtr
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function MapViewOfFile(hFileMapping As IntPtr, dwDesiredAccess As Integer, dwFileOffsetHigh As Integer, dwFileOffsetLow As Integer, dwNumberOfBytesToMap As UIntPtr) As SafeMemoryMappedViewOfFileHandle
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Unicode)>
        Friend Shared Function MessageBeep(uType As Integer) As Integer
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function MoveFile(<[In], MarshalAs(UnmanagedType.LPTStr)> lpExistingFileName As String, <[In], MarshalAs(UnmanagedType.LPTStr)> lpNewFileName As String) As Integer
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function OpenFileMapping(dwDesiredAccess As Integer, <MarshalAs(UnmanagedType.Bool)> bInheritHandle As Boolean, lpName As String) As SafeFileHandle
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Unicode, SetLastError:=True)>
        Friend Shared Function SetLocalTime(systime As SystemTime) As Integer
        End Function

        <SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function UnmapViewOfFile(pvBaseAddress As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <SecurityCritical, DllImport("oleaut32", CharSet:=CharSet.Unicode, PreserveSig:=False)>
        Friend Shared Sub VariantChangeType(<Out> ByRef dest As Object, <[In]> ByRef Src As Object, wFlags As Short, vt As Short)
        End Sub

        <SecurityCritical, DllImport("oleaut32", CharSet:=CharSet.Unicode, PreserveSig:=False)>
        Friend Shared Function VarNumFromParseNum(<MarshalAs(UnmanagedType.LPArray)> numprsPtr As Byte(), <MarshalAs(UnmanagedType.LPArray)> DigitArray As Byte(), dwVtBits As Integer) As Object
        End Function

        <SecurityCritical, DllImport("oleaut32", CharSet:=CharSet.Unicode)>
        Friend Shared Function VarParseNumFromStr(<[In], MarshalAs(UnmanagedType.LPWStr)> str As String, lcid As Integer, dwFlags As Integer, <MarshalAs(UnmanagedType.LPArray)> numprsPtr As Byte(), <MarshalAs(UnmanagedType.LPArray)> digits As Byte()) As Integer
        End Function


        ' Fields
        Public Const LCID_US_ENGLISH As Integer = &H409
        Public Const MEMBERID_NIL As Integer = 0

        ' Nested Types
        <ComImport, EditorBrowsable(EditorBrowsableState.Never), Guid("00020400-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Public Interface IDispatch
            <PreserveSig, Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Function GetTypeInfoCount() As Integer
            <PreserveSig, SecurityCritical>
            Function GetTypeInfo(<[In]> index As Integer, <[In]> lcid As Integer, <Out, MarshalAs(UnmanagedType.Interface)> ByRef pTypeInfo As ITypeInfo) As Integer
            <PreserveSig, SecurityCritical>
            Function GetIDsOfNames() As Integer
            <PreserveSig, SecurityCritical>
            Function Invoke() As Integer
        End Interface

        <ComImport, EditorBrowsable(EditorBrowsableState.Never), Guid("B196B283-BAB4-101A-B69C-00AA00341D07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Public Interface IProvideClassInfo
            <SecurityCritical>
            Function GetClassInfo() As <MarshalAs(UnmanagedType.Interface)> ITypeInfo
        End Interface

        <ComImport, EditorBrowsable(EditorBrowsableState.Never), Guid("00020403-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Public Interface ITypeComp
            <Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Sub RemoteBind(<[In], MarshalAs(UnmanagedType.LPWStr)> szName As String, <[In], MarshalAs(UnmanagedType.U4)> lHashVal As Integer, <[In], MarshalAs(UnmanagedType.U2)> wFlags As Short, <Out, MarshalAs(UnmanagedType.LPArray)> ppTInfo As ITypeInfo(), <Out, MarshalAs(UnmanagedType.LPArray)> pDescKind As DESCKIND(), <Out, MarshalAs(UnmanagedType.LPArray)> ppFuncDesc As FUNCDESC(), <Out, MarshalAs(UnmanagedType.LPArray)> ppVarDesc As VARDESC(), <Out, MarshalAs(UnmanagedType.LPArray)> ppTypeComp As ITypeComp(), <Out, MarshalAs(UnmanagedType.LPArray)> pDummy As Integer())
            <SecurityCritical>
            Sub RemoteBindType(<[In], MarshalAs(UnmanagedType.LPWStr)> szName As String, <[In], MarshalAs(UnmanagedType.U4)> lHashVal As Integer, <Out, MarshalAs(UnmanagedType.LPArray)> ppTInfo As ITypeInfo())
        End Interface

        <ComImport, EditorBrowsable(EditorBrowsableState.Never), Guid("00020401-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Public Interface ITypeInfo
            <PreserveSig, SecurityCritical>
            Function GetTypeAttr(<Out> ByRef pTypeAttr As IntPtr) As Integer
            <PreserveSig, SecurityCritical>
            Function GetTypeComp(<Out> ByRef pTComp As ITypeComp) As Integer
            <PreserveSig, SecurityCritical>
            Function GetFuncDesc(<[In], MarshalAs(UnmanagedType.U4)> index As Integer, <Out> ByRef pFuncDesc As IntPtr) As Integer
            <PreserveSig, SecurityCritical>
            Function GetVarDesc(<[In], MarshalAs(UnmanagedType.U4)> index As Integer, <Out> ByRef pVarDesc As IntPtr) As Integer
            <PreserveSig, SecurityCritical>
            Function GetNames(<[In]> memid As Integer, <Out, MarshalAs(UnmanagedType.LPArray)> rgBstrNames As String(), <[In], MarshalAs(UnmanagedType.U4)> cMaxNames As Integer, <Out, MarshalAs(UnmanagedType.U4)> ByRef cNames As Integer) As Integer
            <PreserveSig, Obsolete("Bad signature, second param type should be Byref. Fix and verify signature before use.", True), SecurityCritical>
            Function GetRefTypeOfImplType(<[In], MarshalAs(UnmanagedType.U4)> index As Integer, <Out> ByRef pRefType As Integer) As Integer
            <PreserveSig, Obsolete("Bad signature, second param type should be Byref. Fix and verify signature before use.", True), SecurityCritical>
            Function GetImplTypeFlags(<[In], MarshalAs(UnmanagedType.U4)> index As Integer, <Out> pImplTypeFlags As Integer) As Integer
            <PreserveSig, SecurityCritical>
            Function GetIDsOfNames(<[In]> rgszNames As IntPtr, <[In], MarshalAs(UnmanagedType.U4)> cNames As Integer, <Out> ByRef pMemId As IntPtr) As Integer
            <PreserveSig, Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Function Invoke() As Integer
            <PreserveSig, SecurityCritical>
            Function GetDocumentation(<[In]> memid As Integer, <Out, MarshalAs(UnmanagedType.BStr)> ByRef pBstrName As String, <Out, MarshalAs(UnmanagedType.BStr)> ByRef pBstrDocString As String, <Out, MarshalAs(UnmanagedType.U4)> ByRef pdwHelpContext As Integer, <Out, MarshalAs(UnmanagedType.BStr)> ByRef pBstrHelpFile As String) As Integer
            <PreserveSig, Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Function GetDllEntry(<[In]> memid As Integer, <[In]> invkind As INVOKEKIND, <Out, MarshalAs(UnmanagedType.BStr)> pBstrDllName As String, <Out, MarshalAs(UnmanagedType.BStr)> pBstrName As String, <Out, MarshalAs(UnmanagedType.U2)> pwOrdinal As Short) As Integer
            <PreserveSig, SecurityCritical>
            Function GetRefTypeInfo(<[In]> hreftype As IntPtr, <Out> ByRef pTypeInfo As ITypeInfo) As Integer
            <PreserveSig, Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Function AddressOfMember() As Integer
            <PreserveSig, Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Function CreateInstance(<[In]> ByRef pUnkOuter As IntPtr, <[In]> ByRef riid As Guid, <Out, MarshalAs(UnmanagedType.IUnknown)> ppvObj As Object) As Integer
            <PreserveSig, Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Function GetMops(<[In]> memid As Integer, <Out, MarshalAs(UnmanagedType.BStr)> pBstrMops As String) As Integer
            <PreserveSig, SecurityCritical>
            Function GetContainingTypeLib(<Out, MarshalAs(UnmanagedType.LPArray)> ppTLib As ITypeLib(), <Out, MarshalAs(UnmanagedType.LPArray)> pIndex As Integer()) As Integer
            <PreserveSig, SecurityCritical>
            Sub ReleaseTypeAttr(typeAttr As IntPtr)
            <PreserveSig, SecurityCritical>
            Sub ReleaseFuncDesc(funcDesc As IntPtr)
            <PreserveSig, SecurityCritical>
            Sub ReleaseVarDesc(varDesc As IntPtr)
        End Interface

        <ComImport, EditorBrowsable(EditorBrowsableState.Never), Guid("00020402-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Public Interface ITypeLib
            <Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Sub RemoteGetTypeInfoCount(<Out, MarshalAs(UnmanagedType.LPArray)> pcTInfo As Integer())
            <SecurityCritical>
            Sub GetTypeInfo(<[In], MarshalAs(UnmanagedType.U4)> index As Integer, <Out, MarshalAs(UnmanagedType.LPArray)> ppTInfo As ITypeInfo())
            <SecurityCritical>
            Sub GetTypeInfoType(<[In], MarshalAs(UnmanagedType.U4)> index As Integer, <Out, MarshalAs(UnmanagedType.LPArray)> pTKind As TYPEKIND())
            <SecurityCritical>
            Sub GetTypeInfoOfGuid(<[In]> ByRef guid As Guid, <Out, MarshalAs(UnmanagedType.LPArray)> ppTInfo As ITypeInfo())
            <Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Sub RemoteGetLibAttr(<Out, MarshalAs(UnmanagedType.LPArray)> ppTLibAttr As tagTLIBATTR(), <Out, MarshalAs(UnmanagedType.LPArray)> pDummy As Integer())
            <SecurityCritical>
            Sub GetTypeComp(<Out, MarshalAs(UnmanagedType.LPArray)> ppTComp As ITypeComp())
            <Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Sub RemoteGetDocumentation(index As Integer, <[In], MarshalAs(UnmanagedType.U4)> refPtrFlags As Integer, <Out, MarshalAs(UnmanagedType.LPArray)> pBstrName As String(), <Out, MarshalAs(UnmanagedType.LPArray)> pBstrDocString As String(), <Out, MarshalAs(UnmanagedType.LPArray)> pdwHelpContext As Integer(), <Out, MarshalAs(UnmanagedType.LPArray)> pBstrHelpFile As String())
            <Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Sub RemoteIsName(<[In], MarshalAs(UnmanagedType.LPWStr)> szNameBuf As String, <[In], MarshalAs(UnmanagedType.U4)> lHashVal As Integer, <Out, MarshalAs(UnmanagedType.LPArray)> pfName As IntPtr(), <Out, MarshalAs(UnmanagedType.LPArray)> pBstrLibName As String())
            <Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical>
            Sub RemoteFindName(<[In], MarshalAs(UnmanagedType.LPWStr)> szNameBuf As String, <[In], MarshalAs(UnmanagedType.U4)> lHashVal As Integer, <Out, MarshalAs(UnmanagedType.LPArray)> ppTInfo As ITypeInfo(), <Out, MarshalAs(UnmanagedType.LPArray)> rgMemId As Integer(), <[In], Out, MarshalAs(UnmanagedType.LPArray)> pcFound As Short(), <Out, MarshalAs(UnmanagedType.LPArray)> pBstrLibName As String())
            <Obsolete("Bad signature. Fix and verify signature before use.", True), SecurityCritical> _
            Sub LocalReleaseTLibAttr()
        End Interface

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Enum tagSYSKIND
            ' Fields
            SYS_MAC = 2
            SYS_WIN16 = 0
        End Enum

        <StructLayout(LayoutKind.Sequential), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Structure tagTLIBATTR
            Public guid As Guid
            Public lcid As Integer
            Public syskind As tagSYSKIND
            <MarshalAs(UnmanagedType.U2)> _
            Public wMajorVerNum As Short
            <MarshalAs(UnmanagedType.U2)> _
            Public wMinorVerNum As Short
            <MarshalAs(UnmanagedType.U2)> _
            Public wLibFlags As Short
        End Structure
    End Class
End Namespace

