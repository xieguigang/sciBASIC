Namespace Text

    ''' <summary>
    ''' ``ASCII`` (``American Standard Code for Information Interchange``，美国信息互换标准代码，``ASCⅡ``) 是基于拉丁字母的一套电脑编码系统。
    ''' 它主要用于显示现代英语和其他西欧语言。它是现今最通用的单字节编码系统，并等同于国际标准``ISO/IEC 646``。
    ''' ASCII第一次以规范标准的型态发表是在1967年，最后一次更新则是在1986年，至今为止共定义了128个字符，其中33个字符无法显示
    ''' （这是以现今操作系统为依归，但在DOS模式下可显示出一些诸如笑脸、扑克牌花式等8-bit符号），且这33个字符多数都已是陈废的控制字符，
    ''' 控制字符的用途主要是用来操控已经处理过的文字，在33个字符之外的是95个可显示的字符，包含用键盘敲下空白键所产生的空白字符也算1个可显示字符
    ''' （显示为空白）。
    ''' </summary>
    ''' <remarks>http://ascii.911cha.com/</remarks>
    Public Class ASCII

        ''' <summary>
        ''' 0000 0000	0	00	NUL	空字符（Null）
        ''' </summary>
        Public Const NUL As Char = Chr(0)
        ''' <summary>
        ''' 0000 0001	1	01	SOH	标题开始
        ''' </summary>
        Public Const SOH As Char = Chr(1)
        ''' <summary>
        ''' 0000 0010	2	02	STX	本文开始
        ''' </summary>
        Public Const STX As Char = Chr(2)
        ''' <summary>
        ''' 0000 0011	3	03	ETX	本文结束
        ''' </summary>
        Public Const ETX As Char = Chr(3)
        ''' <summary>
        ''' 0000 0100	4	04	EOT	传输结束
        ''' </summary>
        Public Const EOT As Char = Chr(4)
        ''' <summary>
        ''' 0000 0101	5	05	ENQ	请求
        ''' </summary>
        Public Const ENQ As Char = Chr(5)
        ''' <summary>
        ''' 0000 0110	6	06	ACK	确认回应
        ''' </summary>
        Public Const ACK As Char = Chr(6)
        ''' <summary>
        ''' 0000 0111	7	07	BEL	响铃
        ''' </summary>
        Public Const BEL As Char = Chr(7)
        ''' <summary>
        ''' 0000 1000	8	08	BS	退格
        ''' </summary>
        Public Const BS As Char = Chr(8)
        ''' <summary>
        ''' 0000 1001	9	09	HT	水平定位符号
        ''' </summary>
        Public Const HT As Char = Chr(9)
        ''' <summary>
        ''' 0000 1010	10	0A	LF	换行键
        ''' </summary>
        Public Const LF As Char = Chr(10)
        ''' <summary>
        ''' 0000 1011	11	0B	VT	垂直定位符号
        ''' </summary>
        Public Const VT As Char = Chr(11)
        ''' <summary>
        ''' 0000 1100	12	0C	FF	换页键
        ''' </summary>
        Public Const FF As Char = Chr(12)
        ''' <summary>
        ''' 0000 1101	13	0D	CR	归位键
        ''' </summary>
        Public Const CR As Char = Chr(13)
        ''' <summary>
        ''' 0000 1110	14	0E	SO	取消变换（Shift out）
        ''' </summary>
        Public Const SO As Char = Chr(14)
        ''' <summary>
        ''' 0000 1111	15	0F	SI	启用变换（Shift in）
        ''' </summary>
        Public Const SI As Char = Chr(15)
        ''' <summary>
        ''' 0001 0000	16	10	DLE	跳出数据通讯
        ''' </summary>
        Public Const DLE As Char = Chr(16)
        ''' <summary>
        ''' 0001 0001	17	11	DC1	设备控制一（XON 启用软件速度控制）
        ''' </summary>
        Public Const DC1 As Char = Chr(17)
        ''' <summary>
        ''' 0001 0010	18	12	DC2	设备控制二
        ''' </summary>
        Public Const DC2 As Char = Chr(18)
        ''' <summary>
        ''' 0001 0011	19	13	DC3	设备控制三（XOFF 停用软件速度控制）
        ''' </summary>
        Public Const DC3 As Char = Chr(19)
        ''' <summary>
        ''' 0001 0100	20	14	DC4	设备控制四
        ''' </summary>
        Public Const DC4 As Char = Chr(20)
        ''' <summary>
        ''' 0001 0101	21	15	NAK	确认失败回应
        ''' </summary>
        Public Const NAK As Char = Chr(21)
        ''' <summary>
        ''' 0001 0110	22	16	SYN	同步用暂停
        ''' </summary>
        Public Const SYN As Char = Chr(22)
        ''' <summary>
        ''' 0001 0111	23	17	ETB	区块传输结束
        ''' </summary>
        Public Const ETB As Char = Chr(23)
        ''' <summary>
        ''' 0001 1000	24	18	CAN	取消
        ''' </summary>
        Public Const CAN As Char = Chr(24)
        ''' <summary>
        ''' 0001 1001	25	19	EM	连接介质中断
        ''' </summary>
        Public Const EM As Char = Chr(25)
        ''' <summary>
        ''' 0001 1010	26	1A	SUB	替换
        ''' </summary>
        Public Const [SUB] As Char = Chr(26)
        ''' <summary>
        ''' 0001 1011	27	1B	ESC	跳出
        ''' </summary>
        Public Const ESC As Char = Chr(27)
        ''' <summary>
        ''' 0001 1100	28	1C	FS	文件分割符
        ''' </summary>
        Public Const FS As Char = Chr(28)
        ''' <summary>
        ''' 0001 1101	29	1D	GS	组群分隔符
        ''' </summary>
        Public Const GS As Char = Chr(29)
        ''' <summary>
        ''' 0001 1110	30	1E	RS	记录分隔符
        ''' </summary>
        Public Const RS As Char = Chr(30)
        ''' <summary>
        ''' 0001 1111	31	1F	US	单元分隔符
        ''' </summary>
        Public Const US As Char = Chr(31)
        ''' <summary>
        ''' 0111 1111	127	7F	DEL	删除
        ''' </summary>
        Public Const DEL As Char = Chr(127)
    End Class
End Namespace