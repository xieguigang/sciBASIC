#Region "Microsoft.VisualBasic::57ee8cfb5127b57404ae2ea8dc16a7a2, sciBASIC#\vs_solutions\installer\bootstrap\ZipLib.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 118
    '    Code Lines: 42
    ' Comment Lines: 63
    '   Blank Lines: 13
    '     File Size: 4.41 KB


    ' Module GZip
    ' 
    ' 
    '     Enum Overwrite
    ' 
    '         Always, IfNewer, Never
    ' 
    ' 
    ' 
    '     Enum ArchiveAction
    ' 
    '         [Error], Ignore, Merge, Replace
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    '     Sub: ImprovedExtractToDirectory, ImprovedExtractToFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO.Compression
Imports System.Runtime.CompilerServices

#If NET_40 = 0 Then

''' <summary>
''' Creating Zip Files Easily in .NET 4.5
''' Tim Corey, 11 May 2012
''' 
''' http://www.codeproject.com/Articles/381661/Creating-Zip-Files-Easily-in-NET
''' </summary>
''' <remarks></remarks>
''' 
Public Module GZip

    ''' <summary>
    ''' Used to specify what our overwrite policy
    ''' is for files we are extracting.
    ''' </summary>
    Public Enum Overwrite
        Always
        IfNewer
        Never
    End Enum

    ''' <summary>
    ''' Used to identify what we will do if we are
    ''' trying to create a zip file and it already
    ''' exists.
    ''' </summary>
    Public Enum ArchiveAction
        Merge
        Replace
        [Error]
        Ignore
    End Enum

    ''' <summary>
    ''' Unzips the specified file to the given folder in a safe
    ''' manner.  This plans for missing paths and existing files
    ''' and handles them gracefully.
    ''' </summary>
    ''' <param name="sourceArchiveFileName">
    ''' The name of the zip file to be extracted
    ''' </param>
    ''' <param name="destinationDirectoryName">
    ''' The directory to extract the zip file to
    ''' </param>
    ''' <param name="overwriteMethod">
    ''' Specifies how we are going to handle an existing file.
    ''' The default is IfNewer.
    ''' </param>
    ''' 
    Public Sub ImprovedExtractToDirectory(sourceArchiveFileName As String, destinationDirectoryName As String, Optional overwriteMethod As Overwrite = Overwrite.IfNewer)
        'Opens the zip file up to be read
        Using archive As ZipArchive = ZipFile.OpenRead(sourceArchiveFileName)
            'Loops through each file in the zip file
            For Each file As ZipArchiveEntry In archive.Entries
                ImprovedExtractToFile(file, destinationDirectoryName, overwriteMethod)
            Next
        End Using
    End Sub

    ''' <summary>
    ''' Safely extracts a single file from a zip file
    ''' </summary>
    ''' <param name="file__1">
    ''' The zip entry we are pulling the file from
    ''' </param>
    ''' <param name="destinationPath">
    ''' The root of where the file is going
    ''' </param>
    ''' <param name="overwriteMethod">
    ''' Specifies how we are going to handle an existing file.
    ''' The default is Overwrite.IfNewer.
    ''' </param>
    ''' 
    <Extension> Public Sub ImprovedExtractToFile(file__1 As ZipArchiveEntry, destinationPath As String, Optional overwriteMethod As Overwrite = Overwrite.IfNewer)
        'Gets the complete path for the destination file, including any
        'relative paths that were in the zip file
        Dim destinationFileName As String = IO.Path.Combine(destinationPath, file__1.FullName)

        'Gets just the new path, minus the file name so we can create the
        'directory if it does not exist
        Dim destinationFilePath As String = IO.Path.GetDirectoryName(destinationFileName)

        'Creates the directory (if it doesn't exist) for the new path
        IO.Directory.CreateDirectory(destinationFilePath)

        'Determines what to do with the file based upon the
        'method of overwriting chosen
        Select Case overwriteMethod
            Case Overwrite.Always
                'Just put the file in and overwrite anything that is found
                file__1.ExtractToFile(destinationFileName, True)

            Case Overwrite.IfNewer
                'Checks to see if the file exists, and if so, if it should
                'be overwritten
                If Not IO.File.Exists(destinationFileName) OrElse IO.File.GetLastWriteTime(destinationFileName) < file__1.LastWriteTime Then
                    'Either the file didn't exist or this file is newer, so
                    'we will extract it and overwrite any existing file
                    file__1.ExtractToFile(destinationFileName, True)
                End If

            Case Overwrite.Never
                'Put the file in if it is new but ignores the 
                'file if it already exists
                If Not IO.File.Exists(destinationFileName) Then
                    file__1.ExtractToFile(destinationFileName)
                End If

            Case Else

        End Select
    End Sub
End Module
#End If
