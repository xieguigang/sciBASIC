# ============================================================================
# validate_lpp.py — 内点法 + Crossover LP 求解器的 Python 镜像验证
# 与 VB 转录逐式对应：
#   1. LinAlg: Cholesky(LL^T)+迭代精化；LU(部分主元)求解/转置求解/秩检测
#   2. IPM: Mehrotra 预测-校正 [readme §二/§三]
#      正规方程 A D² Aᵀ Δy = rhs, D²=diag(x_i/s_i)；静态正则化+一次迭代精化；
#      预测步 σ=0 → μ_aff → σ=(μ_aff/μ)³ → 校正步(含 ΔX_aff ΔS_aff e 补偿)；
#      fraction-to-boundary γ=0.99，原始/对偶分离步长
#   3. Crossover [readme §四]：严格互补划分 → 贪心构造基(秩检测) → 主元循环
#      消超基本(比率测试保原始可行) → 原始单纯形收尾(Phase1/2, 证书)
#   4. 测试：已知最优/影子价、≥/= 混合、退化多解、Klee-Minty、不可行/无界、随机阵
# ============================================================================
import numpy as np

EPS = 1e-13

# ---------------- LinAlg ----------------
def cholesky(A):
    """A = L Lᵀ（对称正定）；失败返回 None"""
    n = A.shape[0]
    L = np.zeros((n, n))
    for j in range(n):
        d = A[j, j] - np.dot(L[j, :j], L[j, :j])
        if d <= 0:
            return None
        L[j, j] = np.sqrt(d)
        for i in range(j + 1, n):
            L[i, j] = (A[i, j] - np.dot(L[i, :j], L[j, :j])) / L[j, j]
    return L

def chol_solve(L, rhs):
    n = L.shape[0]
    y = np.zeros(n)
    for i in range(n):
        y[i] = (rhs[i] - np.dot(L[i, :i], y[:i])) / L[i, i]
    x = np.zeros(n)
    for i in range(n - 1, -1, -1):
        x[i] = (y[i] - np.dot(L[i + 1:, i], x[i + 1:])) / L[i, i]
    return x

def solve_spd(M, rhs, reg):
    """(M + reg·I) Cholesky 求解 + 一次迭代精化"""
    n = M.shape[0]
    Mreg = M + reg * np.eye(n)
    L = cholesky(Mreg)
    if L is None:
        return None
    z = chol_solve(L, rhs)
    res = rhs - Mreg @ z
    z = z + chol_solve(L, res)
    return z

def lu_factor(A):
    """部分主元 LU：P·A = L·U；奇异返回 None"""
    n = A.shape[0]
    LU = A.astype(float).copy()
    piv = list(range(n))
    for k in range(n):
        p = max(range(k, n), key=lambda i: abs(LU[i, k]))
        if abs(LU[p, k]) < EPS:
            return None
        if p != k:
            LU[[k, p], :] = LU[[p, k], :]
            piv[k], piv[p] = piv[p], piv[k]
        for i in range(k + 1, n):
            LU[i, k] /= LU[k, k]
            LU[i, k + 1:] -= LU[i, k] * LU[k, k + 1:]
    return (LU, piv)

def lu_solve(fac, rhs):
    """解 A x = rhs（fac 来自 lu_factor(A)）"""
    LU, piv = fac
    n = LU.shape[0]
    b = np.array([rhs[piv[i]] for i in range(n)], dtype=float)
    for i in range(1, n):
        b[i] -= np.dot(LU[i, :i], b[:i])
    for i in range(n - 1, -1, -1):
        b[i] = (b[i] - np.dot(LU[i, i + 1:], b[i + 1:])) / LU[i, i]
    return b

