d <- read.csv("./data.csv");

head(d);

# 原始数据标准化
sd <- scale(d);
head(sd);

d <- sd;
pca <- princomp(d, cor=T);
screeplot(pca, type="line", main="碎石图", lwd=2);

# 求相关矩阵
dcor <- cor(d);
dcor;

# 求相关矩阵的特征向量 特征值
deig <- eigen(dcor);
deig;

# 输出特征值
deig$values;
sumeigv <- sum(deig$values)
sumeigv;

# 求前2个主成分的累积方差贡献率
sum(deig$value[1:2])/4
sum(deig$value[1:1])/4

# 输出前两个主成分的载荷系数（特征向量）
pca$loadings[,1:2]

# 计算主成分C1和C2的系数b1 和b2：
deig$values[1]/4;
deig$values[2]/4;

# 输出前2 个主成分的得分
s <- pca$scores[,1:2]