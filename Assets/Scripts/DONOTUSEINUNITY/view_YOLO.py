import cv2
import numpy as np
import os

def visualize_yolo(image_path, label_path, class_names, max_display_size=(1600, 900)):
    """
    Visualizes YOLO bounding boxes on an image with a resizable window.

    Args:
        image_path (str): Path to the image file.
        label_path (str): Path to the YOLO label file.
        class_names (list): A list of strings representing the class names.
        max_display_size (tuple): A tuple (max_width, max_height) for the initial display.
    """
    # --- 1. Read the Image ---
    try:
        image = cv2.imread(image_path)
        if image is None:
            print(f"Error: Could not read image at {image_path}")
            return
        height, width, _ = image.shape
    except Exception as e:
        print(f"An error occurred while reading the image: {e}")
        return

    # --- 2. Read the YOLO Annotation File ---
    try:
        with open(label_path, 'r') as f:
            lines = f.readlines()
    except FileNotFoundError:
        print(f"Error: Label file not found at {label_path}")
        return
    except Exception as e:
        print(f"An error occurred while reading the label file: {e}")
        return

    # --- 3. Parse and Draw Bounding Boxes ---
    for line in lines:
        parts = line.strip().split()
        if len(parts) == 5:
            class_id, x_center, y_center, box_width, box_height = map(float, parts)
            class_id = int(class_id)

            # --- 4. Convert YOLO coordinates to pixel coordinates ---
            x_center_px = int(x_center * width)
            y_center_px = int(y_center * height)
            box_width_px = int(box_width * width)
            box_height_px = int(box_height * height)

            x_min = int(x_center_px - (box_width_px / 2))
            y_min = int(y_center_px - (box_height_px / 2))
            x_max = int(x_center_px + (box_width_px / 2))
            y_max = int(y_center_px + (box_height_px / 2))

            # --- 5. Draw the Bounding Box and Label ---
            if class_id < len(class_names):
                class_name = class_names[class_id]
                color = (0, 255, 0) # Green
                cv2.rectangle(image, (x_min, y_min), (x_max, y_max), color, 2)

                label = f'{class_name}'
                (label_width, label_height), baseline = cv2.getTextSize(label, cv2.FONT_HERSHEY_SIMPLEX, 0.5, 2)
                cv2.rectangle(image, (x_min, y_min - label_height - 10), (x_min + label_width, y_min), color, -1)
                cv2.putText(image, label, (x_min, y_min - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
            else:
                print(f"Warning: class_id {class_id} is out of range for the provided class_names.")

    # --- 6. CHANGE START: Create a resizable window and scale the image if it's too large ---
    
    # Create a window that can be resized by the user
    window_name = 'YOLO Visualization'
    cv2.namedWindow(window_name, cv2.WINDOW_NORMAL)

    # Get the screen-friendly dimensions
    display_height, display_width = image.shape[:2]
    max_height, max_width = max_display_size[1], max_display_size[0]

    # Check if the image is larger than the max display size
    if display_height > max_height or display_width > max_width:
        # Calculate the ratio to scale down the image
        ratio = min(max_width / display_width, max_height / display_height)
        # Set new dimensions while maintaining aspect ratio
        new_dims = (int(display_width * ratio), int(display_height * ratio))
        # Resize the image for display
        display_image = cv2.resize(image, new_dims, interpolation=cv2.INTER_AREA)
    else:
        display_image = image

    # Display the (potentially resized) image
    cv2.imshow(window_name, display_image)
    # --- CHANGE END ---
    
    cv2.waitKey(0)
    cv2.destroyAllWindows()

if __name__ == '__main__':
    # --- User Configuration ---
    # Path to the specific image you want to visualize
    image_path = r'C:\Users\edusa\APD\YOLO_Dataset\images\image_20250612_150253226.png'

    # Path to the corresponding YOLO label file
    label_path = r'C:\Users\edusa\APD\YOLO_Dataset\labels\image_20250612_150253226.txt'
    
    # List of your class names
    class_names = ['RedBall'] 

    # --- CHANGE: Set the maximum initial size for the display window (width, height) ---
    # The window will still be resizable if you want to make it bigger or smaller.
    max_window_size = (1280, 720)

    # --- Run the visualization function ---
    try:
        print(f"Attempting to visualize image: {image_path}")
        # Pass the max window size to the function
        visualize_yolo(image_path, label_path, class_names, max_window_size)
    except Exception as e:
        print(f"An unexpected error occurred: {e}")