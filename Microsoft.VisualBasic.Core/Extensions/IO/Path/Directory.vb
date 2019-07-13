#Region "Microsoft.VisualBasic::63cbc4dd9aec4e57d75e98e7437fe50d, Microsoft.VisualBasic.Core\Extensions\IO\Path\Directory.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Directory
    ' 
    '         Properties: DIR
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CopyTo, Exists, GetFullPath, GetRelativePath, GetSubDirectories
    '                   IsAbsolutePath, ToString
    ' 
    '         Sub: CreateDirectory, Delete
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace FileIO

    ''' <summary>
    ''' A wrapper object for the processing of relative file path. 
    ''' </summary>
    Public Class Directory

        ''' <summary>
        ''' 当前的这个文件夹对象的文件路径
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DIR As String

        ''' <summary>
        ''' Construct a directory object from the specific Dir path value.
        ''' </summary>
        ''' <param name="DIR">Target directory path</param>
        Sub New(DIR As String)
            Me.DIR = FileSystem.GetDirectoryInfo(DIR).FullName
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSubDirectories() As IEnumerable(Of String)
            Return DIR.ListDirectory
        End Function

        ''' <summary>
        ''' Gets the full path of the target file based on the path relative to this directory object.
        ''' </summary>
        ''' <param name="file">
        ''' The relative path of the target file, and this parameter is also compatible with absolute file path.
        ''' (相对路径)</param>
        ''' <returns></returns>
        Public Function GetFullPath(file As String) As String
            If Not IsAbsolutePath(file) Then
                file = $"{DIR}/{file}"
            End If

            file = FileSystem.GetFileInfo(file).FullName
            Return file
        End Function

        ''' <summary>
        ''' Determined that the input file path is a absolute path or not?
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        Public Shared Function IsAbsolutePath(file As String) As Boolean
            If InStr(file, ":\") > 0 OrElse InStr(file, ":/") > 0 Then
                Return True
            ElseIf file.First = "/" AndAlso
                (Environment.OSVersion.Platform = PlatformID.Unix OrElse
                 Environment.OSVersion.Platform = PlatformID.MacOSX) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 将当前的这个文件夹之中的内容拷贝到<paramref name="target"/>目标文件夹
        ''' </summary>
        ''' <param name="target">The directory path of target folder.</param>
        ''' <returns></returns>
        Public Function CopyTo(target$, Optional progress As Progress(Of String) = Nothing, Optional includeSrc As Boolean = False) As IEnumerable(Of String)
            Dim list As New List(Of String)
            Dim action = Sub(path$)
                             If Not progress Is Nothing Then
                                 Call DirectCast(progress, IProgress(Of String)).Report(path)
                             End If

                             Call list.Add(path)
                         End Sub

            Call New CopyDirectoryAction(New Progress(Of String)(action)).Copy(DIR, target, includeSrc)

            Return list
        End Function

        Public Overrides Function ToString() As String
            Return DIR
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Exists(DIR As String) As Boolean
            Return IO.Directory.Exists(DIR)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRelativePath(file As String) As String
            Return PathExtensions.RelativePath(DIR, file, appendParent:=False)
        End Function

        ''' <summary>
        ''' Creates a directory.
        ''' </summary>
        ''' <param name="junctionPoint">Name and location of the directory.</param>
        ''' <remarks>
        ''' Exceptions:
        '''   T:System.ArgumentException:
        '''     The directory name is malformed. For example, it contains illegal characters
        '''     or is only white space.
        '''
        '''   T:System.ArgumentNullException:
        '''     directory is Nothing or an empty string.
        '''
        '''   T:System.IO.PathTooLongException:
        '''     The directory name is too long.
        '''
        '''   T:System.NotSupportedException:
        '''     The directory name is only a colon (:).
        '''
        '''   T:System.IO.IOException:
        '''     The parent directory of the directory to be created is read-only
        '''
        '''   T:System.UnauthorizedAccessException:
        '''     The user does not have permission to create the directory.
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub CreateDirectory(junctionPoint As String)
            Call FileSystem.CreateDirectory(junctionPoint)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub Delete(DIR As String)
            Call IO.Directory.Delete(DIR)
        End Sub
    End Class
End Namespace
