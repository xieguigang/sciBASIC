#Region "Microsoft.VisualBasic::b81e01e9c623c899fd4b059707b72cb1, Microsoft.VisualBasic.Core\src\My\UNIX\PathMapper.vb"

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

    '   Total Lines: 112
    '    Code Lines: 50
    ' Comment Lines: 47
    '   Blank Lines: 15
    '     File Size: 5.46 KB


    '     Module PathMapper
    ' 
    '         Properties: platform
    ' 
    '         Function: GetMapPath, HOME
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace My.UNIX

    ''' <summary>
    ''' Maps the linux path to the Windows path.(这个模块是将Linux路径映射为Windows路径的)
    ''' </summary>
    Public Module PathMapper

        ''' <summary>
        ''' Gets a <see cref="System.PlatformID"/> enumeration value that identifies the operating system
        ''' platform.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property platform As PlatformID = Environment.OSVersion.Platform

        ''' <summary>
        ''' Map linux path on Windows:
        ''' 
        ''' + [~ -> C:\User\&lt;user_name>]
        ''' + [# -> <see cref="App.HOME"/>]
        ''' + [/ -> C:\]
        ''' + [/usr/bin -> C:\Program Files\]
        ''' + [/usr -> C:\User\]
        ''' + [- -> <see cref="App.PreviousDirectory"/>]
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Function GetMapPath(path As String) As String
            If platform = PlatformID.MacOSX OrElse platform = PlatformID.Unix Then
                Return path
            End If

            If path.First = "~" Then
                ' HOME
                path = Mid(path, 2)

                If path.First = "/" Then
                    path = App.UserHOME & "/" & path
                Else
                    ' ~username
                    Dim DIR As String = App.UserHOME.ParentPath
                    path = DIR & "/" & path
                End If
            ElseIf path.First = "#"c Then
                path = Mid(path, 2)
                Return $"{App.HOME}/{path}"
            ElseIf path.StartsWith("//\d+(\.\d+){3}", RegexICSng) Then
                ' is a network location
                ' full path
                Return path

            ElseIf path.First = "/" Then
                ' /   ROOT
                path = "C:\" & path
            ElseIf InStr(path, "/usr/bin", CompareMethod.Text) = 1 Then
                path = Mid(path, 9)
                path = "C:\Program Files/" & path
            ElseIf InStr(path, "/usr", CompareMethod.Text) = 1 Then
                path = Mid(path, 5)
                path = App.UserHOME.ParentPath & "/" & path
            ElseIf InStr(path, "-/") = 1 Then
                ' 前一个文件夹
                path = Mid(path, 2)
                path = App.PreviousDirectory & "/" & path
            ElseIf path = "-" Then
                path = App.PreviousDirectory
            End If

            Return path
        End Function

        ''' <summary>
        ''' Get user home folder
        ''' </summary>
        ''' <returns></returns>
        Public Function HOME() As String
            If platform = PlatformID.MacOSX OrElse platform = PlatformID.Unix Then

                ' Fixed error:

                'Unhandled Exception
                'System.TypeInitializationException : The Type initializer for 'Microsoft.VisualBasic.App' threw an exception. ---> Microsoft.VisualBasic.VBDebugger+VisualBasicAppException: @HOME ---> System.Exception: Environment variable error, there is no 'HOMEDRIVE'
                '  --- End of inner exception stack trace ---
                '  at Microsoft.VisualBasic.VBDebugger.Assertion(Boolean test, System.String msg, System.String calls) < 0x410e6520 + 0x00037> in <filename unknown>:0 
                '  at Microsoft.VisualBasic.Language.UnixBash.PathMapper.HOME() < 0x410e62c0 + 0x0009f> in <filename unknown>: 0 
                '  at Microsoft.VisualBasic.App..cctor() < 0x410ddfc0 + 0x0017f> in <filename unknown>:0 
                '  --- End of inner exception stack trace ---
                '  at MathApp.Program.Main() < 0x410ddd60 + 0x0000b> in <filename unknown>: 0 
                '[ERROR] FATAL UNHANDLED EXCEPTION: System.TypeInitializationException : The Type initializer for 'Microsoft.VisualBasic.App' threw an exception. ---> Microsoft.VisualBasic.VBDebugger+VisualBasicAppException: @HOME ---> System.Exception: Environment variable error, there is no 'HOMEDRIVE'
                '  --- End of inner exception stack trace ---
                '  at Microsoft.VisualBasic.VBDebugger.Assertion(Boolean test, System.String msg, System.String calls) < 0x410e6520 + 0x00037> in <filename unknown>:0 
                '  at Microsoft.VisualBasic.Language.UnixBash.PathMapper.HOME() < 0x410e62c0 + 0x0009f> in <filename unknown>: 0 
                '  at Microsoft.VisualBasic.App..cctor() < 0x410ddfc0 + 0x0017f> in <filename unknown>:0 
                '  --- End of inner exception stack trace ---
                '  at MathApp.Program.Main() < 0x410ddd60 + 0x0000b> in <filename unknown>: 0 

                Return Environment.GetEnvironmentVariable("HOME")
            End If

            Dim homeDrive As String = Environment.GetEnvironmentVariable("HOMEDRIVE")
            Dim homePath = Environment.GetEnvironmentVariable("HOMEPATH")

            Call VBDebugger.Assertion(Not String.IsNullOrEmpty(homeDrive), "Environment variable error, there is no 'HOMEDRIVE'")
            Call VBDebugger.Assertion(Not String.IsNullOrEmpty(homePath), "Environment variable error, there is no 'HOMEPATH'")

            Dim fullHomePath As String = homeDrive & Path.DirectorySeparatorChar & homePath
            Return fullHomePath
        End Function
    End Module
End Namespace
