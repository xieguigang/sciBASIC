#Region "Microsoft.VisualBasic::fa5c2171f182323d2302eaedecdf33a9, cuda\ILCuda\Interop\CUresult\nvrtcResult.vb"

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

    '   Total Lines: 17
    '    Code Lines: 14 (82.35%)
    ' Comment Lines: 3 (17.65%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 603 B


    ' Enum nvrtcResult
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' NVRTC 返回码（nvrtcResult）
''' </summary>
Public Enum nvrtcResult
    NVRTC_SUCCESS = 0
    NVRTC_ERROR_OUT_OF_MEMORY = 1
    NVRTC_ERROR_PROGRAM_CREATION_FAILURE = 2
    NVRTC_ERROR_INVALID_INPUT = 3
    NVRTC_ERROR_INVALID_PROGRAM = 4
    NVRTC_ERROR_INVALID_OPTION = 5
    NVRTC_ERROR_COMPILATION_FAILURE = 6
    NVRTC_ERROR_BUILTIN_OPERATION_FAILURE = 7
    NVRTC_ERROR_NO_NAME_EXPRESSIONS_AFTER_COMPILATION = 8
    NVRTC_ERROR_NO_LOWERED_NAMES_BEFORE_COMPILATION = 9
    NVRTC_ERROR_NAME_EXPRESSION_NOT_VALID = 10
    NVRTC_ERROR_INTERNAL_ERROR = 11
End Enum
