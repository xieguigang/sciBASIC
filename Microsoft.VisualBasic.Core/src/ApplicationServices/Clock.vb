#Region "Microsoft.VisualBasic::fac0ffa78c93afafe8b4027d2830e7de, Microsoft.VisualBasic.Core\src\ApplicationServices\Clock.vb"

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

    '   Total Lines: 74
    '    Code Lines: 56
    ' Comment Lines: 0
    '   Blank Lines: 18
    '     File Size: 1.95 KB


    '     Class Clock
    ' 
    '         Properties: Instance
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: elapsedMillis, elapsedSeconds, running, systemElapsedMillis
    ' 
    '         Sub: [stop], pause, reset, start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices

    Public Class Clock

        Private Shared INSTANCEField As Clock

        Private milliseconds As Long

        Private m_running As Boolean

        Private startTime As Long

        Private stopTime As Long

        Public Shared ReadOnly Property Instance As Clock
            Get
                If INSTANCEField Is Nothing Then
                    INSTANCEField = New Clock()
                End If
                Return INSTANCEField
            End Get
        End Property

        Public Sub New()
            milliseconds = 0
            stopTime = CurrentUnixTimeMillis
            m_running = False
        End Sub

        Public Overridable Sub start()
            m_running = True
            startTime = CurrentUnixTimeMillis
        End Sub

        Public Overridable Sub [stop]()
            milliseconds = 0
            m_running = False
            stopTime = CurrentUnixTimeMillis
        End Sub

        Public Overridable Sub pause()
            m_running = False
            stopTime = CurrentUnixTimeMillis
            milliseconds += stopTime - startTime
        End Sub

        Public Overridable Sub reset()
            [stop]()
            start()
        End Sub

        Public Overridable Function elapsedMillis() As Long
            If m_running Then
                Return milliseconds + CurrentUnixTimeMillis - startTime
            Else
                Return milliseconds
            End If
        End Function

        Public Overridable Function systemElapsedMillis() As Long
            Return CurrentUnixTimeMillis
        End Function

        Public Overridable Function elapsedSeconds() As Long
            Return elapsedMillis() / 1000
        End Function

        Public Overridable Function running() As Boolean
            Return m_running
        End Function

    End Class

End Namespace
