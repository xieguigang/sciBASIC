#Region "Microsoft.VisualBasic::b19316480e931dee2d07a353e91503b0, Microsoft.VisualBasic.Core\LICENSE.vb"

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
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return My.Resources.gpl
        End Get
    End Property
#End If

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
