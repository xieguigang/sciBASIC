#Region "Microsoft.VisualBasic::349c20bbbd39b44f1c960f41bf9eb341, Data_science\MachineLearning\MachineLearning\QLearning\Action.vb"

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

    '   Total Lines: 37
    '    Code Lines: 10 (27.03%)
    ' Comment Lines: 22 (59.46%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (13.51%)
    '     File Size: 1.49 KB


    '     Class Action
    ' 
    '         Properties: EnvirState, Qvalues
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace QLearning

    ''' <summary>
    ''' One specific environment state have some possible actions,
    ''' but there is just one best action on the current environment state based on the accumulate q-values
    ''' </summary>
    Public Class Action : Implements INamedValue

        ''' <summary>
        ''' The environment variables state as inputs for the machine.
        ''' </summary>
        ''' <returns>A string expression which uniquely describes the environment state.</returns>
        Public Property EnvirState As String Implements INamedValue.Key
        ''' <summary>
        ''' Actions for the current state.
        ''' </summary>
        ''' <returns>
        ''' The accumulated Q-value of each possible action which is taken on 
        ''' the current environment state.
        ''' </returns>
        Public Property Qvalues As Single()

        ''' <summary>
        ''' Environment -> actions' Q-values
        ''' </summary>
        ''' <returns>
        ''' A string in format like ``[ state ] \t--> [ q1 \t q2 \t ... ]``, in 
        ''' which the Q-values are displayed with a precision of 4 decimal places.
        ''' </returns>
        Public Overrides Function ToString() As String
            Return $"[ {EnvirState} ] {vbTab}--> [{Qvalues.Select(Function(di) di.ToString("F4")).JoinBy(vbTab)}]"
        End Function

    End Class
End Namespace
