import h5py
import numpy as np


def findDemos(demos, path):
    foundDemos = []
    def explore_hdf5(name, obj):
        global count

        # print(f"\nPath: {name}")
        path_items = name.split('/')
        demo_name = path_items[len(path_items)-1]

        if((demo_name.split('_')[0] == 'demo') and any(sub == demo_name for sub in demos)):
           
            if obj.attrs:

                foundDemos.append(obj.attrs['description'])

    with h5py.File(path, "r") as f:
        print("Exploring HDF5 structure...")
        f.visititems(explore_hdf5)

    return foundDemos

# Path to your file
file_path = "C:/Users/Devin/Desktop/demos/dataStack.hdf5"
# file_path = "C:/Users/Devin/Desktop/demos/dataSquare.hdf5"

# stack output
demos = ['demo_11']

# square output
# demos = ['demo_12', 'demo_13', 'demo_14', 'demo_16', 'demo_17', 'demo_31', 'demo_32', 'demo_34', 'demo_35', 'demo_39', 'demo_52', 'demo_53', 'demo_55', 'demo_56', 'demo_57', 'demo_58', 'demo_59', 'demo_60', 'demo_73', 'demo_93', 'demo_97', 'demo_111', 'demo_112', 'demo_113', 'demo_115', 'demo_117', 'demo_119', 'demo_120', 'demo_137', 'demo_152', 'demo_155', 'demo_156', 'demo_157', 'demo_159', 'demo_160', 'demo_172', 'demo_192', 'demo_196', 'demo_197', 'demo_198', 'demo_199']


paths = findDemos(demos, file_path)
print("P: ", paths)