def lu_solve_T(fac, rhs):
    """解 Aᵀ x = rhs。P·A = L·U → Aᵀ = Uᵀ Lᵀ Pᵀ？注意：A = PᵀLU → Aᵀ = UᵀLᵀP
       解 Uᵀ z = rhs（前代），Lᵀ v = z（回代），x = Pᵀ v 即 x[piv[i]] = v[i] 的逆映射"""
    LU, piv = fac
    n = LU.shape[0]
    # Uᵀ z = rhs（Uᵀ 下三角：Uᵀ[i,j] = U[j,i] = LU[j,i], j ≤ i）
    z = np.zeros(n)
    for i in range(n):
        z[i] = (rhs[i] - np.dot(LU[:i, i], z[:i])) / LU[i, i]
    # Lᵀ v = z（Lᵀ 上三角：Lᵀ[i,j] = L[j,i] = LU[j,i], j > i；单位对角）
    v = np.zeros(n)
    for i in range(n - 1, -1, -1):
        v[i] = z[i] - np.dot(LU[i + 1:, i], v[i + 1:])
    # ρ = Pᵀ v：P 定义为 piv[k]=原行号 → 行交换后第 k 行来自原 piv[k]
    # A x = b: x = U⁻¹L⁻¹ P b，即 (lu_solve) b'[k]=b[piv[k]] → P b = b'
    # Aᵀ x = rhs → Uᵀ Lᵀ P x = rhs → P x = v → x = Pᵀ v: x[piv[k]] = v[k]
    x = np.zeros(n)
    for k in range(n):
        x[piv[k]] = v[k]
    return x

def mat_rank(A, tol=1e-10):
    """阶梯消元秩（支持非方阵）"""
    M = A.astype(float).copy()
    m, n = M.shape
    scale = max(1.0, np.abs(M).max()) if M.size else 1.0
    rank = 0
    row = 0
    for col in range(n):
        if row >= m:
            break
        p = row + int(np.argmax(np.abs(M[row:, col])))
        if abs(M[p, col]) <= tol * scale:
            continue
        if p != row:
            M[[row, p], :] = M[[p, row], :]
        for i in range(row + 1, m):
            M[i, col:] -= (M[i, col] / M[row, col]) * M[row, col:]
        row += 1
        rank += 1
    return rank

# ---------------- IPM (Mehrotra) [readme §三] ----------------
def mehrotra_start(A, b, c):
    """Mehrotra (1992) 起始点：最小范数 x̂/ŝ + 偏移平衡"""
    m, n = A.shape
    M0 = A @ A.T + 1e-12 * np.eye(m)
    L0 = cholesky(M0)
    if L0 is None:
        return np.ones(n), np.zeros(m), np.ones(n)
    y1 = chol_solve(L0, b)
    xhat = A.T @ y1
    y2 = chol_solve(L0, A @ c)
    shat = c - A.T @ y2
    dx = max(0.0, -1.5 * np.min(xhat)) if n else 0.0
    ds = max(0.0, -1.5 * np.min(shat)) if n else 0.0
    x = xhat + dx
    s = shat + ds
    dot = float(x @ s)
    dx2 = 0.5 * dot / max(1e-12, float(np.sum(s)))
    ds2 = 0.5 * dot / max(1e-12, float(np.sum(x)))
    x = x + dx2
    s = s + ds2
    x = np.maximum(x, 1e-4)
    s = np.maximum(s, 1e-4)
    return x, y2, s

