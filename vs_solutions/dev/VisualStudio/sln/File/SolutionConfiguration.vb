#Region "Microsoft.VisualBasic::fb9295c32333323bb04871f4d250efc8, vs_solutions\dev\VisualStudio\sln\File\SolutionConfiguration.vb"

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

    '   Total Lines: 40
    '    Code Lines: 22 (55.00%)
    ' Comment Lines: 12 (30.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (15.00%)
    '     File Size: 1.17 KB


    '     Class SolutionConfiguration
    ' 
    '         Properties: Configuration, Name, Platform
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace sln.File

    ''' <summary>
    ''' A solution level build configuration / platform pair, e.g. ``Debug|AnyCPU``.
    ''' </summary>
    Public Class SolutionConfiguration
        ''' <summary>
        ''' The combined name, e.g. ``Debug|AnyCPU``.
        ''' </summary>
        Public Property Name As String
        ''' <summary>
        ''' The configuration part, e.g. ``Debug``.
        ''' </summary>
        Public Property Configuration As String
        ''' <summary>
        ''' The platform part, e.g. ``AnyCPU``.
        ''' </summary>
        Public Property Platform As String

        Public Sub New()
        End Sub

        Public Sub New(name As String)
            Me.Name = name

            If name IsNot Nothing Then
                Dim parts = name.Split({"|"c}, 2)
                Configuration = parts(0)

                If parts.Length > 1 Then
                    Platform = parts(1)
                End If
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Configuration}/{Platform}"
        End Function
    End Class
End Namespace
