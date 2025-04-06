import torch
from torch.utils.data import Dataset, DataLoader
import pandas as pd
import numpy as np
import torch
import os
import json


def parseState(string):
    trimmedString = string.strip(" ")
    stringsList = trimmedString.split(",")
    parsedState = []
    for string in stringsList:
        parsedState.append(float(string.split(':')[1]))
    return parsedState[1:] #dont return the time feature

def parseStates(stateStrings):
    parsedStates = []
    for string in stateStrings:
        parsedStates.append(parseState(string))

    return parsedStates

def createActions(states):
    actions = []
    for index, currentState in enumerate(states):
        if(index < len(states)-1): # to skip last state and acoid out of bounds error
            nextState = states[index+1]

            difference = [round(b - a, 5) for a, b in zip(currentState, nextState)]
            actions.append(difference)
    return actions
    
def savePairs(states, actions):
    
    data = json.dumps([states,actions])
    return data

def main():
    # Code to be executed when the script is run directly
    print("This is the main function.")
    episodesDirectory = "c:/Users/Devin/AppData/LocalLow/DefaultCompany/CS933 Project/"
    saveDirectory = "c:/Users/Devin/AppData/LocalLow/DefaultCompany/CS933_Project_SIM/"
    
    file = "2025-04-06_11-50-36"
    
    f = open(episodesDirectory+'/'+file+"/demo.txt", "r")
    stateString = f.read()
    states = stateString.split('\n')

    states = states[:-1]

    parsedStates = parseStates(states)

    # print(parsedStates)

    actions = createActions(parsedStates)
    parsedStates = parsedStates[:-1] #remove last unused state

    allStates = parsedStates
    allActions =actions

    assert(len(allActions) == len(allStates))

    data = savePairs(allStates, allActions)
    f = open(saveDirectory+file+"-SIM.txt", "a")

    data = data.replace("\'","\"")
    data = data.replace(" ","")
    print("WRITING: ", data)

    f.write(data)
    f.close()


    print("DATA SAVED!")


if __name__ == "__main__":
    main()