Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class ExceptionUtils
        ' Methods
        Private Sub New()
        End Sub

        Friend Shared Function BuildException(Number As Integer, Description As String, ByRef VBDefinedError As Boolean) As Exception
            VBDefinedError = True
            Select Case Number
                Case 0
                    Return Nothing
                Case 3, 20, &H5E, 100
                    Return New InvalidOperationException(Description)
                Case 5, &H1BE, &H1C0, &H1C1
                    Return New ArgumentException(Description)
                Case 6
                    Return New OverflowException(Description)
                Case 7, 14
                    Return New OutOfMemoryException(Description)
                Case 9
                    Return New IndexOutOfRangeException(Description)
                Case 11
                    Return New DivideByZeroException(Description)
                Case 13
                    Return New InvalidCastException(Description)
                Case -2147467261
                    Return New AccessViolationException
                Case &H5B
                    Return New NullReferenceException(Description)
                Case &H30
                    Return New TypeLoadException(Description)
                Case &H34, &H36, &H37, &H39, &H3A, &H3B, &H3D, &H3F, &H43, &H44, 70, &H47, &H4A, &H4B
                    Return New IOException(Description)
                Case &H35
                    Return New FileNotFoundException(Description)
                Case &H3E
                    Return New EndOfStreamException(Description)
                Case &H4C, &H1B0
                    Return New FileNotFoundException(Description)
                Case &H1C
                    Return New StackOverflowException(Description)
                Case &H1A6
                    Return New MissingFieldException(Description)
                Case &H1AD, &H1CE
                    Return New Exception(Description)
                Case &H1B6
                    Return New MissingMemberException(Description)
            End Select
            VBDefinedError = False
            Return New Exception(Description)
        End Function

        Friend Shared Function GetArgumentExceptionWithArgName(ArgumentName As String, ResourceID As String, ParamArray PlaceHolders As String()) As ArgumentException
            Return New ArgumentException(Utils.GetResourceString(ResourceID, PlaceHolders), ArgumentName)
        End Function

        Friend Shared Function GetArgumentNullException(ArgumentName As String) As ArgumentNullException
            Return New ArgumentNullException(ArgumentName, Utils.GetResourceString("General_ArgumentNullException"))
        End Function

        Friend Shared Function GetArgumentNullException(ArgumentName As String, ResourceID As String, ParamArray PlaceHolders As String()) As ArgumentNullException
            Return New ArgumentNullException(ArgumentName, Utils.GetResourceString(ResourceID, PlaceHolders))
        End Function

        Friend Shared Function GetDirectoryNotFoundException(ResourceID As String, ParamArray PlaceHolders As String()) As DirectoryNotFoundException
            Return New DirectoryNotFoundException(Utils.GetResourceString(ResourceID, PlaceHolders))
        End Function

        Friend Shared Function GetFileNotFoundException(FileName As String, ResourceID As String, ParamArray PlaceHolders As String()) As FileNotFoundException
            Return New FileNotFoundException(Utils.GetResourceString(ResourceID, PlaceHolders), FileName)
        End Function

        Friend Shared Function GetInvalidOperationException(ResourceID As String, ParamArray PlaceHolders As String()) As InvalidOperationException
            Return New InvalidOperationException(Utils.GetResourceString(ResourceID, PlaceHolders))
        End Function

        Friend Shared Function GetIOException(ResourceID As String, ParamArray PlaceHolders As String()) As IOException
            Return New IOException(Utils.GetResourceString(ResourceID, PlaceHolders))
        End Function

        <SecurityCritical>
        Friend Shared Function GetWin32Exception(ResourceID As String, ParamArray PlaceHolders As String()) As Win32Exception
            Return New Win32Exception(Marshal.GetLastWin32Error, Utils.GetResourceString(ResourceID, PlaceHolders))
        End Function

        Friend Shared Function MakeException1(hr As Integer, Parm1 As String) As Exception
            Dim resourceString As String
            If ((hr > 0) AndAlso (hr <= &HFFFF)) Then
                resourceString = Utils.GetResourceString(DirectCast(hr, vbErrors))
            Else
                resourceString = ""
            End If
            Dim index As Integer = resourceString.IndexOf("%1", StringComparison.OrdinalIgnoreCase)
            If (index >= 0) Then
                resourceString = (resourceString.Substring(0, index) & Parm1 & resourceString.Substring((index + 2)))
            End If
            Return ExceptionUtils.VbMakeExceptionEx(hr, resourceString)
        End Function

        Friend Shared Function VbMakeException(hr As Integer) As Exception
            Dim resourceString As String
            If ((hr > 0) AndAlso (hr <= &HFFFF)) Then
                resourceString = Utils.GetResourceString(DirectCast(hr, vbErrors))
            Else
                resourceString = ""
            End If
            Return ExceptionUtils.VbMakeExceptionEx(hr, resourceString)
        End Function

        Friend Shared Function VbMakeException(ex As Exception, hr As Integer) As Exception
            Information.Err.SetUnmappedError(hr)
            Return ex
        End Function

        Friend Shared Function VbMakeExceptionEx(Number As Integer, sMsg As String) As Exception
            Dim flag As Boolean
            Dim exception As Exception = ExceptionUtils.BuildException(Number, sMsg, flag)
            If flag Then
                Information.Err.SetUnmappedError(Number)
            End If
            Return exception
        End Function


        ' Fields
        Friend Const CLASS_E_NOTLICENSED As Integer = -2147221230
        Friend Const CO_E_APPDIDNTREG As Integer = -2147220994
        Friend Const CO_E_APPNOTFOUND As Integer = -2147221003
        Friend Const CO_E_CLASSSTRING As Integer = -2147221005
        Friend Const CO_E_SERVER_EXEC_FAILURE As Integer = -2146959355
        Friend Const DISP_E_ARRAYISLOCKED As Integer = -2147352563
        Friend Const DISP_E_BADINDEX As Integer = -2147352565
        Friend Const DISP_E_BADPARAMCOUNT As Integer = -2147352562
        Friend Const DISP_E_BADVARTYPE As Integer = -2147352568
        Friend Const DISP_E_DIVBYZERO As Integer = -2147352558
        Friend Const DISP_E_MEMBERNOTFOUND As Integer = -2147352573
        Friend Const DISP_E_NONAMEDARGS As Integer = -2147352569
        Friend Const DISP_E_NOTACOLLECTION As Integer = -2147352559
        Friend Const DISP_E_OVERFLOW As Integer = -2147352566
        Friend Const DISP_E_PARAMNOTFOUND As Integer = -2147352572
        Friend Const DISP_E_PARAMNOTOPTIONAL As Integer = -2147352561
        Friend Const DISP_E_TYPEMISMATCH As Integer = -2147352571
        Friend Const DISP_E_UNKNOWNINTERFACE As Integer = -2147352575
        Friend Const DISP_E_UNKNOWNLCID As Integer = -2147352564
        Friend Const DISP_E_UNKNOWNNAME As Integer = -2147352570
        Friend Const E_ABORT As Integer = -2147467260
        Friend Const E_ACCESSDENIED As Integer = -2147024891
        Friend Const E_INVALIDARG As Integer = -2147024809
        Friend Const E_NOINTERFACE As Integer = -2147467262
        Friend Const E_NOTIMPL As Integer = -2147467263
        Friend Const E_OUTOFMEMORY As Integer = -2147024882
        Friend Const E_POINTER As Integer = -2147467261
        Friend Const MK_E_CANTOPENFILE As Integer = -2147221014
        Friend Const MK_E_INVALIDEXTENSION As Integer = -2147221018
        Friend Const MK_E_UNAVAILABLE As Integer = -2147221021
        Friend Const REGDB_E_CLASSNOTREG As Integer = -2147221164
        Friend Const STG_E_ACCESSDENIED As Integer = -2147287035
        Friend Const STG_E_CANTSAVE As Integer = -2147286781
        Friend Const STG_E_DISKISWRITEPROTECTED As Integer = -2147287021
        Friend Const STG_E_EXTANTMARSHALLINGS As Integer = -2147286776
        Friend Const STG_E_FILEALREADYEXISTS As Integer = -2147286960
        Friend Const STG_E_FILENOTFOUND As Integer = -2147287038
        Friend Const STG_E_INSUFFICIENTMEMORY As Integer = -2147287032
        Friend Const STG_E_INUSE As Integer = -2147286784
        Friend Const STG_E_INVALIDFUNCTION As Integer = -2147287039
        Friend Const STG_E_INVALIDHANDLE As Integer = -2147287034
        Friend Const STG_E_INVALIDHEADER As Integer = -2147286789
        Friend Const STG_E_INVALIDNAME As Integer = -2147286788
        Friend Const STG_E_LOCKVIOLATION As Integer = -2147287007
        Friend Const STG_E_MEDIUMFULL As Integer = -2147286928
        Friend Const STG_E_NOMOREFILES As Integer = -2147287022
        Friend Const STG_E_NOTCURRENT As Integer = -2147286783
        Friend Const STG_E_NOTFILEBASEDSTORAGE As Integer = -2147286777
        Friend Const STG_E_OLDDLL As Integer = -2147286779
        Friend Const STG_E_OLDFORMAT As Integer = -2147286780
        Friend Const STG_E_PATHNOTFOUND As Integer = -2147287037
        Friend Const STG_E_READFAULT As Integer = -2147287010
        Friend Const STG_E_REVERTED As Integer = -2147286782
        Friend Const STG_E_SEEKERROR As Integer = -2147287015
        Friend Const STG_E_SHAREREQUIRED As Integer = -2147286778
        Friend Const STG_E_SHAREVIOLATION As Integer = -2147287008
        Friend Const STG_E_TOOMANYOPENFILES As Integer = -2147287036
        Friend Const STG_E_UNIMPLEMENTEDFUNCTION As Integer = -2147286786
        Friend Const STG_E_UNKNOWN As Integer = -2147286787
        Friend Const STG_E_WRITEFAULT As Integer = -2147287011
        Friend Const TYPE_E_AMBIGUOUSNAME As Integer = -2147319764
        Friend Const TYPE_E_BADMODULEKIND As Integer = -2147317571
        Friend Const TYPE_E_BUFFERTOOSMALL As Integer = -2147319786
        Friend Const TYPE_E_CANTCREATETMPFILE As Integer = -2147316573
        Friend Const TYPE_E_CANTLOADLIBRARY As Integer = -2147312566
        Friend Const TYPE_E_CIRCULARTYPE As Integer = -2147312508
        Friend Const TYPE_E_DLLFUNCTIONNOTFOUND As Integer = -2147319761
        Friend Const TYPE_E_ELEMENTNOTFOUND As Integer = -2147319765
        Friend Const TYPE_E_INCONSISTENTPROPFUNCS As Integer = -2147312509
        Friend Const TYPE_E_INVALIDSTATE As Integer = -2147319767
        Friend Const TYPE_E_INVDATAREAD As Integer = -2147319784
        Friend Const TYPE_E_IOERROR As Integer = -2147316574
        Friend Const TYPE_E_LIBNOTREGISTERED As Integer = -2147319779
        Friend Const TYPE_E_NAMECONFLICT As Integer = -2147319763
        Friend Const TYPE_E_OUTOFBOUNDS As Integer = -2147316575
        Friend Const TYPE_E_QUALIFIEDNAMEDISALLOWED As Integer = -2147319768
        Friend Const TYPE_E_REGISTRYACCESS As Integer = -2147319780
        Friend Const TYPE_E_SIZETOOBIG As Integer = -2147317563
        Friend Const TYPE_E_TYPEMISMATCH As Integer = -2147316576
        Friend Const TYPE_E_UNDEFINEDTYPE As Integer = -2147319769
        Friend Const TYPE_E_UNKNOWNLCID As Integer = -2147319762
        Friend Const TYPE_E_UNSUPFORMAT As Integer = -2147319783
        Friend Const TYPE_E_WRONGTYPEKIND As Integer = -2147319766
    End Class
End Namespace

