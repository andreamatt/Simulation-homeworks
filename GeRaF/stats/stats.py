from matplotlib import pyplot as plt
import numpy as np
import json
from classes import *
from plot_test import *


jsonFile = open("runResults.json")
data = json.load(jsonFile)
runResult = RunResult(data)

# plot_outcomes(sim_stats)
# plot_success_over_distance(sim_stats)
plot_avgdelay_over_lambda_and_duty(runResult)