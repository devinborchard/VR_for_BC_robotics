import torch
from torch.utils.data import Dataset, DataLoader
import pandas as pd
import numpy as np
import torch
import os
from PIL import Image
import torchvision.transforms as T
import torchvision.models as models
import h5py

episodesDirectory = "C:/Users/Devin/Desktop/demos"


def parseState(string):
    trimmedString = string.strip(" ")
    stringsList = trimmedString.split(",")
    parsedState = []
    for string in stringsList:
        parsedState.append(float(string.split(':')[1]))
    return parsedState

def parseStates(stateStrings):
    times = []
    dones = []
    eef_pos = []
    eef_quat = []
    eef_rot = []
    gripper = []
    for string in stateStrings:
        state = parseState(string)

        times.append(state[0])
        eef_pos.append([state[1], state[2], state[3]])
        eef_quat.append([state[4], state[5], state[6], state[7]])
        eef_rot.append([state[8], state[9], state[10]])
        gripper.append(state[11])
        dones.append(state[12])

    return times, dones, eef_pos, eef_quat, eef_rot, gripper

def createActions(eef_pos, eef_rot, gripper):
    actions = []
    for index, currentPos in enumerate(eef_pos):
        currentState = [currentPos[0], currentPos[1], currentPos[2], eef_rot[index][0], eef_rot[index][1], eef_rot[index][2], gripper[index]]
        if(index < len(eef_pos)-1): # to skip last state and avoid out of bounds error
            nextState = [eef_pos[index+1][0], eef_pos[index+1][1], eef_pos[index+1][2], eef_rot[index+1][0], eef_rot[index+1][1], eef_rot[index+1][2], gripper[index+1]]

            difference = [round(b - a, 5) for a, b in zip(currentState, nextState)]
            actions.append(difference)
    return actions


def parseImage(episodesDirectory, demoFolder, imageFolder):
    parsedImages = []
    images = os.listdir(episodesDirectory+"/"+demoFolder+"/"+imageFolder)
    for image in images:
        img = Image.open(episodesDirectory+"/"+demoFolder+"/"+imageFolder+"/"+image).convert('RGB')  # ensure 3 channels (RGB)
        transform = T.ToTensor()  # Converts to [C, H, W] tensor with values in [0, 1]
        imageTensor = transform(img)
        parsedImages.append(imageTensor)
    return parsedImages


def main():
    # Code to be executed when the script is run directly
    demoFolders = os.listdir(episodesDirectory)  # Gets all file and folder names


    for folder in demoFolders:
        # Create an HDF5 file

        print("FOLDER: ", folder)
        seed = folder.split('_')[2]
        f = open(episodesDirectory+f'/{folder}/demo.txt', "r")
        stateString = f.read()
        states = stateString.split('\n')

        states = states[:-1] #remove empty newline
        
        # for s in states:
        #     print(s)

        times, dones, eef_pos, eef_quat, eef_rot, gripper = parseStates(states)

        # # print(parsedStates)

        # print("ACTIONS: ", actions)
        # robotActions = isolateActions(actions)

        # parsedStates = states[:-1] #remove last unused state
        overImages = parseImage(episodesDirectory, folder, "overCaptures")
        handImages = parseImage(episodesDirectory, folder, "handCaptures")


        assert(len(times) == len(dones) == len(eef_pos) == len(eef_quat) == len(eef_rot) == len(gripper) == len(overImages) == len(handImages))
        
        actions = createActions(eef_pos, eef_rot, gripper)

        with h5py.File(episodesDirectory+f'/{folder}/{folder}.hdf5', 'w') as f:
            group = f.create_group(f"demo_{folder}")
            # Create a dataset named 'my_dataset'
            group.create_dataset('actions', data=actions)
            group.create_dataset('times', data=times[:-1])
            group.create_dataset('dones', data=dones[:-1])
            group.create_dataset('eef_pos', data=eef_pos[:-1])
            group.create_dataset('eef_quat', data=eef_quat[:-1])
            group.create_dataset('eef_rot', data=eef_rot[:-1])
            group.create_dataset('gripper', data=gripper[:-1])
            group.create_dataset('overImages', data=overImages[:-1])
            group.create_dataset('handImages', data=handImages[:-1])


if __name__ == "__main__":
    main()



# Path: data
# Type: Group

# Path: data/demo_1
# Type: Group

# Path: data/demo_1/actions   
# Type: Dataset
# Shape: (134, 7)
# Dtype: float64
# Data: [too large to display]

# Path: data/demo_1/dones     
# Type: Dataset
# Shape: (134,)
# Dtype: int64
# Data: [too large to display]

# Path: data/demo_1/obs
# Type: Group

# Path: data/demo_1/obs/agentview_image
# Type: Dataset
# Shape: (134, 84, 84, 3)
# Dtype: uint8
# Data: [too large to display]

# Path: data/demo_1/obs/robot0_eef_pos
# Type: Dataset
# Shape: (134, 3)
# Dtype: float64
# Data: [too large to display]

# Path: data/demo_1/obs/robot0_eef_quat
# Type: Dataset
# Shape: (134, 4)
# Dtype: float64
# Data: [too large to display]

# Path: data/demo_1/obs/robot0_eye_in_hand_image
# Type: Dataset
# Shape: (134, 84, 84, 3)
# Dtype: uint8
# Data: [too large to display]

# Path: data/demo_1/obs/robot0_gripper_qpos
# Type: Dataset
# Shape: (134, 2)
# Dtype: float64
# Data: [too large to display]