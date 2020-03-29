from pandas import read_csv
import numpy as np
from matplotlib.pyplot import hist, show, scatter
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, chi2
from math import sqrt, pow, floor, ceil


def variance(values):
    return np.var(values, ddof=1)


def std(values):
    return np.std(values, ddof=1)


def CoV(values):
    # Coeff of variation (not defined on heavy tailed sets, because variance=>inf)
    return std(values)/mean(values)


def MAD(values):
    # mean absolute deviation, always defined
    n = len(values)
    m = mean(values)
    return sum([abs(x-m) for x in values])/n


def lorenz_gap(values):
    # always defined, the best
    return MAD(values)/(2*mean(values))


def JFI(values):
    # Jain Fairness Index (same problems as CoV?)
    return 1/(1+pow(CoV(values), 2))


def Gini(values):
    # useless
    return 0


def lorenz_curve(values):
    values = sorted(values)
    curve = []
    line = []
    tot = sum(values)
    cum_sum = np.cumsum(values)
    for i in np.arange(0, len(values), 20):
        line.append(i/len(values))
        curve.append(cum_sum[i]/tot)

    scatter(line, curve, s=0.1)


def quantile_confidence(values, q, confidence):
    # return tuple (lower, upper) of values
    values = sorted(values)
    n = len(values)
    if n > 100:
        # approximate
        z = norm.ppf((1+confidence)/2)
        j = floor(n*q-z*sqrt(n*q*(1-q)))
        k = ceil(n*q+z*sqrt(n*q*(1-q)))+1
        return (values[j], values[k], confidence)

    results = []
    binoms = [binom.cdf(i, n, q) for i in range(0, n)]
    for j in range(1, n):
        for k in range(j, n):
            val = binoms[k] - binoms[j]
            if val >= confidence:
                results.append((j, k, val))

    # sort by smallest interval and highest confidence value
    results = sorted(results, key=lambda x: (x[1]-x[0], -x[2]))
    best = results[0]
    return (values[best[0]], values[best[1]])


def mean_confidence_asymptotic(values, confidence):
    # when variables iid and high n
    n = len(values)
    z = norm.ppf((1+confidence)/2)
    s = std(values)
    m = mean(values)
    j = m - z*s/sqrt(n)
    k = m + z*s/sqrt(n)
    return (j, k)


def mean_confidence_normal(values, confidence):
    # returns lower and upper bounds of mean confidence for normally distributed iid RV
    # under 30 values prefer this over asymptotic
    n = len(values)
    z = student.ppf((1+confidence)/2, n-1)
    s = std(values)
    m = mean(values)
    j = m - z*s/sqrt(n)
    k = m + z*s/sqrt(n)
    return (j, k)


def std_confidence_normal(values, confidence):
    n = len(values)
    z1 = chi2.ppf((1-confidence)/2, n-1)
    z2 = chi2.ppf((1+confidence)/2, n-1)
    s = std(values)
    j = s*sqrt((n-1)/z1)
    k = s*sqrt((n-1)/z2)
    return (j, k)


def bootstrap(values, confidence, func, r0=25):
    R = 1  # ceil(2*r0/(1-confidence))
    n = len(values)
    for r in range(R):
        V = np.random.choice(values, size=(n,))
        print(V)


def QQ_plot(values):
    values = sorted(values)
    curve = []
    line = []
    tot = sum(values)
    cum_sum = np.cumsum(values)
    for i in np.arange(0, len(values), 20):
        line.append(i/len(values))
        curve.append(cum_sum[i]/tot)

    scatter(line, curve, s=0.1)

def success_probability_confidence_normal(values, confidence):
    n = len(values)
    z = sum(values)
    eta = norm.ppf((1+confidence)/2)
    lower = z/n - eta/n * sqrt(z* (1 - z/n))
    upper = z/n + eta/n * sqrt(z* (1 - z/n))
    return (lower, upper)