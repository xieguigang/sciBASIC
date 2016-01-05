Imports System.Text.RegularExpressions

Namespace ConsoleDevice.STDIO__

    Module I_FormatProvider

#Region "格式控制定义区域"

        ''' <summary>
        ''' 对字符串进行格式控制输出的类对象所必须实现的格式输出接口
        ''' </summary>
        ''' <remarks></remarks>
        Friend Interface IFormatProvider
            ''' <summary>
            ''' 将一个输入的字符串按照用户指定的格式进行格式化后返回
            ''' </summary>
            ''' <param name="_format">指定的格式字符串</param>
            ''' <param name="value">输入的值</param>
            ''' <returns>经过格式化之后的字符串</returns>
            ''' <remarks></remarks>
            Function ActionFormat(_format As String, value As String) As String
        End Interface

        ''' <summary>
        ''' g格式符：它将根据数值的大小，自动选用f格式或e格式输出数据，并且它不输出无意义的0
        ''' </summary>
        ''' <remarks></remarks>
        Friend Class g : Implements STDIO__.I_FormatProvider.IFormatProvider

            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                '如果该数字的位数大于7位，则以指数的形式输出
                If CStr(CType(Val(value), Long)).Length > 7 Then Return (New STDIO__.I_FormatProvider.e).FormatString("%e", value)
                '较小的时候则以实数的形式输出
                Return (New STDIO__.I_FormatProvider.f).FormatString("%f", value)
            End Function
        End Class

        ''' <summary>
        ''' %%格式，直接返回一个字符'%'
        ''' </summary>
        ''' <remarks></remarks>
        Friend Class P : Implements STDIO__.I_FormatProvider.IFormatProvider

            Public Function ActionFormat(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                Return "%"
            End Function
        End Class

        ''' <summary>
        ''' e格式符：以指数形式输出实数
        ''' </summary>
        ''' <remarks>
        ''' 'e格式符：以指数形式输出实数
        ''' '+----------+----------------------------------------------------------------------+
        ''' '   %e        按规范化指数形式输出实数，系统自动给出6位小数，指数部分占5位
        ''' '   %m.ne     与前面的叙述相同
        ''' '   %-m.ne    与前面的叙述相同 
        ''' '+---------------------------------------------------------------------------------+
        ''' </remarks>
        Friend Class e : Implements STDIO__.I_FormatProvider.IFormatProvider

            ''' <summary>
            ''' %m.ne
            ''' </summary>
            ''' <remarks></remarks>
            Dim _m_ne As Regex = New Regex("%\d+[.]\d+e")
            ''' <summary>
            ''' %-m.ne
            ''' </summary>
            ''' <remarks></remarks>
            Dim __m_ne As Regex = New Regex("%-\d+[.]\d+e")

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="_format"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                value = CStr(Val(value)) '首先转化为Double，将字符串中头部的可能存在的0去除
                '取得整数部位
                Dim pInt As Long = STDIO__.I_FormatProvider.f.pIntr(value)
                '求出其指数部分
                Dim en As Integer = CStr(pInt).Length - 1
                '将整个数字变为小数
                If pInt < 0 Then value = Mid$(value, 1, 2) & "." & Mid$(value, 3) Else value = Mid$(value, 1, 1) & "." & Mid$(value, 2)
                '到这里，value被转化为指数的标准形式

                '%-m.ne
                If __m_ne.Match(_format).Success Then
                    Return __m_nef(STDIO__.I_FormatProvider.s.pm(_format), STDIO__.I_FormatProvider.s.pn(_format), value, en)
                End If
                '%m.ne
                If _m_ne.Match(_format).Success Then
                    Return _m_nef(STDIO__.I_FormatProvider.s.pm(_format), STDIO__.I_FormatProvider.s.pn(_format), value, en)
                End If

                Return _m_nef(0, 6, value, en)
            End Function

#Region "格式控制符"
            ''' <summary>
            ''' %m.ne
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="n"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function _m_nef(m As Integer, n As Integer, value As String, en As Integer) As String
                value = STDIO__.I_FormatProvider.f.df(n, value) & "e" & en

                If value.Length < m Then
                    Return Space(m - value.Length) & value
                Else
                    Return value
                End If
            End Function

            ''' <summary>
            ''' %-m.ne
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="n"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function __m_nef(m As Integer, n As Integer, value As String, en As Integer) As String
                value = STDIO__.I_FormatProvider.f.df(n, value) & "e" & en

                If value.Length < m Then
                    Return value & Space(m - value.Length)
                Else
                    Return value
                End If
            End Function