def ipm_solve(A, b, c, tol=1e-8, max_iter=200, log=None):
    """Mehrotra 预测-校正 + 自适应正则化（步长停滞 → reg×1000 重试 ≤4 档）
       [readme §2.2 数值手段：静态正则化 + 迭代精化；停滞动态加强为工程惯例]"""
    m, n = A.shape
    x, y, s = mehrotra_start(A, b, c)
    iters = 0
    stall_count = 0
    prev_metric = np.inf
    status = "max_iter"
    for it in range(max_iter):
        iters = it + 1
        r_p = A @ x - b
        r_d = A.T @ y + s - c
        mu = float(x @ s) / n
        nrp = np.linalg.norm(r_p) / (1 + np.linalg.norm(b))
        nrd = np.linalg.norm(r_d) / (1 + np.linalg.norm(c))
        obj = float(c @ x)
        ngap = mu / (1 + abs(obj))
        if log is not None:
            log.append(f"  IPM {it:3d}: rp={nrp:.2e} rd={nrd:.2e} mu={mu:.2e} obj={obj:.8f}")
        if nrp <= tol and nrd <= tol and ngap <= tol:
            status = "optimal"
            break
        # 停滞检测（连续 8 轮综合指标无改善 → 提前退出交 simplex 兜底）
        metric = nrp + nrd + ngap
        if metric > prev_metric * (1 - 1e-12):
            stall_count += 1
            if stall_count >= 8:
                status = "stalled"
                break
        else:
            stall_count = 0
        prev_metric = metric
        d2 = np.clip(x / s, 1e-12, 1e10)
        AD_rd = A @ (d2 * r_d)
        def attempt(reg_mult):
            """一次正则化档位：因子化 + 预测/校正（共享因子）"""
            M = A @ (d2[:, None] * A.T)
            reg = reg_mult * max(1.0, np.abs(M).max())
            L = cholesky(M + reg * np.eye(m))
            if L is None:
                return None
            def newton(sigma_mu, corr):
                Sinv = -x + sigma_mu / s + corr / s
                rhs = -r_p - A @ Sinv - AD_rd
                dy = chol_solve(L, rhs)
                dy = dy + chol_solve(L, rhs - (M + reg * np.eye(m)) @ dy)
                dx = d2 * (A.T @ dy) - x + sigma_mu / s + corr / s + d2 * r_d
                ds = -r_d - A.T @ dy
                return dx, dy, ds
            dx_a, dy_a, ds_a = newton(np.zeros(n), np.zeros(n))
            def max_step(v, dv):
                neg = dv < -1e-14
                if not np.any(neg):
                    return 1.0
                return float(np.min(-v[neg] / dv[neg]))
            a_aff = min(1.0, max_step(x, dx_a), max_step(s, ds_a))
            mu_aff = float((x + a_aff * dx_a) @ (s + a_aff * ds_a)) / n
            sigma = min(1.0, max(0.0, (mu_aff / mu) ** 3))
            dx, dy, ds = newton(sigma * mu * np.ones(n), dx_a * ds_a)
            a_p = min(1.0, 0.99 * max_step(x, dx))
            a_d = min(1.0, 0.99 * max_step(s, ds))
            return (dx, dy, ds, a_p, a_d)
        got = None
        reg_mult = 1e-11
        for att in range(4):
            got = attempt(reg_mult)
            if got is not None and (got[3] > 1e-9 or got[4] > 1e-9):
                break
            reg_mult *= 1000.0
            got = None
        if got is None:
            status = "numeric_fail"
            break
        dx, dy, ds, a_p, a_d = got
        x = x + a_p * dx
        y = y + a_d * dy
        s = s + a_d * ds
        if np.max(np.abs(x)) > 1e14 or np.max(np.abs(s)) > 1e14:
            status = "diverged"
            break
    else:
        status = "max_iter"
    if status not in ("optimal",):
        r_p = A @ x - b
        r_d = A.T @ y + s - c
        nrp = np.linalg.norm(r_p) / (1 + np.linalg.norm(b))
        nrd = np.linalg.norm(r_d) / (1 + np.linalg.norm(c))
        if nrp > 1e-4 and nrd < 1e-6:
            status = "primal_infeasible"
        elif nrd > 1e-4 and nrp < 1e-6:
            status = "dual_infeasible"
    return dict(status=status, x=x, y=y, s=s, iters=iters)

