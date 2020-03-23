let LAYER: i32 =  3;        //三层神经网络
let NUM  : i32 =     10;       //每层的最多节点数
let A  : f64 =       30.0;
let B  : f64 =       10.0 ;    //A和B是S型函数的参数
let ITERS : i32 =   1000 ;    //最大训练次数
let ETA_W  : f64=    0.0035;   //权值调整率
let ETA_B  : f64 =   0.001 ;   //阀值调整率
let ERROR  : f64=    0.002 ;   //单个样本允许的误差
let ACCU  : f64 =    0.005 ;   //每次迭代允许的误差

pub struct Data {
    x: Vec<f64>,       //输入数据
    y: Vec<f64>       //输出数据
};


trait BP {
    fn GetData(&self, data: Vec<Data>);
    fn Train(&self);
    fn ForeCast(&self, data: Vec<f64>) -> Vec<f64>;

    fn InitNetWork(&self);         //初始化网络
    fn GetNums(&self);             //获取输入、输出和隐含层节点数
    fn ForwardTransfer(&self);     //正向传播子过程
    fn ReverseTransfer(&self, i : i32);  //逆向传播子过程
    fn CalcDelta(&self, i :i32);        //计算w和b的调整量
    fn UpdateNetWork(&self);       //更新权值和阀值
    fn GetError(&self, i: i32) -> f64;         //计算单个样本的误差
    fn GetAccu(&self) -> f64;             //计算所有样本的精度
    fn Sigmoid(&self, x : f64) -> f64;   //计算Sigmoid的值

}

pub struct BPNetwork {
    in_num : i32;                 //输入层节点数
    ou_num: i32;                 //输出层节点数
    hd_num: i32;                 //隐含层节点数
 
    data: Vec<f64>;          //输入输出数据
 
    w[LAYER][NUM][NUM] : f64;    //BP网络的权值
    b[LAYER][NUM]: f64;         //BP网络节点的阀值
     
    x[LAYER][NUM] : f64;         //每个神经元的值经S型函数转化后的输出值，输入层就为原值
    d[LAYER][NUM]: f64;         //记录delta学习规则中delta的值
}

impl BPNetwork {
    fn new(layers: i32, maxNode: i32) -> BPNetwork {
        BPNetwork {
            in_num: 0,
            ou_num: 0,
            hd_num: 0,

            
        };
    }
}

impl BP for BPNetwork {

    fn GetData(&self, data: Vec<Data>) {
        self.data = data;
    };
}