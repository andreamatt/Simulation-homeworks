# import and solve data
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp

# define corrected functions for std and variance


def std(values):
	return np.std(values, ddof=1)


def success_probability_confidence_normal(values, confidence):
	n = len(values)
	z = sum(values)
	eta = norm.ppf((1 + confidence) / 2)
	lower = z / n - eta / n * sqrt(z * (1 - z / n))
	upper = z / n + eta / n * sqrt(z * (1 - z / n))
	return (lower, upper)


def success_probability_confidence_three(values, confidence):
	n = len(values)
	lower = 0
	upper = 1 - pow(((1 - confidence) / 2), 1 / n)
	# if sum(values) == 0:
	# 	lower = 0
	# 	upper = 1 - pow(((1 - confidence) / 2), 1 / n)
	# elif sum(values) == len(values):
	# 	lower = pow(((1 - confidence) / 2), 1 / n)
	# 	upper = 1
	# else:
	# 	lower = pow(((1 - confidence) / 2), 1 / n)
	# 	upper = 1 - pow(((1 - confidence) / 2), 1 / n)
	return (lower, upper)


def success_probability_confidence(values, confidence):
	if sum(values) >= 6 and len(values) - sum(values) >= 6:
		return success_probability_confidence_normal(values, confidence)
	return success_probability_confidence_three(values, confidence)


outcomes = []
i = 0
while (True):
	i += 1
	x = np.random.rand() * 2 - 1
	y = np.random.rand() * 2 - 1
	d = sqrt(x**2 + y**2)
	if d <= 1:
		outcomes.append(1)
	else:
		outcomes.append(0)

	# current_prob = sum(outcomes)/len(outcomes)
	interval = np.array(success_probability_confidence(outcomes, 0.95))
	# print(interval)
	
	# if current_prob <= interval[0] or current_prob >= interval[1]:
	# 	break
	if interval[1] - interval[0] < 0.01:
		break

print(f'{sum(outcomes) / len(outcomes) * 4: .4f}, {i}')
