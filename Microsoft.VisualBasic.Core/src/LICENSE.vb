#Region "Microsoft.VisualBasic::b662ae0812e27ad353e607293ac2a2b2, Microsoft.VisualBasic.Core\src\LICENSE.vb"

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

    '   Total Lines: 41
    '    Code Lines: 14
    ' Comment Lines: 23
    '   Blank Lines: 4
    '     File Size: 1.46 KB


    ' Module LICENSE
    ' 
    '     Properties: GPL3
    ' 
    '     Sub: GithubRepository
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

''' <summary>
''' Information about this VisualBasic App framework code module.
''' </summary>
Public Module LICENSE

    '''<summary>
    '''  Looks up a localized string similar to                     GNU GENERAL PUBLIC LICENSE
    '''                       Version 3, 29 June 2007
    '''
    ''' Copyright (C) 2007 Free Software Foundation, Inc. &lt;http://fsf.org/&gt;
    ''' Everyone is permitted to copy and distribute verbatim copies
    ''' of this license document, but changing it is not allowed.
    '''
    '''                            Preamble
    '''
    '''  The GNU General Public License is a free, copyleft license for
    '''software and other kinds of works.
    '''
    '''  The licenses for most software and other practical works are designed
    '''to take away yo [rest of string was truncated]&quot;;.
    '''</summary>
    Public ReadOnly Property GPL3 As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return My.Resources.gpl
        End Get
    End Property

    Public Const githubURL$ = "https://github.com/xieguigang/sciBASIC"

    ''' <summary>
    ''' https://github.com/xieguigang/sciBASIC
    ''' </summary>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub GithubRepository()
        Call System.Diagnostics.Process.Start(LICENSE.githubURL)
    End Sub
End Module
