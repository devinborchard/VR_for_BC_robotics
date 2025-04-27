import torch
from torch.utils.data import Dataset, DataLoader
import pandas as pd
import numpy as np
import torch
import os
import json
import sys
import h5py


def findDemo(file, demoName):
    foundName = ""
    def findDemoName(name, obj):
        nonlocal foundName
        if obj.attrs:
            # print(demoName , obj.attrs['description'])
            if( demoName in obj.attrs['description']):
                foundName = name

    with h5py.File(file, "r") as f:
        f.visititems(findDemoName)

    return foundName

def main():
    # Code to be executed when the script is run directly
    # print(f"This is the main function. with args {sys.argv[1:]} , {sys.argv[2]}")
    hdf5File = sys.argv[1]
    demoName = sys.argv[2]
    demo = findDemo(hdf5File, demoName)
    # print("FOUND IT: ", demo)

    actionsOut = ""
    with h5py.File(hdf5File, "r") as f:
        actions = f[demo+'/actions'][()]  # Load the entire dataset
        for action in actions:
            actionString = ""
            for dim in action:
                actionString = actionString + str(dim) + ','
            actionsOut = actionsOut + actionString + '\n'
            # print("A: ", action)
    print(actionsOut)

if __name__ == "__main__":
    main()