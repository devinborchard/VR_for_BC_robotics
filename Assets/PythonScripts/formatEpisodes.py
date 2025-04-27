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

episodesDirectory = "C:/Users/Devin/Desktop/demos/fixed"


def parseState(string):
    trimmedString = string.strip(" ")
    stringsList = trimmedString.split(",")
    parsedState = []
    for string in stringsList:
        parsedState.append(float(string.split(':')[1]))
    return parsedState

def fixRotation(rot, lastRot):
    if(lastRot == []):
        return rot

    for i, x in enumerate(rot):
        if x - lastRot[i] > 180:
            rot[i] = -1*(360 - x)
    return rot

def parseStates(stateStrings):
    times = []
    dones = []
    eef_pos = []
    eef_quat = []
    eef_rot = []
    gripper = []
    doneCount = 0
    lastStateRep = []
    lastRotation = []
    for string in stateStrings:
        state = parseState(string)

        rot = [state[8], state[9], state[10]]
        rot = fixRotation(rot, lastRotation)

        stateRep = [state[1], state[2], state[3], state[8], state[9], state[10]]
        if(stateRep != lastStateRep or state[11] == 1): #accept all gripper closed states

            times.append(state[0])
            eef_pos.append([state[1], state[2], state[3]])
            eef_quat.append([state[4], state[5], state[6], state[7]])
            eef_rot.append(rot)
            gripper.append([state[11]])
            dones.append(state[12])
            
            lastStateRep = stateRep
        
            lastRotation = rot



    return times, dones, eef_pos, eef_quat, eef_rot, gripper

def createActions(eef_pos, eef_rot, gripper):
    actions = []
    gripperState = 0
    for index, currentPos in enumerate(eef_pos):
        currentState = [currentPos[0], currentPos[1], currentPos[2], eef_rot[index][0], eef_rot[index][1], eef_rot[index][2]]
        curGripper = gripper[index][0]
        if(index < len(eef_pos)-1): # to skip last state and avoid out of bounds error
            nextState = [eef_pos[index+1][0], eef_pos[index+1][1], eef_pos[index+1][2], eef_rot[index+1][0], eef_rot[index+1][1], eef_rot[index+1][2]]
            nextGripper = gripper[index+1][0]

            if(nextGripper - curGripper == 1):
                gripperState = 1
            if(nextGripper - curGripper == -1):
                gripperState = 0
            

            difference = [round(b - a, 5) for a, b in zip(currentState, nextState)]
            difference.append(gripperState)
            actions.append(difference)
    return actions


def parseImage(imageDir):
    parsedImages = []
    images = os.listdir(imageDir)
    sorted_images = sorted(images, key=lambda x: float(x.replace('.png', '')))
    for image in sorted_images:
        # print("IMG: ", image)
        img = Image.open(imageDir + "/" + image).convert('RGB')  # ensure 3 channels (RGB)
        imageArray = np.array(img)  # Converts to (H, W, C) = (84, 84, 3)
        parsedImages.append(imageArray)
    return parsedImages


def main():
    includedTasks = ['basic1']
    # Code to be executed when the script is run directly
    userFolders = os.listdir(episodesDirectory)  # Gets all file and folder names
    demoCount = 1
    with h5py.File("C:/Users/Devin/Desktop/demos"+f'/temp.hdf5', 'w') as f:
        for user in userFolders:
            userDirectory = os.listdir(episodesDirectory+'/'+user)
            for task in userDirectory:
                if(any(sub in task for sub in includedTasks)):
                    taskFolders = os.listdir(episodesDirectory+'/'+user+'/'+task)
                    for folder in taskFolders:
                        # Create an HDF5 file

                        print("FOLDER: ", folder)
                        seed = folder.split('_')[2]
                        fd = open(episodesDirectory+'/'+user+'/'+task+f'/{folder}/demo.txt', "r")
                        stateString = fd.read()
                        states = stateString.split('\n')

                        states = states[:-1] #remove empty newline
                        
                        times, dones, eef_pos, eef_quat, eef_rot, gripper = parseStates(states)

                        print("DONE: ",dones[len(dones) - 1])
                        if(dones[len(dones) - 1] == 1):
                            print("ADDING")
                            overImages = parseImage(episodesDirectory+'/'+user+'/'+task+f'/{folder}/overCaptures')[0:len(times)]
                            handImages = parseImage(episodesDirectory+'/'+user+'/'+task+f'/{folder}/handCaptures')[0:len(times)]


                            assert(len(times) == len(dones) == len(eef_pos) == len(eef_quat) == len(eef_rot) == len(gripper) == len(overImages) == len(handImages))
                            
                            actions = createActions(eef_pos, eef_rot, gripper)

                            group = f.create_group(f"data/demo_{demoCount}")
                            group.attrs['description'] = user+'/'+task+f'/{folder}'
                            group.attrs['num_samples'] = len(actions)

                            # Create a dataset named 'my_dataset'
                            group.create_dataset('actions', data=actions)
                            group.create_dataset('times', data=times[:-1])
                            group.create_dataset('dones', data=dones[:-1])

                            obsGroup = f.create_group(f"data/demo_{demoCount}/obs")
                            # Create a dataset named 'my_dataset'
                            obsGroup.create_dataset('robot0_eef_pos', data=eef_pos[:-1])
                            obsGroup.create_dataset('robot0_eef_quat', data=eef_quat[:-1])
                            obsGroup.create_dataset('robot0_eef_rot', data=eef_rot[:-1])
                            obsGroup.create_dataset('robot0_gripper', data=gripper[:-1])
                            obsGroup.create_dataset('agentview_image', data=overImages[:-1])
                            obsGroup.create_dataset('robot0_eye_in_hand_image', data=handImages[:-1])

                            demoCount = demoCount +1


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