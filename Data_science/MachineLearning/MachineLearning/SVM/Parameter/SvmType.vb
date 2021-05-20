#Region "Microsoft.VisualBasic::0226bbd0b1ec3b44424b853fe3771726, Data_science\MachineLearning\MachineLearning\SVM\Parameter\SvmType.vb"

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

    '     Enum SvmType
    ' 
    '         C_SVC, EPSILON_SVR, NU_SVC, NU_SVR, ONE_CLASS
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVM
    ''' <summary>
    ''' Contains all of the types of SVM this library can model.
    ''' </summary>
    Public Enum SvmType
        ''' <summary>
        ''' C-SVC.
        ''' </summary>
        C_SVC
        ''' <summary>
        ''' nu-SVC.
        ''' </summary>
        NU_SVC
        ''' <summary>
        ''' one-class SVM
        ''' </summary>
        ONE_CLASS
        ''' <summary>
        ''' epsilon-SVR
        ''' </summary>
        EPSILON_SVR
        ''' <summary>
        ''' nu-SVR
        ''' </summary>
        NU_SVR
    End Enum
End Namespace
