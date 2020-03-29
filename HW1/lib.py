import pandas
import numpy as np
from matplotlib.pyplot import hist, show, scatter
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, chi2
from math import sqrt, pow, floor, ceil

def variance(values):
    return np.var(values, ddof=1)

def std(values):
    return np.std(values, ddof=1)

data = 'HW1\\data'
data_ex1 = f'{data}\\data_ex2.csv'

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
    for i in range(len(values)):
        line.append(i/len(values))
        curve.append(sum(values[:i])/tot)

    scatter(line, curve)



def quantile_confidence(values, q, confidence):
    # return tuple (j, k) of indexes
    values = sorted(values)
    n = len(values)
    if n > 100:
        # approximate
        z = norm.ppf((1+confidence)/2)
        left = n*q
        right = z*sqrt(n*q*(1-q))
        j = floor(left-right)
        k = ceil(left+right)+1
        return (j, k, confidence)

    results = []
    for j in range(1, n):
        for k in range(j, n):
            val = binom.cdf(k, n, 0.5)-binom.cdf(j, n, 0.5)
            if val >= confidence:
                results.append((j, k, val))

    results = sorted(results, key=lambda x: (x[1]-x[0], -x[2]))
    return (values[results[0]], values[results[1]])


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


# print(quantile_confidence(31, 0.5, 0.95))

#0.98
print(0.5*(1.96))
print(norm.ppf((1+0.95)/2))
print(norm.ppf((1+0.99)/2))
print(student.ppf((1+0.95)/2, 100-1))
print(student.ppf((1+0.95)/2, 100))


ex1 = pandas.read_csv(data_ex1, header=None)
values = ex1.to_numpy()[:, 0]

# print("mean", mean(values))
# print("std", std(values))
# print("var (aka std^2)", var(values))
# print("CoV", CoV(values))
# print("MAD", MAD(values))
# print("Lorenz gap", lorenz_gap(values))
# print("JFI", JFI(values))
