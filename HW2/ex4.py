# Exercise 4
# In a popular board game, the players roll two dice at every turn. They want to test the fairness of the dice, so
# they note the number of occurrences of each possible result, from 2 to 12. The collected data are as follows
# values      2  3  4  5  6  7  8  9 10 11 12
# occurrences 1  4  2  7 10  9  9 14  7  5  3
# 1. Find the probability mass function of the distribution
# 2. Run a chi-squared test on this data to check if it is in accordance with the discrete triangular distribution
# that characterizes a 2-dice roll: ...
# Discuss the result of the test.

# import
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp


# define corrected functions for std and variance
def variance(values):
	return np.var(values, ddof=1)


def std(values):
	return np.std(values, ddof=1)


def triangular(k):
	if k >= 2 and k <= 7:
		return 1 / 36 * (k - 1)
	else:
		return 1 / 36 * (13 - k)


data = np.array([1, 4, 2, 7, 10, 9, 9, 14, 7, 5, 3])
X = range(2, 13)  # possible values
N = sum(data)  # number of trials
Y = data / N  # sample probabilities
plt.plot(X, Y, 'o--', color='k')  # bar, lines o cosa?
# plt.show()
Y = [triangular(k) for k in X]
plt.plot(X, Y, 'o--', color='r')
plt.show()

K = len(X)
T = 0
for i in range(K):
	k = X[i]
	T += (data[i] - triangular(k) * N)**2 / (N * triangular(k))

print(f"T = {T: .3f}")
print(f"Prob t>=T (p-value) = {1 - chi2.cdf(T, K-1): .4f}")
