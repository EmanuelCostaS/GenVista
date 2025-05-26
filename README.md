# SkyEyeSim - Synthetic Obstacle Detection Dataset Generator for Airborne Systems

---

This Unity project provides a framework for generating **synthetic image datasets** that can be used for, but not limited to, diverse computer vision tasks on aerial environment and simulations. Designed with machine learning applications in mind, it facilitates the creation of diverse visual data to train robust computer vision models, such as those used in aerial navigation.

## Why use this project?

Training reliable object detection models for airborne systems requires vast amounts of varied data, which can be expensive and time-consuming to collect in the real world. This repository offers a solution by:

* **Automating image capture:** Efficiently generate thousands of images from a simulated aerial perspective.
* **Controlling environmental variables:** Easily randomize lighting conditions, backgrounds, and the placement of obstacles.
* **Generating ground truth:** Automatically produce precise bounding box annotations for all detected obstacles, formatted for popular object detection frameworks like YOLO.
* **Enabling diverse scenarios:** Simulate a wide range of operational conditions, from varying altitudes and angles to different times of day and atmospheric effects.

## Getting Started

1.  **Clone this repository:** `git clone https://github.com/EmanuelCostaS/SkyEyeSim.git`
2.  **Open in Unity:** Navigate to the cloned project in the Unity Hub and open it.
3.  **Explore the `DatasetGenerator` script:** Attach this script to an empty GameObject in your scene to configure the dataset generation process. Adjust parameters for image count, randomization ranges for obstacles and camera positions, and output folders.
4.  **Add your own obstacles:** Populate the scene with various 3D models you wish to detect. Ensure these models have a `Renderer` component for bounding box calculation.
5.  **Run the scene:** The project will automatically generate images and corresponding label files into your specified output directory.

---

This tool aims to simplify the creation of high-quality synthetic datasets, accelerating the development and testing of vision systems for airborne platforms.
