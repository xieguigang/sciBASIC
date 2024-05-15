#Region "Microsoft.VisualBasic::1c84ff5b5f860d9c0840b4c2cd15d410, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Trainings\Reporter.vb"

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

    '   Total Lines: 31
    '    Code Lines: 19
    ' Comment Lines: 4
    '   Blank Lines: 8
    '     File Size: 722 B


    '     Class Reporter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DoReport, ReportUnix, ReportWindows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace NeuralNetwork

    ''' <summary>
    ''' some console api is not working well on unix platform,
    ''' the helper is used for solve such problem
    ''' </summary>
    Public Class Reporter

        ReadOnly trainer As ANNTrainer

        Sub New(trainer As ANNTrainer)
            Me.trainer = trainer
        End Sub

        Public Sub DoReport(i As Integer, errors As Double())
            If App.IsMicrosoftPlatform Then
                Call ReportWindows()
            Else
                Call ReportUnix()
            End If
        End Sub

        Private Sub ReportWindows()

        End Sub

        Private Sub ReportUnix()

        End Sub
    End Class
End Namespace
