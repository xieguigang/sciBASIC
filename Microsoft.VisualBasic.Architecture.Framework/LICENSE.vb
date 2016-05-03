
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
    ''' https://github.com/xieguigang/VisualBasic_AppFramework
    ''' </summary>
    Public Sub GithubRepository()
        Call System.Diagnostics.Process.Start("https://github.com/xieguigang/VisualBasic_AppFramework")
    End Sub
End Module
