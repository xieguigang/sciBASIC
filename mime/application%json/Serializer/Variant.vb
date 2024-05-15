#Region "Microsoft.VisualBasic::29285dd324248baea974d922ef0c22cd, mime\application%json\Serializer\Variant.vb"

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

    '   Total Lines: 46
    '    Code Lines: 27
    ' Comment Lines: 10
    '   Blank Lines: 9
    '     File Size: 1.32 KB


    ' Class [Variant]
    ' 
    '     Properties: jsonValue
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Public MustInherit Class [Variant]

    Protected ReadOnly schemaList As Type()

    Dim m_jsonValue As Object

    ''' <summary>
    ''' 反序列化得到的结果值
    ''' </summary>
    ''' <returns></returns>
    Public Property jsonValue As Object
        Get
            Return m_jsonValue
        End Get
        Friend Set(value As Object)
            m_jsonValue = value
        End Set
    End Property

    Sub New(ParamArray schemaList As Type())
        Me.schemaList = schemaList

        If schemaList.IsNullOrEmpty Then
            Throw New InvalidProgramException
        End If
    End Sub

    ''' <summary>
    ''' 在这里应该是根据一些json文档的关键特征选择合适的类型进行反序列
    ''' 选择的过程由用户代码进行控制
    ''' </summary>
    ''' <param name="json"></param>
    ''' <returns></returns>
    Protected Friend MustOverride Function which(json As JsonObject) As Type

    Public Overrides Function ToString() As String
        If m_jsonValue Is Nothing Then
            Return $"Variant[{schemaList.Select(Function(t) t.Name).JoinBy(", ")}]"
        Else
            Return jsonValue.ToString
        End If
    End Function

End Class
