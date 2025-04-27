
from PIL import Image
import numpy as np
import os
import base64
from io import BytesIO


import sys


    

def parseImage(image):

    img = base64.b64decode(image)
    image = Image.open(BytesIO(img))

    # Ensure the image is in RGB format (3 channels)
    image = image.convert("RGB")
    imageArray = np.array(image)  # Converts to (H, W, C) = (84, 84, 3)
    return imageArray

def parseState(state):
    stringsList = state.split(",")
    parsedState = []
    for string in stringsList:
        parsedState.append(float(string.split(':')[1]))

    times = parsedState[0]
    eef_pos = [parsedState[1], parsedState[2], parsedState[3]]
    eef_quat = [parsedState[4], parsedState[5], parsedState[6], parsedState[7]]
    eef_rot = [parsedState[8], parsedState[9], parsedState[10]]
    gripper = [parsedState[11]]
    dones = parsedState[12]

    return times, dones, eef_pos, eef_quat, eef_rot, gripper

# def main():
#     statesString = sys.argv[1]

#     states = statesString.split('~')

#     times, dones, eef_pos, eef_quat, eef_rot, gripper = parseState(states[0])
#     stateData = [eef_pos, eef_rot, gripper]

#     img1 = parseImage(states[1])
#     img2 = parseImage(states[2])
#     imgs = [img1, img2]

    
#     select_obs=np.hstack(stateData).reshape(1, -1)
    
#     image_obs = np.hstack([
#         np.array(img).reshape(1, -1)
#         for img in imgs
#     ])

#     obs = np.hstack([select_obs, image_obs])
#     obs_tensor = torch.tensor(obs, dtype=torch.float32)

#     model = torch.load('C:/Users/Devin/CS933 Project/Assets/PythonScripts/bc_square.pth', weights_only = False)
#     model.eval()
#     with torch.no_grad():
#         out = model(obs_tensor)

#     print( str(out[0].tolist()))

# if __name__ == "__main__":
#     main()