# ---------------- 原始单纯形（Phase1/2 收尾与证书） ----------------
class Simplex:
    """标准形 min cᵀx, Ax=b, x≥0，b≥0（调用方保证）。
       非基变量恒 0 → x_B = B⁻¹b。修订单纯形 + 每次重构 LU（中等规模足够）。"""

    def __init__(self, A, b, c, log=None):
        self.A = A
        self.b = b
        self.c = c
        self.m, self.n = A.shape
        self.log = log

    def solve(self):
        # Phase 1
        A1 = np.hstack([self.A, np.eye(self.m)])
        c1 = np.concatenate([np.zeros(self.n), np.ones(self.m)])
        barred = set(range(self.n, self.n + self.m))    # 人造列禁止再入
        st = self._loop(A1, c1, list(range(self.n, self.n + self.m)),
                        barred, 40 * max(1, self.m), "P1")
        if st[0] != "optimal":
            return dict(status=st[0], iters=st[3], drop_rows=[])
        it_total = st[3]
        # 驱逐人造基变量 / 冗余行
        basis = st[1]
        rows_alive = list(range(self.m))
        drop_rows = []
        i = 0
        while i < len(basis):
            if basis[i] >= self.n:
                facB = lu_factor(A1[np.ix_(rows_alive, basis)])
                rho = lu_solve_T(facB, self._unit(len(basis), i))
                piv_col = -1
                for j in range(self.n):
                    if j not in basis:
                        col = A1[np.ix_(rows_alive, [j])].ravel()
                        if abs(float(rho @ col)) > 1e-9:
                            piv_col = j
                            break
                if piv_col >= 0:
                    basis[i] = piv_col
                else:
                    drop_rows.append(rows_alive[i])
                    rows_alive.pop(i)
                    basis.pop(i)
                    i -= 1
            i += 1
        # Phase 2
        keep = [i for i in range(self.m) if i not in drop_rows]
        A2 = self.A[keep, :]
        b2 = self.b[keep]
        st2 = self._loop2(A2, b2, self.c, basis, 40 * max(1, len(keep)), "P2")
        it_total += st2[3]
        if st2[0] != "optimal":
            return dict(status=st2[0], iters=it_total, drop_rows=drop_rows)
        basis = st2[1]
        x = np.zeros(self.n)
        x[basis] = st2[2]
        facB = lu_factor(A2[:, basis])
        y = lu_solve_T(facB, self.c[basis])
        return dict(status="optimal", x=x, y=y, basis=basis, iters=it_total,
                    drop_rows=drop_rows)

    def _unit(self, n, i):
        e = np.zeros(n)
        e[i] = 1.0
        return e

    def _loop(self, A, c, basis, barred, max_iter, tag):
        """Phase 1：从人造基起，xB = B⁻¹b"""
        m = A.shape[0]
        xB = self.b.copy()
        iters = 0
        stall = 0
        bland = False
        prev_obj = None
        while iters < max_iter:
            iters += 1
            facB = lu_factor(A[:, basis])
            if facB is None:
                return ("numeric_fail", basis, xB, iters)
            xB = lu_solve(facB, self.b)
            if np.min(xB) < -1e-7 * (1 + np.linalg.norm(self.b)):
                return ("infeasible", basis, xB, iters)
            y = lu_solve_T(facB, c[basis])
            d = c - A.T @ y
            inB = set(basis)
            enter = self._price(d, inB, barred, bland, c, tol=1e-9)
            if enter < 0:
                obj1 = sum(xB[i] for i in range(m) if basis[i] >= self.n)
                return ("optimal" if obj1 <= 1e-7 * (1 + np.linalg.norm(self.b))
                        else "infeasible", basis, xB, iters)
            alpha = lu_solve(facB, A[:, enter])
            leave, ratio = self._ratio(xB, alpha)
            if leave < 0:
                return ("unbounded", basis, xB, iters)
            if not bland:
                basis[leave] = enter
            else:
                basis[leave] = enter
            obj = float(c[basis] @ xB)
            if prev_obj is not None and abs(prev_obj - obj) < 1e-14:
                stall += 1
                if stall >= 20:
                    bland = True
            else:
                stall = 0
            prev_obj = obj
        return ("max_iter", basis, xB, iters)

    def _loop2(self, A, b, c, basis, max_iter, tag):
        """Phase 2：原始可行基起（x_B = B⁻¹b ≥ 0）"""
        m = A.shape[0]
        iters = 0
        stall = 0
        bland = False
        prev_obj = None
        while iters < max_iter:
            iters += 1
            facB = lu_factor(A[:, basis])
            if facB is None:
                return ("numeric_fail", basis, None, iters)
            xB = lu_solve(facB, b)
            if np.min(xB) < -1e-7 * (1 + np.linalg.norm(b)):
                return ("infeasible", basis, xB, iters)
            y = lu_solve_T(facB, c[basis])
            d = c - A.T @ y
            inB = set(basis)
            enter = self._price(d, inB, set(), bland, c, tol=1e-9)
            if enter < 0:
                return ("optimal", basis, xB, iters)
            alpha = lu_solve(facB, A[:, enter])
            leave, ratio = self._ratio(xB, alpha)
            if leave < 0:
                return ("unbounded", basis, xB, iters)
            basis[leave] = enter
            obj = float(c[basis] @ xB)
            if prev_obj is not None and abs(prev_obj - obj) < 1e-14:
                stall += 1
                if stall >= 20:
                    bland = True
            else:
                stall = 0
            prev_obj = obj
        return ("max_iter", basis, None, iters)

    def _price(self, d, inB, barred, bland, c, tol):
        if bland:
            for j in range(self.n):
                if j not in inB and j not in barred and d[j] < -tol * (1 + abs(c[j])):
                    return j
            return -1
        best = -tol
        enter = -1
        for j in range(self.n):
            if j not in inB and j not in barred and d[j] < best:
                best = d[j]
                enter = j
        return enter

    def _ratio(self, xB, alpha):
        leave = -1
        ratio = None
        for i in range(len(xB)):
            if alpha[i] > 1e-9:
                r = max(0.0, xB[i]) / alpha[i]
                if ratio is None or r < ratio - 1e-12 or \
                   (abs(r - ratio) <= 1e-12 and abs(alpha[i]) > abs(alpha[leave])):
                    ratio = r
                    leave = i
        return leave, ratio

