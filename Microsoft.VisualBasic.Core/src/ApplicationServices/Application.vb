#Region "Microsoft.VisualBasic::8dd42783dfbcd8f329dce6dfa249d7f8, Microsoft.VisualBasic.Core\src\ApplicationServices\Application.vb"

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

    '   Total Lines: 59
    '    Code Lines: 39
    ' Comment Lines: 11
    '   Blank Lines: 9
    '     File Size: 1.78 KB


    '     Class Application
    ' 
    '         Properties: ExecutablePath, ProductName, ProductVersion, StartupPath
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports AssemblyMeta = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo

Namespace ApplicationServices

    ''' <summary>
    ''' Application information
    ''' </summary>
    Public Class Application

        Shared ReadOnly main As Assembly
        Shared ReadOnly meta As AssemblyMeta

        ''' <summary>
        ''' try to fix for the winform visual designer error
        ''' </summary>
        Shared Sub New()
            On Error Resume Next

            main = Assembly.GetEntryAssembly()
            meta = main.FromAssembly
        End Sub

        ''' <summary>
        ''' Gets the path for the executable file that started the application, 
        ''' not including the executable name.
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property StartupPath As String
            Get
                If main Is Nothing Then
                    Return Environment.CurrentDirectory
                Else
                    Return Path.GetDirectoryName(main.Location)
                End If
            End Get
        End Property

        Public Shared ReadOnly Property ExecutablePath As String
            Get
                Return main.Location
            End Get
        End Property

        Public Shared ReadOnly Property ProductName As String
            Get
                Return meta.AssemblyProduct
            End Get
        End Property

        Public Shared ReadOnly Property ProductVersion As String
            Get
                Return meta.AssemblyVersion
            End Get
        End Property
    End Class
End Namespace
