Public Class VECTOR

    Public [dim] As Integer '  //数组维数
    ''' <summary>
    '''         //用一维数组构造向量
    ''' </summary>
    ''' <remarks></remarks>
    Public ele As Double()

    Public Sub New(m As Integer)
        Me.dim = m
        ele = New Double(m)
    End Sub

    ''' <summary>
    ''' 
    ''' 
    '''  //-------两个向量加法算符重载
    '''   //------分量分别相加
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Operator +(v1 As VECTOR, v2 As VECTOR) As VECTOR

        Dim N0 As Integer = v1.dim

        Dim v3 As VECTOR = New VECTOR(N0)

        For j As Integer = 0 To N0
            v3(j) = v1(j) + v2(j)
        Next
        Return v3
    End Operator

    //-----------------向量减法算符重载
    //-----------------分量分别想减
    public static VEC operator -(VEC v1, VEC v2)
    {
        int N0;
        //获取变量维数
        N0 = v1.dim;

        VEC v3 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v3.ele[j] = v1.ele[j] - v2.ele[j];
        }
        return v3;
    }

    //----------------向量乘法算符重载
    //-------分量分别相乘，相当于MATLAB中的  .*算符
    public static VEC operator *(VEC v1, VEC v2)
    {

        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v3 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v3.ele[j] = v1.ele[j] * v2.ele[j];
        }
        return v3;
    }


    //---------------向量除法算符重载
    //--------------分量分别相除，相当于MATLAB中的   ./算符
    public static VEC operator /(VEC v1, VEC v2)
    {
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v3 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v3.ele[j] = v1.ele[j] / v2.ele[j];
        }
        return v3;
    }


    //向量减加实数
    //各分量分别加实数
    public static VEC operator +(VEC v1, double a)
    {
        //向量数加算符重载
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v2 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v2.ele[j] = v1.ele[j] + a;
        }
        return v2;
    }

    //向量减实数
    //各分量分别减实数
    public static VEC operator -(VEC v1, double a)
    {
        //向量数加算符重载
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v2 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v2.ele[j] = v1.ele[j] - a;
        }
        return v2;
    }


    //向量 数乘
    //各分量分别乘以实数
    public static VEC operator *(VEC v1, double a)
    {
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v2 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v2.ele[j] = v1.ele[j] * a;
        }
        return v2;
    }


    //向量 数除
    //各分量分别除以实数
    public static VEC operator /(VEC v1, double a)
    {
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v2 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v2.ele[j] = v1.ele[j] / a;
        }
        return v2;
    }


    //实数加向量
    public static VEC operator +(double a, VEC v1)
    {
        //向量数加算符重载
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v2 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v2.ele[j] = v1.ele[j] + a;
        }
        return v2;
    }

    //实数减向量
    public static VEC operator -(double a, VEC v1)
    {
        //向量数加算符重载
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v2 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v2.ele[j] = v1.ele[j] - a;
        }
        return v2;
    }


    //向量 数乘
    public static VEC operator *(double a, VEC v1)
    {
        int N0;

        //获取变量维数
        N0 = v1.dim;

        VEC v2 = new VEC(N0);

        int j;
        for (j = 0; j < N0; j++)
        {
            v2.ele[j] = v1.ele[j] * a;
        }
        return v2;
    }



    //---------------向量内积
    public static double operator |(VEC v1, VEC v2)
    {
        int N0, M0;

        //获取变量维数
        N0 = v1.dim;

        M0 = v2.dim;

        if (N0 != M0)
            System.Console.WriteLine("Inner vector dimensions must agree！");
        //如果向量维数不匹配，给出告警信息

        double sum;
        sum = 0.0;

        int j;
        for (j = 0; j < N0; j++)
        {
            sum = sum + v1.ele[j] * v2.ele[j];
        }
        return sum;
    }


    //-----------向量外积（相当于列向量，乘以横向量）
    public static MAT operator ^(VEC v1, VEC v2)
    {
        int N0, M0;

        //获取变量维数
        N0 = v1.dim;

        M0 = v2.dim;

        if (N0 != M0)
            System.Console.WriteLine("Inner vector dimensions must agree！");
        //如果向量维数不匹配，给出告警信息

        MAT vvmat = new MAT(N0, N0);

        for (int i = 0; i < N0; i++)
            for (int j = 0; j < N0; j++)
                vvmat[i, j] = v1[i] * v2[j];

        //返回外积矩阵
        return vvmat;

    }


    //---------------向量模的平方
    public static double operator ~(VEC v1)
    {
        int N0;

        //获取变量维数
        N0 = v1.dim;

        double sum;
        sum = 0.0;

        int j;
        for (j = 0; j < N0; j++)
        {
            sum = sum + v1.ele[j] * v1.ele[j];
        }
        return sum;
    }

    //  负向量 
    public static VEC operator -(VEC v1)
    {
        int N0 = v1.dim;

        VEC v2=new VEC(N0);

        for (int i = 0; i < N0; i++)
            v2.ele[i] = -v1.ele[i];

        return v2;
    }

    public double this[int index]
    /*------------------------------------------ comment
Author:                                                                         syz()
    Date        : 2011-07-03 19:53:28         
    ---------------------------------------------------
Desciption:                                                                     创建索引器()
     *
                                                                                Post Script
     *
paramtegers:
     *
    -------------------------------------------------*/
    {
        get
        {
            //get accessor
            return ele[index];

        }
        set
        {
            //set accessor
            ele[index] = value;

        }

    }
}
End Class
