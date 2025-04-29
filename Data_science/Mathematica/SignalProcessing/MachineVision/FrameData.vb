#Region "Microsoft.VisualBasic::1e0cad6745178b3d3d43578e66249050, Data_science\Mathematica\SignalProcessing\MachineVision\FrameData.vb"

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

    '   Total Lines: 88
    '    Code Lines: 58 (65.91%)
    ' Comment Lines: 13 (14.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (19.32%)
    '     File Size: 2.53 KB


    ' Class FrameData
    ' 
    ' 
    ' 
    ' Class FrameData
    ' 
    '     Properties: Detections, FrameID
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: GenericEnumerator, SetIndex, ToString
    ' 
    ' Class Detection
    ' 
    '     Properties: FrameID, ObjectID, Position
    ' 
    '     Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' A frame data of the <see cref="Detection"/>.
''' </summary>
Public Class FrameData : Inherits FrameData(Of Detection)
End Class

Public Class FrameData(Of T As Detection) : Implements Enumeration(Of T)

    <XmlAttribute>
    Public Property FrameID As Integer

    <XmlElement("Objects")>
    Public Property Detections As T()

    ''' <summary>
    ''' get detection object by its index in current frame
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property Item(index As Integer) As T
        Get
            Return Detections(index)
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(id As Integer, detections As IEnumerable(Of Detection))
        _FrameID = id
        _Detections = detections.SafeQuery.ToArray
    End Sub

    ''' <summary>
    ''' Just set the given index value to the <see cref="FrameID"/>
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    <DebuggerStepThrough>
    Public Function SetIndex(id As Integer) As FrameData(Of T)
        FrameID = id

        For Each obj As T In Detections.SafeQuery
            obj.FrameID = id
        Next

        Return Me
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"#{FrameID} " & Detections _
            .SafeQuery _
            .Select(Function(a) a.ObjectID) _
            .GetJson
    End Function

    Public Iterator Function GenericEnumerator() As IEnumerator(Of T) Implements Enumeration(Of T).GenericEnumerator
        For Each obj As T In Detections.SafeQuery
            Yield obj
        Next
    End Function
End Class

Public Class Detection

    <XmlAttribute>
    Public Property ObjectID As String
    Public Property Position As PointF
    Public Property FrameID As Integer

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"{ObjectID} [x:{Position.X}, y:{Position.Y}]"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(obj As Detection) As PointF
        Return obj.Position
    End Operator
End Class


