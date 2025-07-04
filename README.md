# GenVista 
> Crafting Diverse Synthetic Worlds for AI Perception.

---

## Overview

GenVista is an open-source, Unity-based framework for generating diverse, richly annotated **synthetic datasets**. It aims to help ML, robotics, and computer vision developers create customizable 3D scenes and simulate sensor outputs to accelerate AI model development. Our mission is to provide a versatile, community-driven tool for a wide array of perception tasks. It has just started, but it will be upgraded from time to time.

## Core Features

*   **Versatile 3D Environments**: Import real-life 3D scenarious, using [CESIUM for Unity](https://cesium.com/learn/unity/).
*   **Customizable Sensors**: Simulate cameras (RGB, depth, segmentation) with extensibility for more. (TBD)
*   **Rich Annotations**: Auto-generate 2D/3D bounding boxes, semantic/instance masks, and depth maps. (TBD)
*   **Domain Randomization**: Tools to vary textures, lighting, and object placements. (TBD)
*   **Unity Powered**: Leverages Unity for high-fidelity rendering and physics.
*   **Scripting & Extensibility**: Full programmatic control and modular design for community contributions.
*   **Detailed Atmosphere**: Thanks to [Tenkoku Dynamic Sky](https://assetstore.unity.com/packages/tools/particles-effects/tenkoku-dynamic-sky-34435), it is simple to edit the weather conditions.

## Getting Started

1.  **Prerequisites**: Unity Hub, Unity Editor (2022.3 LTS+), Git.
2.  **Clone**: `git clone https://github.com/EmanuelCostaS/GenVista.git` 
3.  **Open in Unity**: Use Unity Hub to open the cloned project.

## Showcase

*(To be added.)*

## Potential Applications

*   Generating datasets to train object detectors for autonomous systems (vehicles, drones).
*   Developing perception algorithms for robotics in varied environments.
*   Generating large-scale datasets for computer vision research (segmentation, depth estimation).
*   Prototyping 3D assets and environments for simulations or VR/AR.
*   Supplementing real-world data to improve model generalization.

## Contribution

We welcome community contributions! Please see `CONTRIBUTING.md` (to be added) for guidelines on reporting bugs, suggesting features, and submitting pull requests.

## Assets Used & Acknowledgements

This project utilizes several key assets to achieve its functionality and visual fidelity. We are grateful to the creators for their valuable resources.

### Geospatial Engine & Data

This project utilizes [Cesium for Unity](https://cesium.com/platform/cesium-for-unity/) along with data streamed from [Cesium ion](https://cesium.com/ion/) to integrate real-world 3D geospatial capabilities. We thank the Cesium team for providing these powerful tools.

**Important Usage Note for Cesium Platform:** This project is developed for research and non-commercial purposes, utilizing a Cesium ion Community account. For any commercial use of Cesium ion data and services, a commercial Cesium ion subscription is required in accordance with Cesium's terms of service. Please refer to the [Cesium ion pricing page](https://cesium.com/ion/pricing/) for more details on account types and commercial licensing.

### Dynamic Sky & Weather System

This project utilizes the **[Tenkoku Dynamic Sky](https://assetstore.unity.com/packages/tools/particles-effects/tenkoku-dynamic-sky-34435)** for its dynamic sky, lighting, and weather effects. This is a free asset obtained from **Unity Asset Store**. It is used in accordance with its specified license: **Unity Asset Store EULA for free assets**. We thank **Tanuki Digital** for making this valuable resource freely available.

---

## License

GenVista Engine is released under the Apache License 2.0,
See the `LICENSE` file for details. (TBD)

---

Thank you for your interest in GenVista! If you like it, please leave a star ⭐
