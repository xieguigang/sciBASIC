#Region "Microsoft.VisualBasic::d0be62a3a6e490d570d40e7ea5173dd4, Data_science\Mathematica\Math\Math\DownSampling\DSAlgorithms.vb"

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

    '   Total Lines: 92
    '    Code Lines: 59 (64.13%)
    ' Comment Lines: 16 (17.39%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (18.48%)
    '     File Size: 3.44 KB


    '     Class DSAlgorithms
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Enum InnerEnum
    ' 
    '             LTD, LTOB, LTTB, MAXMIN, PIPLOT
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ordinal, process, ToString, valueOf, values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math.DownSampling.LargestTriangleBucket
Imports Microsoft.VisualBasic.Math.DownSampling.MaxMin

Namespace DownSampling

    ''' <summary>
    ''' Downsampling algorithms for time series data（LTOB, LTTB, LTD, OSI-PI Plot）
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/avina/downsampling
    ''' </remarks>
    Public NotInheritable Class DSAlgorithms : Implements DownSamplingAlgorithm

        ''' <summary>
        ''' OSIsoft PI PlotValues </summary>
        Public Shared ReadOnly PIPLOT As New DSAlgorithms("PIPLOT", InnerEnum.PIPLOT, New PIPlotAlgorithm())
        ''' <summary>
        ''' Largest Triangle Three Bucket </summary>
        Public Shared ReadOnly LTTB As New DSAlgorithms("LTTB", InnerEnum.LTTB, (New LTABuilder()).threeBucket().fixed().build())
        ''' <summary>
        ''' Largest Triangle One Bucket </summary>
        Public Shared ReadOnly LTOB As New DSAlgorithms("LTOB", InnerEnum.LTOB, (New LTABuilder()).oneBucket().fixed().build())
        ''' <summary>
        ''' Largest Triangle Dynamic </summary>
        Public Shared ReadOnly LTD As New DSAlgorithms("LTD", InnerEnum.LTD, (New LTABuilder()).threeBucket().dynamic().build())
        ''' <summary>
        ''' Maximum and minimum value </summary>
        Public Shared ReadOnly MAXMIN As New DSAlgorithms("MAXMIN", InnerEnum.MAXMIN, New MMAlgorithm())

        Shared ReadOnly valueList As New List(Of DSAlgorithms)()

        Shared Sub New()
            valueList.Add(PIPLOT)
            valueList.Add(LTTB)
            valueList.Add(LTOB)
            valueList.Add(LTD)
            valueList.Add(MAXMIN)
        End Sub

        Public Enum InnerEnum
            PIPLOT
            LTTB
            LTOB
            LTD
            MAXMIN
        End Enum

        Public ReadOnly innerEnumValue As InnerEnum
        Private ReadOnly nameValue As String
        Private ReadOnly ordinalValue As Integer
        Private Shared nextOrdinal As Integer = 0

        Private [delegate] As DownSamplingAlgorithm

        Private Sub New(name As String, thisInnerEnumValue As InnerEnum, [delegate] As DownSamplingAlgorithm)
            Me.delegate = [delegate]

            nameValue = name
            ordinalValue = nextOrdinal
            nextOrdinal += 1
            innerEnumValue = thisInnerEnumValue
        End Sub

        Public Function process(data As IList(Of ITimeSignal), threshold As Integer) As IList(Of ITimeSignal) Implements DownSamplingAlgorithm.process
            Return [delegate].process(data, threshold)
        End Function

        Public Shared Function values() As IEnumerable(Of DSAlgorithms)
            Return valueList
        End Function

        Public Function ordinal() As Integer
            Return ordinalValue
        End Function

        Public Overrides Function ToString() As String
            Return nameValue
        End Function

        Public Shared Function valueOf(name As String) As DSAlgorithms
            For Each enumInstance As DSAlgorithms In DSAlgorithms.valueList
                If enumInstance.nameValue = name Then
                    Return enumInstance
                End If
            Next

            Throw New ArgumentException(name)
        End Function
    End Class

End Namespace