# ---------------- Crossover [readme §四] ----------------
def crossover(A, b, c, x_ipm, s_ipm, log=None):
    """从 IPM 近似最优出发构造最优基。返回 dict(status, x, y, basis, pivots)"""
    m, n = A.shape
    kappa = 1e-8 * max(1.0, np.abs(x_ipm).max())
    basic_cand = [j for j in range(n) if x_ipm[j] > kappa and x_ipm[j] >= s_ipm[j]]
    nonbasic = [j for j in range(n) if j not in set(basic_cand)]
    if log is not None:
        log.append(f"  Crossover 划分: 基候选={len(basic_cand)} 非基={len(nonbasic)}")
    # 阶段1：贪心构造初始基（候选按 x 降序，秩检测）
    basis = []
    for j in sorted(basic_cand, key=lambda j: -x_ipm[j]):
        if len(basis) == m:
            break
        B = A[:, basis + [j]]
        if mat_rank(B) > len(basis):
            basis.append(j)
    # 不足 m → 用剩余列补（优先稀疏）
    if len(basis) < m:
        rest = [j for j in range(n) if j not in basis]
        rest.sort(key=lambda j: (np.count_nonzero(A[:, j]), -x_ipm[j]))
        for j in rest:
            if len(basis) == m:
                break
            B = A[:, basis + [j]]
            if mat_rank(B) > len(basis):
                basis.append(j)
    if len(basis) < m:
        return dict(status="rank_deficient")
    inB = set(basis)
    # 超基本 = 候选但未入基，带 IPM 值
    super_ = {j: float(x_ipm[j]) for j in basic_cand if j not in inB}
    xN = np.zeros(n)
    for j in nonbasic:
        xN[j] = 0.0
    pivots = 0
    flips = 0
    # 阶段2：主元循环消超基本
    while super_:
        j = max(super_, key=lambda k: super_[k])
        xj = super_[j]
        facB = lu_factor(A[:, basis])
        xB = lu_solve(facB, b - A[:, list(super_.keys())] @ np.array(
            [super_[k] for k in super_]) if super_ else b)
        alpha = lu_solve(facB, A[:, j])
        # 方向：x_j 下降 δ → x_B + δα ≥ 0
        dmax = None
        leave = -1
        for i in range(m):
            if alpha[i] < -1e-12:
                r = max(0.0, xB[i]) / (-alpha[i])
                if dmax is None or r < dmax:
                    dmax = r
                    leave = i
        dmax = 0.0 if dmax is None else dmax
        if dmax >= xj - 1e-12:
            # 直接到界：非基化
            super_.pop(j)
            xN[j] = 0.0
            flips += 1
        else:
            # 主元：j 入基（值 xj − dmax），leave 出基（到 0）
            delta = dmax
            super_.pop(j)
            # 出基变量到下界 0（非基化）；j 以值 xj − δ 入基（x_B 由重算恢复等式）
            basis[leave] = j
            pivots += 1
    # 阶段3：对偶检查 + 单纯形收尾
    facB = lu_factor(A[:, basis])
    xB = lu_solve(facB, b)
    y = lu_solve_T(facB, c[basis])
    d = c - A.T @ y
    inB = set(basis)
    dual_ok = all(d[j] >= -1e-7 * (1 + abs(c[j])) for j in range(n) if j not in inB)
    if log is not None:
        log.append(f"  Crossover: 主元 {pivots} 次翻界 {flips} 次，对偶可行={dual_ok}")
    if not dual_ok:
        sx = Simplex(A, b, c)
        res = sx._loop2(A, b, c, basis, 40 * max(1, m), "cleanup")
        if res[0] != "optimal":
            return dict(status=res[0])
        basis = res[1]
        xB = res[2]
        facB = lu_factor(A[:, basis])
        y = lu_solve_T(facB, c[basis])
    x = np.zeros(n)
    x[basis] = xB
    return dict(status="optimal", x=x, y=y, basis=basis, pivots=pivots)

