# AR Face Filter Application

## Project Overview
This Unity AR Face Filter application provides an immersive augmented reality experience with multiple face filters, smooth UI animations, and interactive features.


## Features
✅ Multi-screen UI with animated transitions
✅ Background music with toggle control
✅ 3+ Face filters (Glasses, Mask, Face Paint)
✅ Intuitive filter selection menu
✅ Snapshot/Screenshot functionality
✅ Smooth scene transitions
✅ AR Foundation-based face tracking

## Technical Stack
- **Unity Version:** 2022.3 LTS or higher
- **AR Framework:** AR Foundation 5.x
- **Face Tracking:** ARKit Face Tracking (iOS) / ARCore Face Mesh (Android)
- **Programming Language:** C# (.NET)


## Setup Instructions

### Prerequisites
1. Install Unity Hub
2. Install Unity 2022.3 LTS or higher
3. Install Android Build Support or iOS Build Support module

### Installation Steps

1. **Clone the Repository**
```bash
git clone https://github.com/DUSHIME1212/Face-Filter.git
cd Face-Filter
```

2. **Open in Unity**
   - Open Unity Hub
   - Click "Add" and select the project folder
   - Open the project

3. **Install Required Packages**
   - Open Package Manager (Window > Package Manager)
   - Install the following:
     - AR Foundation (5.x)
     - ARKit XR Plugin (iOS) or ARCore XR Plugin (Android)
     - XR Plugin Management

4. **Configure XR Settings**
   - Edit > Project Settings > XR Plug-in Management
   - Enable ARKit (iOS) or ARCore (Android)
   - Go to AR Foundation settings
   - Ensure Face Tracking is enabled

5. **Build Settings**
   - File > Build Settings
   - Add all scenes in order:
     1. WelcomeScene
     2. AppInfoScene
     3. ARFaceFilterScene
   - Select your target platform (iOS/Android)
   - Click "Switch Platform"

### Android-Specific Setup
1. Set minimum API level to 24 or higher
2. Enable "ARCore Supported" in XR Settings
3. Ensure camera permissions in AndroidManifest.xml

### iOS-Specific Setup
1. Set minimum iOS version to 12.0 or higher
2. Enable "ARKit Face Tracking" in XR Settings
3. Add camera usage description in Info.plist

## Building the APK

### For Android
```
1. File > Build Settings
2. Select Android platform
3. Player Settings:
   - Set Company Name
   - Set Product Name
   - Set Package Name (com.yourname.facefilter)
   - Set Minimum API Level: Android 7.0 (API 24)
4. Build Settings > Build
5. Choose output location
6. Wait for build to complete
```

### Testing
- Deploy to physical device (AR features require device camera)
- Emulators do not support AR face tracking

## Usage Guide

### Welcome Screen
- App launches with animated welcome screen
- Background music plays automatically
- Music toggle button in top corner
- "Get Started" button navigates to App Info

### App Info Screen
- Displays student information
- Shows app features and description
- "Start AR Experience" button launches face filter

### AR Face Filter Scene
- Camera activates and detects face
- Filter menu appears at bottom
- Tap filter icons to switch between filters
- Camera button takes screenshots
- Back button returns to homepage

## Additional Features Implemented

1. **Screenshot Functionality**
   - Capture current AR view
   - Save to device gallery
   - Visual feedback on capture

2. **Filter Animations**
   - Smooth fade in/out on filter switch
   - Scale animations on selection
   - Particle effects on certain filters

## Known Limitations
- Requires device with ARKit/ARCore support
- Best performance on devices with depth camera
- Lighting conditions affect tracking quality

## Troubleshooting

**Face not tracking:**
- Ensure good lighting
- Check camera permissions
- Verify AR Foundation packages installed

**Music not playing:**
- Check audio files in Resources folder
- Verify AudioSource component settings

**Build errors:**
- Clear Library folder and reimport
- Verify all packages are up to date
- Check platform-specific requirements

## Credits
- AR Foundation by Unity Technologies
- Face tracking powered by ARKit/ARCore
- UI animations using DOTween (optional)