#End Region
        End Class

        ''' <summary>
        ''' f格式符：按实数格式输出
        ''' </summary>
        ''' <remarks>
        ''' 'f格式符：按实数格式输出。
        ''' '+----------+------------------------------------------------------------------------------------+
        ''' '   %f         整数部分全部显示出来,小数部分显示6位.但并不是显示的所有数字都是有效数字
        ''' '   %m.nf      指定数据的宽度共为m列,其中有n位小数.如果数值长度小于m，则左侧补空格。
        ''' '   %-m.nf     与%m.f类似，只是应在右侧补空格
        ''' '+-----------------------------------------------------------------------------------------------+
        ''' </remarks>
        Friend Class f : Implements STDIO__.I_FormatProvider.IFormatProvider

            ''' <summary>
            ''' %m.nf
            ''' </summary>
            ''' <remarks></remarks>
            Dim _m_nf As Regex = New Regex("%\d+[.]\d+f")
            ''' <summary>
            ''' %-m.nf
            ''' </summary>
            ''' <remarks></remarks>
            Dim __m_nf As Regex = New Regex("%-\d+[.]\d+f")

            ''' <summary>
            ''' 默认的小数位长度为6位
            ''' </summary>
            ''' <remarks></remarks>
            Public Const [Default] As Integer = 6

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="_format"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                '%-m.nf
                If __m_nf.Match(_format).Success Then Return __m_nff(pm(_format), pn(_format), value)
                '%m.nf
                If _m_nf.Match(_format).Success Then Return _m_nff(pm(_format), pn(_format), value)

                '%f
                Return df([Default], value)
            End Function

            Private Function pn(_format As String) As Integer
                Return Val(Regex.Match(_format, "[.]\d+").Value.Substring(1))
            End Function

            Private Function pm(_format As String) As Integer
                Return Val(Regex.Match(_format, "\d+[.]?").Value)
            End Function

            ''' <summary>
            ''' 控制小数的位数后输出
            ''' </summary>
            ''' <param name="n"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function df(n As Integer, value As String) As String
                '控制小数位数
                Dim srtn As String = pIntr(value).ToString
                Dim sDeci As String = pDeci(n, value).ToString
                If Val(sDeci) > 0 Then srtn = "%i.%d".Replace("%i", srtn).Replace("%d", sDeci)

                Return srtn
            End Function

#Region "格式控制字符串"
            ''' <summary>
            ''' %-m.nf
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="n"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function __m_nff(m As Integer, n As Integer, value As String) As String
                '控制小数位数
                Dim srtn As String = df(n, value)

                If srtn.Length < m Then
                    Return srtn & Space(m - srtn.Length)
                Else
                    Return srtn
                End If
            End Function

            ''' <summary>
            ''' %m.nf
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="n"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function _m_nff(m As Integer, n As Integer, value As String) As String
                '控制小数位数
                Dim srtn As String = df(n, value)

                If srtn.Length < m Then
                    Return Space(m - srtn.Length) & srtn
                Else
                    Return srtn
                End If
            End Function

            ''' <summary>
            ''' 解析整数部分
            ''' </summary>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function pIntr(value As String) As Long
                Return CType(Val(value), Long)
            End Function

            ''' <summary>
            ''' 解析小数部分
            ''' </summary>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function pDeci(n As Integer, value As String) As ULong
                Dim decreg As Regex = New Regex("[.]\d+")
                Dim decmch As Match = decreg.Match(value)
                If decmch.Success Then
                    If decmch.Value.Length - 1 > n Then
                        Return decmch.Value.Substring(1, n)
                    Else
                        Return decmch.Value.Substring(1)
                    End If
                Else
                    Return 0    '没有小数部分则返回0
                End If
            End Function
#End Region
        End Class

        ''' <summary>
        ''' s格式符：用来输出一个字符串
        ''' </summary>
        ''' <remarks>
        ''' 's格式符：用来输出一个字符串
        ''' '+----------+------------------------------------------------------------------------------------+
        ''' '   %s         用来输出一个字符串，不含双引号. 例:printf("%s","CHINA");
        ''' '   %ms        m指定宽度（字符串长度小于m时左补空格，大于时按实际宽度输出）
        ''' '   %-ms       左对齐，不足m时右补空格
        ''' '   %m.ns      输出占m列，只取字符串中左端n个字符．这n各字符输出在m列的右侧，左补空格．
        ''' '   %-m.ns     同上，右补空格 
        ''' '+-----------------------------------------------------------------------------------------------+
        ''' </remarks>
        Friend Class s : Implements STDIO__.I_FormatProvider.IFormatProvider

            ''' <summary>
            ''' %ms
            ''' </summary>
            ''' <remarks></remarks>
            Dim _ms As Regex = New Regex("%\d+s")
            ''' <summary>
            ''' %-ms
            ''' </summary>
            ''' <remarks></remarks>
            Dim __ms As Regex = New Regex("%-\d+s")
            ''' <summary>
            ''' %m.ns
            ''' </summary>
            ''' <remarks></remarks>
            Dim _m_ns As Regex = New Regex("%\d+[.]\d+s")
            ''' <summary>
            ''' %-m.ns
            ''' </summary>
            ''' <remarks></remarks>
            Dim __m_ns As Regex = New Regex("%-\d+[.]\d+s")

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="_format"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                '%-m.ns
                If __m_ns.Match(_format).Success Then Return __m_nsf(pm(_format), pn(_format), value)
                '%-ms
                If __ms.Match(_format).Success Then Return __msf(pm(_format), value)
                '%m.ns
                If _m_ns.Match(_format).Success Then Return _m_nsf(pm(_format), pn(_format), value)
                '%ms
                If _ms.Match(_format).Success Then Return _msf(pm(_format), value)

                '%s
                Return value
            End Function

            Protected Friend Shared Function pn(_format As String) As Integer
                Return Val(Regex.Match(_format, "[.]\d+").Value.Substring(1))
            End Function

            Protected Friend Shared Function pm(_format As String) As Integer
                Return Val(Regex.Match(_format, "\d+[.]?").Value)
            End Function