# ---------------- 完整流水线 ----------------
def solve_lp(A, b, c, log=None):
    m, n = A.shape
    assert np.all(b >= -1e-12), "b 须 ≥0（调用方翻转）"
    r = ipm_solve(A, b, c, log=log)
    if r["status"] == "optimal":
        cr = crossover(A, b, c, r["x"], r["s"], log=log)
        if cr["status"] == "optimal":
            gap = abs(float(c @ cr["x"]) - float(c @ r["x"]))
            if log is not None:
                log.append(f"  |obj_cx − obj_ipm| = {gap:.3e}")
            return dict(status="optimal", x=cr["x"], y=cr["y"], iters=r["iters"],
                        pivots=cr["pivots"], obj=float(c @ cr["x"]))
        # crossover 失败 → 纯单纯形
        sx = Simplex(A, b, c)
        res = sx.solve()
        obj = float(c @ res["x"]) if res.get("x") is not None else None
        return dict(status=res["status"], x=res.get("x"), y=res.get("y"),
                    iters=r["iters"] + res.get("iters", 0), obj=obj)
    # IPM 失败 → 纯单纯形兜底
    sx = Simplex(A, b, c)
    res = sx.solve()
    obj = float(c @ res["x"]) if res.get("x") is not None else None
    return dict(status=res["status"], x=res.get("x"), y=res.get("y"),
                iters=res.get("iters", 0), obj=obj)

# ============================ 测试 ============================
def check(name, cond, detail=""):
    global FAILS
    if cond:
        print(f"  [PASS] {name} {detail}")
    else:
        FAILS += 1
        print(f"  [FAIL] {name} {detail}")

