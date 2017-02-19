#Region "Microsoft.VisualBasic::f0637e35e03fb51e0ab23a9a363276ae, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\LICENSE.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

''' <summary>
''' Information about this VisualBasic App framework code module.
''' </summary>
Public Module LICENSE

#If FRAMEWORD_CORE Then

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
        Get
            Return My.Resources.gpl
        End Get
    End Property
#End If

    ''' <summary>
    ''' https://github.com/xieguigang/sciBASIC
    ''' </summary>
    Public Sub GithubRepository()
        Call System.Diagnostics.Process.Start("https://github.com/xieguigang/sciBASIC")
    End Sub
End Module
