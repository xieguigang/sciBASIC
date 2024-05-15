#Region "Microsoft.VisualBasic::339384eaade8e50422dd7444a55d7fff, Data_science\Mathematica\Math\KaplanMeierEstimator\Models\Patient.vb"

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

    '   Total Lines: 9
    '    Code Lines: 7
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 225 B


    '     Class Patient
    ' 
    '         Properties: CensorEvent, CensorEventTime, Id
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Models
    Public Class Patient
        Public Property Id As Integer

        Public Property CensorEvent As EventFreeSurvival

        Public Property CensorEventTime As Integer
    End Class
End Namespace