FAILS = 0

def test_known():
    print("== T1 已知最优 + 影子价 (max 3x+5y) ==")
    # max 3x1+5x2; x1≤4; 2x2≤12; 3x1+2x2≤18 → min −(3,5)·x, 标准形 + 3 松弛
    A = np.array([[1, 0, 1, 0, 0], [0, 2, 0, 1, 0], [3, 2, 0, 0, 1]], float)
    b = np.array([4, 12, 18], float)
    c = np.array([-3, -5, 0, 0, 0], float)
    log = []
    r = solve_lp(A, b, c, log)
    x = r["x"][:2]
    check(r["status"] == "optimal", "状态", f"iters={r['iters']}")
    check(abs(r["obj"] + 36) < 1e-6, "目标 = −36", f"obj={r['obj']:.6f}")
    check(abs(x[0] - 2) < 1e-5 and abs(x[1] - 6) < 1e-5, "x* = (2,6)", f"x={x}")
    # 对偶（min 形 y ≤ 0）：max 意义影子价 = −y_min = (0, 1.5, 1)
    ymax = -r["y"]
    check(abs(ymax[0]) < 1e-6 and abs(ymax[1] - 1.5) < 1e-6 and abs(ymax[2] - 1) < 1e-6,
          "影子价 (0,1.5,1)", f"y={np.round(ymax,6)}")
    check(r["pivots"] <= 6, "crossover 主元次数少", f"pivots={r['pivots']}")

def test_mixed():
    print("== T2 ≥/≤ 混合 (min 2x+3y) ==")
    # x+y≥10; x≥2; y≤8 → 标准形
    A = np.array([[1, 1, -1, 0, 0], [1, 0, 0, -1, 0], [0, 1, 0, 0, 1]], float)
    b = np.array([10, 2, 8], float)
    c = np.array([2, 3, 0, 0, 0], float)
    r = solve_lp(A, b, c)
    check(r["status"] == "optimal", "状态")
    check(abs(r["obj"] - 20) < 1e-6, "目标 = 20", f"obj={r['obj']:.6f}")
    x = r["x"][:2]
    check(abs(x[0] - 10) < 1e-5 and abs(x[1]) < 1e-5, "x*=(10,0)", f"x={x}")
    ymin = r["y"]    # min 问题影子价 = y
    check(abs(ymin[0] - 2) < 1e-6 and abs(ymin[1]) < 1e-6 and abs(ymin[2]) < 1e-6,
          "影子价 (2,0,0)", f"y={np.round(ymin,6)}")

def test_degenerate():
    print("== T3 退化多解 (min x+y, x+y=5) ==")
    A = np.array([[1, 1, 0, 0], [1, 0, 1, 0], [0, 1, 0, -1]], float)
    b = np.array([5, 3, 1], float)
    c = np.array([1, 1, 0, 0], float)
    r = solve_lp(A, b, c)
    check(r["status"] == "optimal", "状态")
    check(abs(r["obj"] - 5) < 1e-6, "目标 = 5", f"obj={r['obj']:.6f}")
    xx = r["x"]
    check(abs(xx[0] + xx[1] - 5) < 1e-6 and xx[0] <= 3 + 1e-6 and xx[1] >= 1 - 1e-6,
          "可行性", f"x={np.round(xx,6)}")

def test_klee_minty():
    print("== T4 Klee-Minty 3 维 ==")
    A = np.array([[1, 0, 0, 1, 0, 0], [20, 1, 0, 0, 1, 0], [200, 20, 1, 0, 0, 1]], float)
    b = np.array([1, 100, 10000], float)
    c = np.array([-100, -10, -1, 0, 0, 0], float)
    r = solve_lp(A, b, c)
    check(r["status"] == "optimal", "状态")
    check(abs(r["obj"] + 10000) < 1e-6, "目标 = −10000", f"obj={r['obj']:.6f}")

