# GLMNet 5.1 复刻的黄金数据导出脚本。
# 用法: Rscript export_golden.R <output-directory>
# 只写指定输出目录下的 CSV，不安装软件包、不修改 R 库。
suppressPackageStartupMessages(library(glmnet))

args <- commandArgs(trailingOnly = TRUE)
if (length(args) < 1) {
    stop("usage: Rscript export_golden.R <output-directory>")
}
out <- args[1]
dir.create(out, showWarnings = FALSE, recursive = TRUE)

v <- as.character(utils::packageVersion("glmnet"))
if (!identical(substr(v, 1, 3), "5.1")) {
    stop("glmnet 5.1 is required for golden data generation")
}

write_matrix <- function(path, m) {
    utils::write.table(as.matrix(m), file = file.path(out, path),
                       sep = ",", row.names = FALSE, col.names = FALSE,
                       na = "0", quote = FALSE)
}

write_fit <- function(prefix, fit, family) {
    lam <- fit$lambda
    L <- length(lam)
    a0 <- rep(0, L)

    if (family %in% c("gaussian", "binomial", "poisson")) {
        a0 <- as.vector(fit$a0)
        beta <- t(as.matrix(fit$beta))
    } else if (family == "cox") {
        beta <- t(as.matrix(fit$beta))
    } else {
        beta <- t(as.matrix(fit$beta[[1]]))
    }

    nzero <- apply(abs(beta) > 1e-12, 1, sum)
    fit_tbl <- data.frame(step = seq_len(L) - 1, lambda = lam, a0 = a0,
                          df = as.vector(fit$df), dev.ratio = as.vector(fit$dev.ratio),
                          nzero = as.vector(nzero), nulldev = rep(fit$nulldev, L))
    utils::write.table(fit_tbl, file = file.path(out, paste0(prefix, "_fit.csv")),
                       sep = ",", row.names = FALSE, quote = FALSE)
    utils::write.table(cbind(data.frame(step = seq_len(L) - 1), beta),
                       file = file.path(out, paste0(prefix, "_beta.csv")),
                       sep = ",", row.names = FALSE, quote = FALSE)
}

write_cv <- function(prefix, cv) {
    L <- length(cv$lambda)
    tbl <- data.frame(step = seq_len(L) - 1, lambda = as.vector(cv$lambda),
                      cvm = as.vector(cv$cvm), cvsd = as.vector(cv$cvsd),
                      lambda.min = rep(cv$lambda.min, L),
                      lambda.1se = rep(cv$lambda.1se, L))
    utils::write.table(tbl, file = file.path(out, paste0(prefix, "_cv.csv")),
                       sep = ",", row.names = FALSE, quote = FALSE)
}

set.seed(1)

# ---------- gaussian ----------
data(QuickStartExample)
x <- as.matrix(QuickStartExample$x)
y <- as.vector(QuickStartExample$y)
n <- nrow(x)
foldid <- sample(rep(seq_len(5), length.out = n))
write_matrix("gaussian_x.csv", x)
write_matrix("gaussian_y.csv", matrix(y, ncol = 1))
write_matrix("gaussian_foldid.csv", matrix(foldid, ncol = 1))
fit <- glmnet(x, y, family = "gaussian")
write_fit("gaussian", fit, "gaussian")
cv <- cv.glmnet(x, y, family = "gaussian", type.measure = "mse", foldid = foldid)
write_cv("gaussian", cv)

# ---------- binomial ----------
data(BinomialExample)
x <- as.matrix(BinomialExample$x)
y <- as.vector(BinomialExample$y)
n <- nrow(x)
foldid <- sample(rep(seq_len(5), length.out = n))
write_matrix("binomial_x.csv", x)
write_matrix("binomial_y.csv", matrix(y, ncol = 1))
write_matrix("binomial_foldid.csv", matrix(foldid, ncol = 1))
fit <- glmnet(x, y, family = "binomial")
write_fit("binomial", fit, "binomial")
cv <- cv.glmnet(x, y, family = "binomial", type.measure = "deviance", foldid = foldid)
write_cv("binomial", cv)

# ---------- poisson ----------
data(PoissonExample)
x <- as.matrix(PoissonExample$x)
y <- as.vector(PoissonExample$y)
n <- nrow(x)
foldid <- sample(rep(seq_len(5), length.out = n))
write_matrix("poisson_x.csv", x)
write_matrix("poisson_y.csv", matrix(y, ncol = 1))
write_matrix("poisson_foldid.csv", matrix(foldid, ncol = 1))
fit <- glmnet(x, y, family = "poisson")
write_fit("poisson", fit, "poisson")
cv <- cv.glmnet(x, y, family = "poisson", type.measure = "deviance", foldid = foldid)
write_cv("poisson", cv)

# ---------- multinomial ----------
data(MultinomialExample)
x <- as.matrix(MultinomialExample$x)
y <- as.integer(MultinomialExample$y)
n <- nrow(x)
foldid <- sample(rep(seq_len(5), length.out = n))
write_matrix("multinomial_x.csv", x)
write_matrix("multinomial_y.csv", matrix(y, ncol = 1))
write_matrix("multinomial_foldid.csv", matrix(foldid, ncol = 1))
fit <- glmnet(x, y, family = "multinomial")
write_fit("multinomial", fit, "multinomial")
cv <- tryCatch(
    cv.glmnet(x, y, family = "multinomial", type.measure = "deviance", foldid = foldid),
    error = function(e) cv.glmnet(x, y, family = "multinomial", type.measure = "mse", foldid = foldid))
write_cv("multinomial", cv)

# ---------- cox ----------
data(CoxExample)
x <- as.matrix(CoxExample$x)
ys <- CoxExample$y
y <- cbind(time = as.vector(ys[, 1]), status = as.vector(ys[, 2]))
n <- nrow(x)
foldid <- sample(rep(seq_len(5), length.out = n))
write_matrix("cox_x.csv", x)
write_matrix("cox_y.csv", y)
write_matrix("cox_foldid.csv", matrix(foldid, ncol = 1))
fit <- glmnet(x, y, family = "cox")
write_fit("cox", fit, "cox")
cv <- cv.glmnet(x, y, family = "cox", type.measure = "deviance", foldid = foldid)
write_cv("cox", cv)

# ---------- mgaussian ----------
data(MultiGaussianExample)
x <- as.matrix(MultiGaussianExample$x)
y <- as.matrix(MultiGaussianExample$y)
n <- nrow(x)
foldid <- sample(rep(seq_len(5), length.out = n))
write_matrix("mgaussian_x.csv", x)
write_matrix("mgaussian_y.csv", y)
write_matrix("mgaussian_foldid.csv", matrix(foldid, ncol = 1))
fit <- glmnet(x, y, family = "mgaussian")
write_fit("mgaussian", fit, "mgaussian")
cv <- cv.glmnet(x, y, family = "mgaussian", type.measure = "mse", foldid = foldid)
write_cv("mgaussian", cv)

cat(sprintf("OK: golden data exported to %s (glmnet %s)\n", normalizePath(out), v))