#Region "格式输出函数"
            ''' <summary>
            ''' %-m.ns
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="n"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function __m_nsf(m As Integer, n As Integer, value As String) As String
                If value.Length > n Then value = Mid$(value, 1, n)
                Return __msf(m, value)
            End Function

            ''' <summary>
            ''' %m.ns
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="n"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function _m_nsf(m As Integer, n As Integer, value As String) As String
                If value.Length > n Then value = Mid$(value, 1, n)
                Return _msf(m, value)
            End Function

            ''' <summary>
            ''' %-ms
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function __msf(m As Integer, value As String) As String
                If value.Length < m Then
                    Return value & Space(m - value.Length)
                Else
                    Return value
                End If
            End Function

            ''' <summary>
            ''' %ms
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function _msf(m As Integer, value As String) As String
                If value.Length < m Then
                    Return Space(m - value.Length) & value
                Else
                    Return value
                End If
            End Function
#End Region
        End Class

        ''' <summary>
        ''' c格式符：用来输出一个字符。一个整数，其值在0～255之间时也可以以字符的格式输出
        ''' </summary>
        ''' <remarks>一个整数，若其值在0～255范围内，也可以用字符形式输出，在输出前，将该整数转换为对应的ASCII字符。反之，一个字符数据也可以用整数形式输出</remarks>
        Friend Class c : Implements STDIO__.I_FormatProvider.IFormatProvider
            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                If value.Length > 1 Then
                    Return ChrW(Val(value))     '是数字，则将其转化为字符后输出
                Else                            '假若输入的字符串参数为一个字符串的话，则只输出其第一个字符
                    Return value.ToArray.First
                End If
            End Function
        End Class

        ''' <summary>
        ''' u格式符：以十进制数形式输出unsigned的整数
        ''' </summary>
        ''' <remarks></remarks>
        Friend Class u : Implements STDIO__.I_FormatProvider.IFormatProvider

            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                Return System.Convert.ToUInt64(value)
            End Function
        End Class

        ''' <summary>
        ''' x格式符：按十六进制格式输出整数。(不会出现负数格式) 
        ''' </summary>
        ''' <remarks></remarks>
        Friend Class x : Implements STDIO__.I_FormatProvider.IFormatProvider

            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                Return System.Convert.ToString(CType(Val(value), Long), 16)
            End Function
        End Class

        ''' <summary>
        ''' o(字母)格式符：按八进制格式输出整数。(不会出现负数格式)
        ''' </summary>
        ''' <remarks></remarks>
        Friend Class o : Implements STDIO__.I_FormatProvider.IFormatProvider

            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                Return System.Convert.ToString(CType(Val(value), Long), 8)
            End Function
        End Class

        ''' <summary>
        ''' b格式符：按照二进制格式输出整数
        ''' </summary>
        ''' <remarks></remarks>
        Friend Class b : Implements STDIO__.I_FormatProvider.IFormatProvider

            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                Return System.Convert.ToString(CType(Val(value), Long), 2)
            End Function
        End Class

        ''' <summary>
        ''' d格式符：按十进制格式输出
        ''' </summary>
        ''' <remarks>
        ''' 'd格式符：按十进制格式输出
        ''' '+-------------+----------------------------------------------------------------------------------------------------------+
        ''' '  %d              输出数字长度为变量数值的实际长度 
        ''' '  %md             m指定输出数据的宽度。当数据本身的实际宽度小于m时，则数据左端补空格；若大于m，则按数据的实际位数输出。 
        ''' '  %ld, %mld       l(小写字母L)表示输出“长整型”数据 
        ''' '  %0md, %0mld     0(数字0)表示位数不足m时补0 
        ''' '+------------------------------------------------------------------------------------------------------------------------+
        ''' '  注：%后面的m(位数控制)、0（位数不足补0）对于其他格式符也适用。 
        ''' </remarks>
        Friend Class d : Implements STDIO__.I_FormatProvider.IFormatProvider

            ''' <summary>
            ''' %md
            ''' </summary>
            ''' <remarks></remarks>
            Dim _md As Regex = New Regex("%\d+d")
            ''' <summary>
            ''' %mld
            ''' </summary>
            ''' <remarks></remarks>
            Dim _mld As Regex = New Regex("%\d+ld")
            ''' <summary>
            ''' %0md
            ''' </summary>
            ''' <remarks></remarks>
            Dim _0md As Regex = New Regex("%0\d+d")
            ''' <summary>
            ''' %0mld
            ''' </summary>
            ''' <remarks></remarks>
            Dim _0mld As Regex = New Regex("%0\d+ld")

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="_format"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function FormatString(_format As String, value As String) As String Implements IFormatProvider.ActionFormat
                '当输入的是单个的字符的时候，输出其ASCW值
                If value.Length = 1 And Not IsNumeric(value) Then Return FormatString(_format, AscW(value).ToString)
                'Else
                '处理时数字的情况
                value = CType(Val(value), Long).ToString   '将输入的值转化为整形数

                '%0md or %0mld
                If _0mld.Match(_format).Success Or _0md.Match(_format).Success Then Return _0mldf(CInt(Val(Regex.Match(_format, "\d+").Value)), value)
                '%md or %mld
                If _md.Match(_format).Success Or _mld.Match(_format).Success Then Return _mldf(CInt(Val(Regex.Match(_format, "\d+").Value)), value)

                '%d or %ld
                Return value
            End Function

#Region "格式输出函数"

            ''' <summary>
            ''' %md or %mld
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function _mldf(m As Integer, value As String) As String
                If value.Length < m Then
                    Return Space(m - value.Length) & value
                Else
                    Return value
                End If
            End Function

            ''' <summary>
            ''' %0md or %0mld
            ''' </summary>
            ''' <param name="m"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function _0mldf(m As Integer, value As String) As String
                If value.Length < m Then
                    Return Zero(m - value.Length) & value
                Else
                    Return value
                End If
            End Function
#End Region

            Const ____ZERO As String =
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000"

            ''' <summary>
            ''' 获得含有指定数目的字符0的字符串
            ''' </summary>
            ''' <param name="m"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function Zero(m As Integer) As String
                Return ____ZERO.Substring(0, m)
            End Function

            Public Shared Function ZeroFill(n As Integer, len As Integer) As String
                Dim sn As String = CStr(n)
                If sn.Length >= len Then
                    Return sn
                Else
                    Dim d As Integer = len - sn.Length
                    Dim zr As String = Zero(d)
                    Return zr & sn
                End If
            End Function
        End Class
#End Region
    End Module
End Namespace