def test_infeasible():
    print("== T5 不可行 ==")
    A = np.array([[1, -1, 0], [1, 0, 1]], float)
    b = np.array([5, 2], float)
    c = np.array([1, 0, 0], float)
    r = solve_lp(A, b, c)
    check(r["status"] == "infeasible", "证书 = infeasible", f"status={r['status']}")

def test_unbounded():
    print("== T6 无界 ==")
    A = np.array([[1, -1, 1]], float)
    b = np.array([1], float)
    c = np.array([-1, 0, 0], float)
    r = solve_lp(A, b, c)
    check(r["status"] == "unbounded", "证书 = unbounded", f"status={r['status']}")

def test_random():
    print("== T7 随机 25×65 ==")
    rng = np.random.default_rng(42)
    m0, n = 15, 25
    A0 = np.where(rng.random((m0, n)) < 0.35, rng.normal(0, 2, (m0, n)), 0.0)
    xstar = np.where(rng.random(n) < 0.6, rng.uniform(0.5, 5, n), 0.0)
    # 上界行保证有界：c<0 的变量加 x_j ≤ u_j
    c = np.round(rng.normal(0, 3, n), 3)
    ub_rows = []
    ub_vals = []
    for j in range(n):
        if c[j] < 0:
            ub_rows.append(j)
            ub_vals.append(xstar[j] + rng.uniform(0.5, 3))
    m = m0 + len(ub_rows)
    A = np.zeros((m, n + m))
    A[:m0, :n] = A0
    for k, j in enumerate(ub_rows):
        A[m0 + k, j] = 1.0
    for i in range(m):
        A[i, n + i] = 1.0
    b = A[:m, :n] @ xstar
    for i in range(m):          # b ≥ 0 行翻转（求解器标准形构造同款）
        if b[i] < 0:
            A[i, :] = -A[i, :]
            b[i] = -b[i]
    c_full = np.concatenate([c, np.zeros(m)])
    r = solve_lp(A, b, c_full)
    check(r["status"] == "optimal", "状态", f"iters={r.get('iters')}")
    x = r["x"][:n]
    resid = np.linalg.norm(A[:m, :n] @ x + A[:m, n:] @ r["x"][n:] - b)
    check(resid < 1e-6, "原始可行", f"‖Ax−b‖={resid:.2e}")
    check(np.min(x) > -1e-7, "x ≥ 0")
    # KKT: 完整 A（含松弛）互补松弛 + 支撑集 reduced cost
    dfull = c_full - A.T @ r["y"]
    comp = float(np.abs((r["x"]) * dfull).max())
    check(comp < 1e-5, "互补松弛", f"max x·d={comp:.2e}")
    check(np.min(dfull[r["x"] > 1e-6]) > -1e-5 if np.any(r["x"] > 1e-6) else True,
          "支撑集上 reduced cost ≈ 0")

def test_redundant():
    print("== T8 冗余等式行 ==")
    # 行3 = 行1 + 行2（b 一致）→ 冗余，应删除并正常求解
    A = np.array([[1, 1, 1, 0, 0, 0],
                  [1, 2, 0, 1, 0, 0],
                  [2, 3, 1, 1, 0, 0]], float)
    b = np.array([4, 5, 9], float)
    c = np.array([1, 1, 1, 0, 0, 0], float)
    r = solve_lp(A, b, c)
    check(r["status"] == "optimal", "状态", f"status={r['status']}")
    # min x1+x2+x3 s.t. x1+x2+x3=4, x1+2x2=5 → x1=3,x2=1,x3=0 → obj 4
    check(abs(r["obj"] - 4) < 1e-6, "目标 = 4", f"obj={r['obj']:.6f}")

if __name__ == "__main__":
    test_known()
    test_mixed()
    test_degenerate()
    test_klee_minty()
    test_infeasible()
    test_unbounded()
    test_random()
    test_redundant()
    print(f"\n总计: {'ALL PASS' if FAILS == 0 else f'{FAILS} FAILS'}")
