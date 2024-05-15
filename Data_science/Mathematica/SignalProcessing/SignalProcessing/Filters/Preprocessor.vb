#Region "Microsoft.VisualBasic::a62d3d8ddfa8e874a9da14bb5983750d, Data_science\Mathematica\SignalProcessing\SignalProcessing\Filters\Preprocessor.vb"

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

    '   Total Lines: 33
    '    Code Lines: 5
    ' Comment Lines: 25
    '   Blank Lines: 3
    '     File Size: 1.17 KB


    '     Interface Preprocessor
    ' 
    '         Sub: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  Copyright [2009] [Marcin Rzeźnicki]
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
' http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' 

Namespace Filters
    ''' <summary>
    ''' This interface represents types which are able to perform data processing in
    ''' place. Useful examples include: eliminating zeros, padding etc.
    ''' 
    ''' @author Marcin Rzeźnicki </summary>
    Public Interface Preprocessor

        ''' <summary>
        ''' Data processing method. Called on Preprocessor instance when its
        ''' processing is needed
        ''' </summary>
        ''' <param name="data"> </param>
        Sub apply(data As Double())
    End Interface

End Namespace
