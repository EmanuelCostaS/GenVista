import cv2
import numpy as np
import os

def visualize_yolo_annotations(image_path: str, label_path: str, class_names: list):
    """
    Reads an image and its corresponding YOLO-format annotation file,
    and displays the image with bounding boxes drawn.

    Args:
        image_path (str): Full path to the image file.
        label_path (str): Full path to the YOLO label file (.txt).
        class_names (list): A list of strings where the index corresponds
                            to the class ID (e.g., class_names[0] is the name for ID 0).
    """
    # 1. Load the image
    if not os.path.exists(image_path):
        print(f"Error: Image file not found at {image_path}")
        return
    
    img = cv2.imread(image_path)
    if img is None:
        print(f"Error: Could not load image from {image_path}")
        return

    # Get image dimensions
    H, W, _ = img.shape
    
    print(f"Image loaded with dimensions: W={W}, H={H}")
    
    # 2. Load the annotations
    if not os.path.exists(label_path):
        print(f"Warning: Label file not found at {label_path}. Displaying image without boxes.")
        cv2.imshow("YOLO Visualization", img)
        cv2.waitKey(0)
        cv2.destroyAllWindows()
        return

    annotations = []
    try:
        with open(label_path, 'r') as f:
            for line in f:
                # Format: class_id center_x center_y width height (all normalized to 0-1)
                parts = line.strip().split()
                if len(parts) == 5:
                    annotations.append([float(p) for p in parts])
    except Exception as e:
        print(f"Error reading label file: {e}")
        return

    # 3. Process and draw bounding boxes
    print(f"Found {len(annotations)} annotations.")
    
    for anno in annotations:
        class_id, center_x_norm, center_y_norm, width_norm, height_norm = anno
        class_id = int(class_id)
        
        # Denormalize coordinates to pixel values
        center_x = int(center_x_norm * W)
        center_y = int(center_y_norm * H)
        box_width = int(width_norm * W)
        box_height = int(height_norm * H)
        
        # Convert center/width/height to top-left (x_min, y_min) and bottom-right (x_max, y_max)
        x_min = int(center_x - box_width / 2)
        y_min = int(center_y - box_height / 2)
        x_max = int(center_x + box_width / 2)
        y_max = int(center_y + box_height / 2)

        # Get class name and color
        class_name = class_names[class_id] if class_id < len(class_names) else f"Unknown Class {class_id}"
        
        # Use a consistent color for the box and text (e.g., green in BGR)
        color = (0, 255, 0) 
        thickness = 2

        # Draw the rectangle
        cv2.rectangle(img, (x_min, y_min), (x_max, y_max), color, thickness)

        # Put the class label text above the box
        label = f"{class_name}"
        font = cv2.FONT_HERSHEY_SIMPLEX
        font_scale = 0.6
        font_thickness = 1
        
        # Determine text size to make a background rectangle for better visibility
        (text_width, text_height), baseline = cv2.getTextSize(label, font, font_scale, font_thickness)
        
        # Draw a filled rectangle for the label background
        cv2.rectangle(img, (x_min, y_min - text_height - 5), (x_min + text_width, y_min), color, -1)
        
        # Put the text on top of the background
        cv2.putText(img, label, (x_min, y_min - 5), font, font_scale, (0, 0, 0), font_thickness, cv2.LINE_AA)
        
    # 4. Display the image
    window_name = "YOLO Annotation Visualization"
    
    # Resize for comfortable viewing if the image is too large
    if W > 1200 or H > 900:
        scale_percent = min(1200 / W, 900 / H)
        new_w = int(W * scale_percent)
        new_h = int(H * scale_percent)
        img = cv2.resize(img, (new_w, new_h), interpolation=cv2.INTER_AREA)

    cv2.imshow(window_name, img)
    # Wait for a key press to close the window
    print("Press any key to close the visualization window...")
    cv2.waitKey(0)
    cv2.destroyAllWindows()


# **IMPORTANT: REPLACE THESE PATHS WITH YOUR ACTUAL FILE LOCATIONS**
image_path = r'/home/navms1/Documentos/APD/GenVista/Dataset/images/image_20260121_155258405.png' # Ensure this is a valid image file (.jpg, .png, etc.)
label_path = r'/home/navms1/Documentos/APD/GenVista/Dataset/labels/image_20260121_155258405.txt' # Your label file

# **IMPORTANT: DEFINE YOUR CLASS NAMES IN ORDER OF THEIR ID (0, 1, 2, ...)**
# For example, if class ID 0 is 'cat' and class ID 1 is 'dog'.
class_names = [
    "RedBall"
]

# --- Run the visualization ---
visualize_yolo_annotations(image_path, label_path, class_names)