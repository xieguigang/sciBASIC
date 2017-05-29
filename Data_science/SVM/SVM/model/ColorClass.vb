#Region "Microsoft.VisualBasic::87c5003ac44f8c8b74c1bf521f75f530, ..\sciBASIC#\Data_science\SVM\SVM\model\ColorClass.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Model

    ''' <summary>
    ''' http://blog.csdn.net/v_july_v/article/details/7624837
    ''' 
    ''' ###### ���Է���
    ''' 
    ''' ������һ����άƽ�棬ƽ���������ֲ�ͬ�����ݣ��ֱ���Ȧ�Ͳ��ʾ��������Щ���������Կɷֵģ�
    ''' ���Կ�����һ��ֱ�߽����������ݷֿ�������ֱ�߾��൱��һ����ƽ�棬��ƽ��һ�ߵ����ݵ���
    ''' ��Ӧ��yȫ��-1 ����һ������Ӧ��yȫ��1
    ''' </summary>
    Public Enum ColorClass As Integer
        RED = 1
        BLUE = -1
    End Enum
End Namespace
