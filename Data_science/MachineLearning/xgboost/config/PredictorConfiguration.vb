#Region "Microsoft.VisualBasic::762b8970835d4a2c2f134873def34f49, Data_science\MachineLearning\xgboost\config\PredictorConfiguration.vb"

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

    '   Total Lines: 32
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.11 KB


    '     Class PredictorConfiguration
    ' 
    '         Properties: objFunction
    ' 
    '         Function: builder
    '         Class BuilderType
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: build, objFunction
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.XGBoost.learner

Namespace config
    Public Class PredictorConfiguration
        Public Class BuilderType
            Friend predictorConfiguration As PredictorConfiguration

            Friend Sub New()
                predictorConfiguration = New PredictorConfiguration()
            End Sub

            Public Overridable Function objFunction(objFunc As ObjFunction) As BuilderType
                predictorConfiguration._objFunction = objFunc
                Return Me
            End Function

            Public Overridable Function build() As PredictorConfiguration
                Dim result = predictorConfiguration
                predictorConfiguration = Nothing
                Return result
            End Function
        End Class

        Public Shared ReadOnly [DEFAULT] As New PredictorConfiguration()

        Public Overridable ReadOnly Property objFunction As ObjFunction

        Public Shared Function builder() As BuilderType
            Return New BuilderType()
        End Function
    End Class
End Namespace
