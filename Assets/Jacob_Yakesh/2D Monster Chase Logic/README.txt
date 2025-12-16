# 2D Monster Chase Logic

## Overview
This project provides a sophisticated AI system for a monster in 2D horror games. The monster is designed to perceive the player's actions and react unpredictably, creating tension and challenge. The AI includes features such as pathfinding, dynamic behavior, sound detection, and teleportation. The system is highly configurable and optimized for smooth performance on various devices.

## IMPORTANT
This package requires `A* Pathfinding` (Aron Granberg) to work. Upon downloading this package, you also have to download (free version is sufficient) A* Pathfinding and import it into the project.
You can download A* Pathfinding here: https://arongranberg.com/astar/download


## Features
- **Pathfinding:** Utilizes the A* algorithm for efficient navigation in complex environments.
- **Dynamic Behavior:** The monster can patrol, chase, and lose track of the player based on visibility and sound.
- **Sound Detection:** Reacts to player footsteps, especially when sprinting.
- **Teleportation:** Can teleport to alternative targets near the player under certain conditions.
- **Customizable Parameters:** Easily adjust behavior through the Unity Inspector (e.g., speed, detection range, teleport chance).

## Installation
1. **Download the Package**

2. **Import into Unity:**
   - Drag and drop or import the downloaded package into your Unity project.
   - Ensure you are using Unity version **2022.3.21f1** or later for compatibility.
   - The AIAgent.cs script is commented, in order to not cause compilation errors, before importing A* Pathfinding. Uncomment the code for the project to work.

3. **Download and Import A* Pathfinding**
   - Download A* Pathfinding: https://arongranberg.com/astar/download
   - Import it into your Unity project
   - Place the `AstarPathfindingProject` folder into the `2D Monster Chase Logic`
   - You may get an `InvalidOperationException: Insecure connection not allowed` error message. If so, navigate to `AstarUpdateChecker.cs` and comment the following code:
   /*
      #if UNITY_2018_1_OR_NEWER
               updateCheckDownload = UnityWebRequest.Get(query);
               updateCheckDownload.SendWebRequest();
      #else
               updateCheckDownload = new WWW(query);
      #endif
               lastUpdateCheck = System.DateTime.UtcNow;
   */

   Now everything should be functional and you can open the MainScene to test out the functionality in an independent enviroment.


4. **How to use in your scene:**
   - Add the `Monster` prefab from the `Prefabs` folder to your scene.
   - Ensure the player object is present in the scene and assign it to the `Player` field in the `AiAgent` script.
   - Place alternative targets (empty GameObjects or use the altTarget prefab) around the scene and assign them to the `Alternative Targets` array in the `AiAgent` script.
       
     
5. **Set up A Pathfinding**
     - Create an empty Game Object and add the script `Pathdinder`.
     - Add Grid Graph.
     - In the inspector, click on the Scan button.
     - Make sure that the monster has AIPath script added.  

6. **Configure the AI:**
   - Adjust parameters in the `AiAgent` script via the Unity Inspector to customize the monster's behavior.

## Usage
### Monster Behavior
- **Patrolling:** The monster moves between alternative targets when not chasing the player.
- **Chasing:** If the player is detected (visually or audibly), the monster will chase them at increased speed.
- **Teleportation:** The monster can teleport to the nearest alternative target if the player is audible but not visible.

### Customization
- **Speed:** Adjust `Base Speed` and `Chase Speed Multiplier` to control movement speed.
- **Detection:** Modify `FOV Angle`, `FOV Range`, and `Sprint Detection Range` to change how the monster detects the player.
- **Teleportation:** Set `Teleport Chance` and `Teleport Retry Delay` to control teleportation behavior.

### Integration
- **Player Setup:** Ensure your player object has a `Player` script with a `IsSprinting` boolean to control sprinting behavior.
- **Camera:** Use the `CameraFollow` script to create a dynamic camera that follows the player and zooms out during inactivity.

## Example Scene
An example scene (`MainScene`) is included to demonstrate the monster's behavior. It features a maze layout, player controls, and pre-configured monster settings.

## Troubleshooting
- **Monster Not Moving:** Ensure the `AIPath` script is attached to the monster and the `Grid Graph` is properly scanned.
- **Player Not Detected:** Verify the `Player` field is assigned in the `AiAgent` script and the player is within the detection range.
- **Teleportation Issues:** Check the `Alternative Targets` array and ensure the `Teleport Chance` is set correctly.

## Credits
- **Developer:** Jakub Jakeš
- **Tools Used:** Unity Engine, A* Pathfinding Project (Aron Granberg)

---

Thank you for using this asset! We hope it enhances your 2D horror game development experience.