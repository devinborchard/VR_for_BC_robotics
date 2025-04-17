import torch
from torch.utils.data import Dataset, DataLoader
import pandas as pd
import numpy as np
import torch
import os
from PIL import Image
import torchvision.transforms as T
import torchvision.models as models

episodesDirectory = "c:/Users/Devin/AppData/LocalLow/DefaultCompany/CS933 Project/"


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
    gripper = []
    for string in stateStrings:
        state = parseState(string)

        times.append(state[0])
        eef_pos.append([state[1], state[2], state[3]])
        eef_quat.append([state[4], state[5], state[6], state[7]])
        gripper.append(state[8])
        dones.append(state[9])

    return times, dones, eef_pos, eef_quat, gripper

# def createActions(states):
#     actions = []
#     for index, currentState in enumerate(states):
#         if(index < len(states)-1): # to skip last state and avoid out of bounds error
#             nextState = states[index+1]

#             difference = [round(b - a, 5) for a, b in zip(currentState, nextState)]
#             actions.append(difference)
#     return actions


# def isolateActions(actions):
#     isolatedActions = []
#     for action in actions:
#         isolated = action[0:7]
#         isolatedActions.append(torch.tensor(isolated))
#     return isolatedActions

# class StateActionDataset(Dataset):
#     def __init__(self, states, actions):
#         self.states = states
#         self.actions = actions

#     def __len__(self):
#         return len(self.states)

#     def __getitem__(self, idx):
#         return self.states[idx], self.actions[idx]
    
# def addImagesToState(states, times, folder):
#     observations = []
#     for i, state in enumerate(states):
#         time = times[i]
#         img = Image.open(episodesDirectory+f'{folder}/camCaptures/{time}.png').convert('RGB')  # ensure 3 channels (RGB)
#         transform = T.ToTensor()  # Converts to [C, H, W] tensor with values in [0, 1]
#         imageTensor = transform(img)
#         observations.append([torch.tensor(state),imageTensor])
#     return observations

# def saveDataLoader(states, actions, dataLoaderPath):
#     # Save the dataset as a dictionary
#     torch.save({
#         "states": torch.stack(states),  # Convert list of tensors to a single tensor
#         "actions": torch.stack(actions),
#     }, dataLoaderPath)

# def loadDataLoader(dataLoaderPath):
#     loaded_data = torch.load(dataLoaderPath)

#     # Reconstruct dataset
#     dataset = StateActionDataset(loaded_data["states"], loaded_data["actions"])

#     # Recreate DataLoader
#     dataloader = DataLoader(dataset, batch_size=16, shuffle=True)
#     return dataloader

# def encodeObservation(observations):
#     regnet = models.regnet_y_400mf(pretrained=True)
#     regnet.eval()
#     image_encoder = torch.nn.Sequential(*(list(regnet.children())[:-1]))  # Removes the final FC layer
#     transform = T.Compose([
#         T.Resize((256, 256)),
#         T.Normalize(mean=[0.485, 0.456, 0.406],
#                     std=[0.229, 0.224, 0.225])
#     ])

#     encodedObservations = []
#     max_len = 0
#     # print(len(observations), "episodes")

#     # First pass: encode and track max length
#     for i, observation in enumerate(observations):
#         # print("ep:", i)
#         encodedStates = []

#         # print(len(observation), "states")
#         for state in observation:
#             image = state[1]
#             low_dim_state = state[0].unsqueeze(0)    
#             img = transform(image)
#             img = img.unsqueeze(0)
#             with torch.no_grad():
#                 features = image_encoder(img)  # [1, 440, 1, 1]
#                 features = features.view(features.size(0), -1)  # [1, 440]
#             combined = torch.cat([features, low_dim_state], dim=1)  # [1, 447]
#             encodedStates.append(combined.squeeze(0))  # [447]

#         max_len = max(max_len, len(encodedStates))
#         encodedObservations.append(encodedStates)

#     # Second pass: pad and convert to tensors of shape [max_len, 447]
#     paddedObservations = []
#     for states in encodedObservations:
#         length = len(states)
#         if length < max_len:
#             pad_size = max_len - length
#             pad = [torch.zeros_like(states[0]) for _ in range(pad_size)]
#             states.extend(pad)
#         padded_tensor = torch.stack(states)  # [max_len, 447]
#         paddedObservations.append(padded_tensor)

#     return paddedObservations

# def pad_action_sequences(action_sequences, max_len):
#     padded_actions = []

#     for actions in action_sequences:
#         length = len(actions)
        
#         action_dim = actions[0].shape[-1]
#         if length < max_len:
#             pad_size = max_len - length
#             padding = [torch.zeros(action_dim) for _ in range(pad_size)]
#             actions = actions + padding  # pad at the end

#         padded_actions.append(torch.stack(actions))  # shape: [max_len, action_dim]

#     return padded_actions

def main():
    # Code to be executed when the script is run directly
    demoFolders = os.listdir(episodesDirectory)  # Gets all file and folder names
    
    demoObservations = []
    demoActions = []

    for folder in demoFolders:
        print("FOLDER: ", folder)
        f = open(episodesDirectory+f'{folder}/demo.txt', "r")
        stateString = f.read()
        states = stateString.split('\n')

        states = states[:-1] #remove empty newline
        
        print("States: ", states)
        times, dones, eef_pos, eef_quat, gripper = parseStates(states)

        # # print(parsedStates)

        # actions = createActions(parsedStates)
        # robotActions = isolateActions(actions)
        # parsedStates = parsedStates[:-1] #remove last unused state

        # observations = addImagesToState(parsedStates, times, folder)

        # demoObservations.append(observations)
        # demoActions.append(robotActions)


    # assert(len(demoObservations) == len(demoActions))

    # encodesObservations = encodeObservation(demoObservations)

    # max_length = encodesObservations[0].shape[0]
    # padded_actions =pad_action_sequences(demoActions, max_length)


    # dataLoaderPath = "state_action_data.pt"
    # saveDataLoader(encodesObservations, padded_actions, dataLoaderPath)
    # dataLoader = loadDataLoader(dataLoaderPath)

    # print("DATA SAVED!")


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