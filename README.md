# 🧟 ZombieHitFrenzy

A fun Android game where you drive a car and hit zombies before the time runs out!

---

## 🎮 What is This Game?

ZombieHitFrenzy is a top-down car game for Android phones.
You look at the game from above, like a bird looking down.
You swipe your finger on the screen to move the car.
Your goal is to hit as many zombies as possible in 60 seconds.

---

## 📱 How to Play

- **Swipe UP** → Car goes forward
- **Swipe DOWN** → Car brakes, then goes reverse
- **Swipe LEFT** → Car turns left
- **Swipe RIGHT** → Car turns right
- **Release finger** → Car slowly stops

Hit zombies with your car to get points.
The game ends when the timer reaches zero.
Try to get the highest score you can!

---

## 🖥️ What You See on Screen

| Item | Position | Description |
|------|----------|-------------|
| Score | Top Left | How many zombies you hit |
| Timer | Top Right | How many seconds are left |
| FPS | Top Center | How smooth the game is running |

When the timer hits 0, the game shows your final score.
Tap the **Restart** button to play again.

---

## 🛠️ How This Game Was Made

| Tool | What It Is Used For |
|------|---------------------|
| Unity 6.3 LTS | Game engine used to build the game |
| C# | Programming language used to write the scripts |
| Android | The platform this game runs on |
| Prometeo Car Controller | Asset used to make the car move |
| Supercyan Zombie Pack | Asset used for the zombie characters |
| TextMeshPro | Used to show text on screen |
| Unity NavMesh | Used to make zombies walk toward the car |
| New Input System | Used to read touch and keyboard input |

---

## 📁 Project Folder Structure

```
ZombieHitFrenzy/
│
├── Assets/
│   ├── Scripts/                  ← All game scripts written for this project
│   │   ├── GameManager.cs        ← Controls score, timer, and game state
│   │   ├── SwipeInputController.cs ← Reads finger swipe on screen
│   │   ├── CarControllerBridge.cs  ← Connects swipe input to the car
│   │   ├── ZombieController.cs   ← Makes zombies walk toward the car
│   │   ├── ZombieSpawner.cs      ← Spawns zombies in the arena
│   │   ├── TopDownCameraFollow.cs ← Makes camera follow the car from above
│   │   └── FPSCounter.cs         ← Shows FPS on screen
│   │
│   ├── PROMETEO - Car Controller/ ← Car asset from Unity Asset Store
│   ├── Supercyan Character Pack Zombie Sample/ ← Zombie asset
│   └── TextMesh Pro/             ← Text display package
│
├── ProjectSettings/              ← Unity project settings
└── Packages/                     ← Unity packages list
```

---

## ⚙️ Scripts Explained

### GameManager.cs
This is the main brain of the game.
- Counts down the 60 second timer
- Shows the score on screen
- Shows FPS on screen
- Hides the HUD and shows the End Screen when time is up
- Has a Restart button that reloads the game

### SwipeInputController.cs
This reads your finger movement on the phone screen.
- Drag up = go forward
- Drag down = brake then reverse
- Drag left or right = steer
- Uses Unity's New Input System

### CarControllerBridge.cs
This connects your swipe input to the Prometeo Car Controller asset.
- Calls GoForward(), GoReverse(), Brakes(), TurnLeft(), TurnRight()
- Uses Reflection to talk to Prometeo without editing its code too much

### ZombieController.cs
This controls each zombie.
- Zombie walks toward the player car using NavMesh
- When the car hits the zombie, it falls down (ragdoll)
- Score goes up by 1 for each zombie hit

### ZombieSpawner.cs
This puts zombies into the game arena.
- Keeps at least 10 zombies alive at all times
- Spawns new zombie when one dies
- Places zombies randomly inside the arena

### TopDownCameraFollow.cs
This moves the camera to always look at the car from above.
- Camera stays directly above the car
- Smooth follow so it does not jump around

---

## 📲 How to Install on Android Phone

1. Download the **ZombieHitFrenzy.apk** file
2. Send it to your phone by USB cable, WhatsApp, or Google Drive
3. On your phone, go to **Settings → Install Unknown Apps**
4. Allow your File Manager to install unknown apps
5. Open the APK file on your phone
6. Tap **Install**
7. Open the game and play!

---

## 🔧 How to Open in Unity (For Developers)

1. Download and install **Unity Hub** from https://unity.com
2. Install **Unity 6.3 LTS** through Unity Hub
3. When installing, also add:
   - Android Build Support
   - Android SDK and NDK Tools
   - OpenJDK
4. Clone this repository:
```
git clone https://github.com/YourUsername/ZombieHitFrenzy.git
```
5. Open Unity Hub → Click **Open** → Select the project folder
6. Wait for Unity to load the project
7. Open the **GameScene** from the Assets folder
8. Press the **Play** button to test in editor

---

## 🏗️ How to Build the APK

1. Open the project in Unity
2. Go to **File → Build Profiles**
3. Select **Android** platform
4. Click **Player Settings** and set:
   - Orientation: Portrait
   - Minimum API Level: Android 8.0 (API 26)
   - Scripting Backend: IL2CPP
   - Target Architecture: ARM64
5. Click **Build**
6. Choose a folder to save the APK
7. Wait 5-10 minutes for the build to finish

---

## 🎯 Game Settings

| Setting | Value |
|---------|-------|
| Game Duration | 60 seconds |
| Minimum Zombies | 10 at all times |
| Arena Size | 40 x 40 units |
| Target FPS | 60 frames per second |
| Minimum Android Version | Android 8.0 (API 26) |
| Screen Orientation | Portrait only |

---

## 🐛 Known Issues

- FPS counter takes 1-2 seconds to show a number after the game starts
- Zombies sometimes overlap each other when spawning close together
- Car may slide a little after releasing the finger — this is normal physics

---

## 🙏 Credits

| Asset / Tool | Creator |
|--------------|---------|
| Prometeo Car Controller | Mena (Unity Asset Store) |
| Supercyan Zombie Pack | Supercyan (Unity Asset Store) |
| Unity Engine | Unity Technologies |
| Game Scripts | Made for ZombieHitFrenzy project |

---

## 📄 License

This project was made for learning and personal use.
The Prometeo Car Controller and Supercyan Zombie Pack assets belong to their original creators.
Do not sell or republish those assets separately.

---

## 📬 Contact

If you find a bug or have a question, open an **Issue** on this GitHub page.

---

*Made with Unity 6.3 LTS — Have fun hitting zombies! 🧟🚗*
